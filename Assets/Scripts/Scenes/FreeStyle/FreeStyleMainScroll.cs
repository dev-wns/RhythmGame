using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System;

public class FreeStyleMainScroll : ScrollBase, IKeyBind
{
    public SongInfomation prefab;
    private RectTransform rt;

    [Header("Scroll")]
    public ScrollBar scrollbar;
    public int maxShowCount = 7;
    public int extraCount   = 8;
    
    private int median;
    private float curPos;
    private float size;
    
    [Header( "Count Text" )]
    public TextMeshProUGUI maxText;
    public TextMeshProUGUI curText;

    public Scene CurrentScene { get; private set; }
    [Header( "Scene" )]
    private LinkedList<SongInfomation> songs = new LinkedList<SongInfomation>();
    private LinkedListNode<SongInfomation> curNode, prevNode, nextNode;
    private CustomVerticalLayoutGroup group;

    [Header("Time")]
    private readonly float ScrollUpdateTime = .075f;
    private readonly float KeyHoldWaitTime = .5f;
    private readonly float KeyUpWaitTime = .2f;
    private bool isKeyUp, isKeyPress;
    private float keyUpTime, keyPressTime;
    private readonly uint waitPreviewTime = 500;
    private float playback;

    private Song curSong;
    public event Action<Song> OnSelectSong;
    public event Action<Song> OnSoundRestart;

    private void Awake()
    {
        IsLoop = true;
        rt = transform as RectTransform;
        curPos = rt.anchoredPosition.y;

        CurrentScene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Scene>();
        group = GetComponent<CustomVerticalLayoutGroup>();
        KeyBind();

        median = Mathf.FloorToInt( ( maxShowCount + extraCount ) / 2f );

        // 객체 할당
        scrollbar.Initialize( NowPlaying.Inst.Songs.Count );
        Length    = NowPlaying.Inst.Songs.Count;
        int count = NowPlaying.Inst.CurrentSongIndex - median < 0 ?
                    NowPlaying.Inst.CurrentSongIndex - median + Length :
                    NowPlaying.Inst.CurrentSongIndex - median;

        for ( int i = 0; i < maxShowCount + extraCount; i++ )
        {
            if ( count > Length - 1 ) count = 0;
            var song = Instantiate( prefab, transform );
            song.Initialize();
            song.SetInfo( NowPlaying.Inst.Songs[count++] );
            songs.AddLast( song );
        }
        Select( NowPlaying.Inst.CurrentSongIndex );

        // 레이아웃 갱신
        group.Initialize();
        group.SetLayoutVertical();

        // 중앙 위치에 있는 객체
        curNode = songs.First;
        for ( int i = 0; i < median; i++ )
        {
            curNode = curNode.Next;
        }
        size = curNode.Value.rt.sizeDelta.y + group.spacing;

        // Active 조절기준이 될 객체
        prevNode = songs.First;
        nextNode = songs.Last;
        int extraHalf = Mathf.FloorToInt( extraCount / 2f );
        for ( int i = 0; i < extraHalf; i++ )
        {
            prevNode.Value.gameObject.SetActive( false );
            nextNode.Value.gameObject.SetActive( false );
            prevNode = prevNode.Next;
            nextNode = nextNode.Previous;
        }

        // Count Text
        if ( maxText ) maxText.text = Length.ToString();
    }

    private void Start()
    {
        curNode.Value.rt.DOAnchorPosX( -100f, .5f );
        UpdateScrollBar();

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
        int infoIndex = CurrentIndex - median < 0 ?
                        CurrentIndex - median + Length :
                        CurrentIndex - median;
        last.SetInfo( NowPlaying.Inst.Songs[infoIndex] );
        
        // 활성화
        nextNode.Value.gameObject.SetActive( false );
        nextNode = nextNode.Previous;

        prevNode = prevNode.Previous;
        prevNode.Value.gameObject.SetActive( true );

        // 노드 이동
        songs.RemoveLast();
        songs.AddFirst( last );
      
        // 위치 갱신
        curNode.Value.rt.DOAnchorPosX( 0f, .5f );
        curNode = curNode.Previous;
        curNode.Value.rt.DOAnchorPosX( -100f, .5f );

        curPos -= size;
        rt.DOAnchorPosY( curPos, .25f );

        //UpdateSong();
        UpdateScrollBar();
    }

