using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class FreeStyleMainScroll : ScrollBase
{
    public SongUI prefab;
    public FreeStyleSearch search;

    private RectTransform rt => transform as RectTransform;
    private Vector2 contentOriginPos;
    private bool HasAnySongs => Length != 0;

    [Header("Scroll")]
    public int maxShowCount = 7;

    private int median;
    private float curPos;
    private float size;

    [Header( "Count Text" )]
    public TextMeshProUGUI maxText;
    public TextMeshProUGUI curText;

    public Scene CurrentScene { get; private set; }
    [Header( "Scene" )]
    private LinkedList<SongUI> songs = new LinkedList<SongUI>();
    private LinkedListNode<SongUI> medianNode;
    private CustomVerticalLayoutGroup group;

    [Header("Time")]
    private bool isEnd;
    private readonly float ScrollUpdateTime = .075f;
    private readonly float KeyHoldWaitTime  = .5f;
    private readonly uint  FadeDuration     = 2500; // ms
    private float fadeStartPos;
    private float keyPressTime;
    private bool  isKeyDown;
    public  static double Playback;
    private float endTime; // 마지막 노트의 처리시간
    private Coroutine corVolumeFade;

    [Header("Contents")]
    public GameObject noContents;

    private Song curSong;
    public event Action<Song>  OnSelectSong;
    public event Action<Song>  OnSoundRestart;

    #region Unity Callback
    private void Awake()
    {
        IsLoop = true;

        CurrentScene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Scene>();
        group = GetComponent<CustomVerticalLayoutGroup>();

        if ( !DataStorage.IsMultiPlaying )
        {
            AudioManager.Inst.OnReload += OnBufferSetting;
            search.OnSearch += UpdateLayoutAndSong;
        }

        median = Mathf.FloorToInt( maxShowCount / 2f );
        contentOriginPos = rt.anchoredPosition;

        KeyBind();

        // 객체 할당
        for ( int i = 0; i < maxShowCount; i++ )
        {
            var song = Instantiate( prefab, transform );
            song.gameObject.SetActive( false );
            songs.AddLast( song );
        }
    }

    public void UpdateScrollView()
    {
        Length = NowPlaying.Inst.Songs.Count;

        noContents.SetActive( !HasAnySongs );
        if ( HasAnySongs )
        {
            UpdateSongElements();
            UpdateSong();
        }
    }

    private void Start()
    {
        UpdateScrollView();

        if ( !HasAnySongs )
        {
            AudioManager.Inst.AllStop();
            AudioManager.Inst.Load( $@"{Application.streamingAssetsPath}\\Default\\Sounds\\Bgm\\Hana.mp3", true, false );
            AudioManager.Inst.Play( 0f );
            AudioManager.Inst.Position = 160000;
            AudioManager.Inst.FadeVolume( 0f, 1f, .5f );
        }
    }

    private void Update()
    {
        if ( !HasAnySongs ) return;

        Playback += ( Time.deltaTime * 1000f ) * GameSetting.CurrentPitch;

        if ( !isEnd && fadeStartPos < Playback )
        {
            isEnd = true;
            if ( !ReferenceEquals( corVolumeFade, null ) )
            {
                AudioManager.Inst.StopCoroutine( corVolumeFade );
                corVolumeFade = null;
            }

            corVolumeFade = AudioManager.Inst.FadeVolume( new Music( AudioManager.Inst.MainSound, AudioManager.Inst.MainChannel ), curSong.volume * .01f, 0f, FadeDuration * .001f, () => 
            {
                AudioManager.Inst.Play( 0f );
                AudioManager.Inst.Position = ( uint )curSong.previewTime;
                AudioManager.Inst.FadeVolume( new Music( AudioManager.Inst.MainSound, AudioManager.Inst.MainChannel ), 0f, curSong.volume * .01f, FadeDuration * .5f * .001f );
                Playback = curSong.previewTime;
                OnSoundRestart?.Invoke( curSong );
                isEnd = false;
            }, .5f );
        }
    }

    private void UpdateSong()
    {
        // 현재 음악 페이드아웃
        Music curMusic = new Music( AudioManager.Inst.MainSound, AudioManager.Inst.MainChannel );
        if ( isEnd && !ReferenceEquals( corVolumeFade, null ) )
        {
            AudioManager.Inst.StopCoroutine( corVolumeFade );
            corVolumeFade = null;
        }
        corVolumeFade = AudioManager.Inst.FadeVolume( curMusic, curSong.volume * .01f, 0f, .5f, () => AudioManager.Inst.Release( curMusic ) );

        // 새로운 음악 로딩
        NowPlaying.Inst.UpdateSong( CurrentIndex );
        curSong = NowPlaying.CurrentSong;

        AudioManager.Inst.Load( curSong.audioPath, false, true );
        endTime = curSong.totalTime;
        curSong.previewTime = ( int )GetPreviewTime( curSong.previewTime );
        Playback = curSong.previewTime;

        float diff = AudioManager.Inst.Length - endTime;
        fadeStartPos = diff > FadeDuration ? endTime : AudioManager.Inst.Length - FadeDuration;
        isEnd = false;

        // 음악 재생 및 페이드인
        AudioManager.Inst.Play( 0f );
        AudioManager.Inst.Position = ( uint )curSong.previewTime;
        OnSelectSong?.Invoke( curSong );
        AudioManager.Inst.FadeVolume( new Music( AudioManager.Inst.MainSound, AudioManager.Inst.MainChannel ), 0f, curSong.volume * .01f, .5f );
    }

    private void OnDestroy()
    {
        AudioManager.Inst.OnReload -= OnBufferSetting;
    }
    #endregion

    #region Update Song & Scroll
    private void UpdateSongElements()
    {
        Length = NowPlaying.Inst.Songs.Count;

        // 이전 UI 이펙트 초기화
        int medianCounts = 0;
        medianNode?.Value.Select( false );
        medianNode = songs.First;
        Select( NowPlaying.CurrentIndex );
        int index = CurrentIndex - median < 0 ? Length - ( Global.Math.Abs( CurrentIndex - median + 1 ) % Length ) - 1 :
                                                ( CurrentIndex - median ) % Length;

        foreach ( var song in songs )
        {
            if ( medianCounts < median )
            {
                medianNode = medianNode.Next;
                medianCounts++;
            }

            song.gameObject.SetActive( HasAnySongs );
            song.SetInfo( NowPlaying.Inst.Songs[index] );
            song.PositionReset();

            index = index + 1 < Length ? index + 1 : 0;
        }

        // 레이아웃 갱신
        group.Initialize();
        group.SetLayoutVertical();

        size = medianNode.Value.rt.sizeDelta.y + group.spacing;

        // Count Text
        maxText.text = $"{Length}";
        curText.text = $"{CurrentIndex + 1}";

        medianNode.Value.Select( true );
        rt.anchoredPosition = contentOriginPos;
        curPos = contentOriginPos.y;

        UpdateNodePositionX();
    }

    private void UpdateNodePositionX()
    {
        medianNode.Value.MoveX( 0f );
        int count = 1;
        var prevNode = medianNode.Previous;
        var nextNode = medianNode.Next;

        while ( prevNode is not null && nextNode is not null )
        {
            if ( prevNode is not null )
            {
                prevNode.Value.MoveX( 50f * count );
                prevNode = prevNode.Previous;
            }

            if ( nextNode is not null )
            {
                nextNode.Value.MoveX( 50f * count );
                nextNode = nextNode.Next;
            }

            count++;
        }
    }

    private void UpdateLayoutAndSong()
    {
        UpdateSongElements();

        if ( curSong.index != NowPlaying.CurrentIndex )
             UpdateSong();
    }

    public override void PrevMove()
    {
        base.PrevMove();
        // 객체 위치 스왑
        var first = songs.First.Value;
        var last  = songs.Last.Value;
        last.rt.anchoredPosition = new Vector2( first.rt.anchoredPosition.x, first.rt.anchoredPosition.y + size );

        // Song 정보 수정
        int infoIndex = CurrentIndex - median < 0 ? Length - ( Global.Math.Abs( CurrentIndex - median + 1 ) % Length ) - 1 :
                                                    ( CurrentIndex - median ) % Length;
        last.SetInfo( NowPlaying.Inst.Songs[infoIndex] );

        // 노드 이동
        songs.RemoveLast();
        songs.AddFirst( last );
        last.rt.SetAsFirstSibling();

        // 위치 갱신
        medianNode.Value.Select( false );
        medianNode = medianNode.Previous;
        medianNode.Value.Select( true );

        curPos -= size;
        rt.DOAnchorPosY( curPos, .3f );

        UpdateNodePositionX();

        curText.text = $"{CurrentIndex + 1}";
    }

    public override void NextMove()
    {
        base.NextMove();
        // 객체 위치 스왑
        var first = songs.First.Value;
        var last  = songs.Last.Value;
        first.rt.anchoredPosition = new Vector2( last.rt.anchoredPosition.x, last.rt.anchoredPosition.y - size );

        // Song 정보 수정
        int infoIndex = ( CurrentIndex + median ) % Length;
        first.SetInfo( NowPlaying.Inst.Songs[infoIndex] );

        // 노드 이동
        songs.RemoveFirst();
        songs.AddLast( first );
        first.rt.SetAsLastSibling();

        // 위치 갱신
        medianNode.Value.Select( false );
        medianNode = medianNode.Next;
        medianNode.Value.Select( true );

        curPos += size;
        rt.DOAnchorPosY( curPos, .3f );

        UpdateNodePositionX();

        curText.text = $"{CurrentIndex + 1}";
    }

    private void OnBufferSetting()
    {
        AudioManager.Inst.Load( curSong.audioPath, false, true );
        AudioManager.Inst.Play();
        AudioManager.Inst.Position = ( uint )Playback;
    }

    private uint GetPreviewTime( int _time ) => _time > endTime || _time <= 0 ? ( uint )( endTime * .35f ) : ( uint )_time;
    #endregion

    #region Input
    private void SelectChart()
    {
        if ( !HasAnySongs ) return;

        GameSetting.NoteSizeMultiplier = NowPlaying.CurrentSong.keyCount == 4 ? 1.25f : 1f;

        AudioManager.Inst.Play( SFX.MainClick );
        CurrentScene.LoadScene( SceneType.Game );
    }

    private void ScrollDown()
    {
        if ( !HasAnySongs ) return;

        AudioManager.Inst.Play( SFX.MainSelect );
        PrevMove();

        if ( !isKeyDown )
        {
            isKeyDown = true;
            keyPressTime = 0f;
        }
    }

    private void ScrollUp()
    {
        if ( !HasAnySongs ) return;

        AudioManager.Inst.Play( SFX.MainSelect );
        NextMove();

        if ( !isKeyDown )
        {
            isKeyDown = true;
            keyPressTime = 0f;
        }
    }

    private void KeyHold( Action _action )
    {
        if ( !HasAnySongs ) return;

        keyPressTime += Time.deltaTime;
        if ( keyPressTime >= KeyHoldWaitTime + ScrollUpdateTime )
        {
            keyPressTime = KeyHoldWaitTime;
            _action?.Invoke();
        }
    }

    private void KeyUp()
    {
        if ( !HasAnySongs ) return;

        if ( NowPlaying.CurrentIndex != CurrentIndex )
             UpdateSong();

        isKeyDown = false;
        keyPressTime = 0f;
    }
    #endregion

    public void KeyBind()
    {
        CurrentScene.Bind( ActionType.Main, KeyCode.Return, SelectChart );

        CurrentScene.Bind( ActionType.Main, KeyState.Down, KeyCode.UpArrow, ScrollDown );
        CurrentScene.Bind( ActionType.Main, KeyState.Down, KeyCode.DownArrow, ScrollUp );

        // 지연시간 이후 일정시간마다 델리게이트 실행 ( Hold 시 0.5초 이후부터 빠르게 스크롤 )
        CurrentScene.Bind( ActionType.Main, KeyState.Hold, KeyCode.UpArrow, () => KeyHold( ScrollDown ) );
        CurrentScene.Bind( ActionType.Main, KeyState.Hold, KeyCode.DownArrow, () => KeyHold( ScrollUp ) );

        // 재고있던 스크롤 시간 초기화 및 비활성화 + 채보변경 타이머 시작
        CurrentScene.Bind( ActionType.Main, KeyState.Up, KeyCode.UpArrow, KeyUp );
        CurrentScene.Bind( ActionType.Main, KeyState.Up, KeyCode.DownArrow, KeyUp );
    }
}
