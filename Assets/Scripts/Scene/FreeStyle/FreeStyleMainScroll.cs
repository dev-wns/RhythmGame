using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using TMPro;
using TreeEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class FreeStyleMainScroll : ScrollBase
{
    public SongInfomation prefab;
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
    private LinkedList<SongInfomation> songs = new LinkedList<SongInfomation>();
    private LinkedListNode<SongInfomation> medianNode;
    private CustomVerticalLayoutGroup group;

    [Header("Time")]
    private readonly float ScrollUpdateTime = .075f;
    private readonly float KeyHoldWaitTime  = .5f;
    private readonly uint waitPreviewTime   = 500;
    private bool isKeyDown;
    private float keyPressTime;
    private float playback;
    private float endTime;

    [Header("Contents")]
    public GameObject noContents;

    private Song curSong;
    public event Action<Song> OnSelectSong;
    public event Action<Song> OnSoundRestart;

    #region Unity Callback
    private void Awake()
    {
        IsLoop = true;

        CurrentScene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Scene>();
        group = GetComponent<CustomVerticalLayoutGroup>();
        SoundManager.Inst.OnReload += OnBufferSetting;
        search.OnSearch += UpdateLayoutAndSong;

        median           = Mathf.FloorToInt( maxShowCount / 2f );
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
            SoundManager.Inst.AllStop();
            SoundManager.Inst.Load( $@"{Application.streamingAssetsPath}\\Default\\Sounds\\Bgm\\Hana.mp3", true, false );
            SoundManager.Inst.Play( 0f );
            SoundManager.Inst.Position = 160000;
            SoundManager.Inst.FadeVolume( 0f, 1f, .5f );
        }
    }

    private void Update()
    {
        if ( !HasAnySongs ) return;

        playback += ( Time.deltaTime * 1000f ) * GameSetting.CurrentPitch;
        if ( endTime > 0f && ( endTime + waitPreviewTime < playback ) )
        {
            SoundManager.Inst.Play();
            SoundManager.Inst.Position = ( uint )curSong.previewTime;
            playback = curSong.previewTime;
            OnSoundRestart?.Invoke( curSong );

            SoundManager.Inst.FadeVolume( 0f, 1f, .5f );
        }
    }

    private void OnDestroy()
    {
        SoundManager.Inst.OnReload -= OnBufferSetting;
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
        Select( NowPlaying.Inst.CurrentSongIndex );
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

        if ( curSong.UID != NowPlaying.CurrentSong.UID )
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

    private void UpdateSong()
    {
        NowPlaying.Inst.UpdateSong( CurrentIndex );
        curSong = NowPlaying.CurrentSong;

        // 이전 음악 페이드아웃
        Music prevMusic = new Music( SoundManager.Inst.MainSound, SoundManager.Inst.MainChannel );
        SoundManager.Inst.FadeVolume( prevMusic, 1f, 0f, .5f, () => SoundManager.Inst.Stop( prevMusic ) );

        // 새로운 음악 로딩
        SoundManager.Inst.Load( curSong.audioPath, false, true );
        endTime             = ( int )SoundManager.Inst.Length;
        curSong.previewTime = ( int )GetPreviewTime( curSong.previewTime );
        playback            = curSong.previewTime;

        // 음악 재생 및 페이드인
        SoundManager.Inst.Play( 0f );
        SoundManager.Inst.Position = ( uint )curSong.previewTime;
        OnSelectSong?.Invoke( curSong );
        SoundManager.Inst.FadeVolume( new Music( SoundManager.Inst.MainSound, SoundManager.Inst.MainChannel ), 0f, 1f, .5f );
    }

    private void OnBufferSetting()
    {
        SoundManager.Inst.Load( curSong.audioPath, false, true );
        SoundManager.Inst.Play();
        SoundManager.Inst.Position = ( uint )playback;
    }

    private uint GetPreviewTime( int _time ) => _time > endTime || _time <= 0 ? ( uint )( endTime * .35f ) : ( uint )_time;
    #endregion

    #region Input
    private void SelectChart()
    {
        if ( !HasAnySongs ) return;

        GameSetting.NoteSizeMultiplier = NowPlaying.KeyCount == 4 ? 1.25f : 1f;

        SoundManager.Inst.Play( SoundSfxType.MainClick );
        CurrentScene.LoadScene( SceneType.Game );
    }

    private void ScrollDown()
    {
        if ( !HasAnySongs ) return;

        SoundManager.Inst.Play( SoundSfxType.MainSelect );
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

        SoundManager.Inst.Play( SoundSfxType.MainSelect );
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
        
        if ( NowPlaying.Inst.CurrentSongIndex != CurrentIndex )
             UpdateSong();

        isKeyDown = false;
        keyPressTime = 0f;
    }
    #endregion

    public void KeyBind()
    {
        CurrentScene.Bind( ActionType.Main, KeyCode.Return, SelectChart );

        CurrentScene.Bind( ActionType.Main, InputType.Down, KeyCode.UpArrow,   ScrollDown );
        CurrentScene.Bind( ActionType.Main, InputType.Down, KeyCode.DownArrow, ScrollUp );

        // 지연시간 이후 일정시간마다 델리게이트 실행 ( Hold 시 0.5초 이후부터 빠르게 스크롤 )
        CurrentScene.Bind( ActionType.Main, InputType.Hold, KeyCode.UpArrow,   () => KeyHold( ScrollDown ) );
        CurrentScene.Bind( ActionType.Main, InputType.Hold, KeyCode.DownArrow, () => KeyHold( ScrollUp ) );

        // 재고있던 스크롤 시간 초기화 및 비활성화 + 채보변경 타이머 시작
        CurrentScene.Bind( ActionType.Main, InputType.Up, KeyCode.UpArrow,   KeyUp );
        CurrentScene.Bind( ActionType.Main, InputType.Up, KeyCode.DownArrow, KeyUp );
    }
}