    public override void NextMove()
    {
        base.NextMove();

        // 객체 위치 스왑
        var first = songs.First.Value;
        var last  = songs.Last.Value;
        first.rt.anchoredPosition = new Vector2( last.rt.anchoredPosition.x, last.rt.anchoredPosition.y - size );
        
        // Song 정보 수정
        int infoIndex = CurrentIndex + median >= Length ?
                        CurrentIndex + median - Length :
                        CurrentIndex + median;
        first.SetInfo( NowPlaying.Inst.Songs[infoIndex] );

        // 활성화
        prevNode.Value.gameObject.SetActive( false );
        prevNode = prevNode.Next;

        nextNode = nextNode.Next;
        nextNode.Value.gameObject.SetActive( true );

        // 노드 이동
        songs.RemoveFirst();
        songs.AddLast( first );

        // 위치 갱신
        curNode.Value.rt.DOAnchorPosX( 0f, .5f );
        curNode = curNode.Next;
        curNode.Value.rt.DOAnchorPosX( -100f, .5f );

        curPos += size;
        rt.DOAnchorPosY( curPos, .25f );

        //UpdateSong();
        UpdateScrollBar();
    }

    private void UpdateScrollBar()
    {
        scrollbar.UpdateHandle( CurrentIndex );

        if ( curText ) 
             curText.text = ( CurrentIndex + 1 ).ToString();
    }

    private void UpdateSong()
    {
        NowPlaying.Inst.UpdateSong( CurrentIndex );
        curSong = NowPlaying.Inst.CurrentSong;

        Music prevMusic = new Music( SoundManager.Inst.MainSound, SoundManager.Inst.MainChannel );
        SoundManager.Inst.FadeVolume( prevMusic, 1f, 0f, .5f, () => SoundManager.Inst.Stop( prevMusic ) );

        SoundManager.Inst.Load( curSong.audioPath, false, true );
        curSong.totalTime = ( int )SoundManager.Inst.Length;
        curSong.previewTime = ( int )GetPreviewTime( curSong.previewTime );
        playback = curSong.previewTime;

        SoundManager.Inst.Play();
        OnSelectSong?.Invoke( curSong );
        Music curMusic = new Music( SoundManager.Inst.MainSound, SoundManager.Inst.MainChannel );
        SoundManager.Inst.FadeVolume( curMusic, 0f, 1f, .5f );
        SoundManager.Inst.Position = ( uint )curSong.previewTime;
    }

    private uint GetPreviewTime( int _time ) => _time <= 0 ? ( uint )( curSong.totalTime * .314f ) : ( uint )_time;

    private void SelectChart()
    {
        GameSetting.NoteSizeMultiplier = NowPlaying.Inst.CurrentSong.keyCount == 4 ? 1.25f : 1f;
        SoundManager.Inst.Play( SoundSfxType.MainClick );
        CurrentScene.LoadScene( SceneType.Game );
    }

    private void ScrollDown()
    {
        isKeyUp = false;
        SoundManager.Inst.Play( SoundSfxType.MainSelect );
        PrevMove();
    }

    private void ScrollUp()
    {
        isKeyUp = false;
        SoundManager.Inst.Play( SoundSfxType.MainSelect );
        NextMove();
    }

    private void Update()
    {
        playback += ( Time.deltaTime * 1000f ) * GameSetting.CurrentPitch;
        if ( ( curSong.totalTime + waitPreviewTime < playback ) )
        {
            SoundManager.Inst.Play();
            SoundManager.Inst.Position = ( uint )curSong.previewTime;
            playback = curSong.previewTime;
            OnSoundRestart?.Invoke( curSong );

            Music curMusic = new Music( SoundManager.Inst.MainSound, SoundManager.Inst.MainChannel );
            SoundManager.Inst.FadeVolume( curMusic, 0f, 1f, .5f );
        }

        if ( isKeyUp )
        {
            keyUpTime += Time.deltaTime;
            if ( keyUpTime >= KeyUpWaitTime )
            {
                isKeyUp = false;
                UpdateSong();
            }
        }
    }

    private void KeyHold( Action _action )
    {
        keyPressTime += Time.deltaTime;
        if ( keyPressTime >= KeyHoldWaitTime )
             isKeyPress = true;

        if ( isKeyPress && keyPressTime >= ScrollUpdateTime )
        {
            keyPressTime = 0f;
            _action?.Invoke();
        }
    }

    private void KeyUp()
    {
        keyPressTime = keyUpTime = 0f;
        isKeyPress = false;
        isKeyUp = true;
    }

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
