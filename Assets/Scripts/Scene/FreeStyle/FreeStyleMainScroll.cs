using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System;

public class FreeStyleMainScroll : ScrollBase
{
    public SongInfomation prefab;

    private RectTransform rt => transform as RectTransform;
    private Vector2 contentOriginPos;
    private Vector2 prefabOriginPos;
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
    private LinkedListNode<SongInfomation> curNode;
    private CustomVerticalLayoutGroup group;

    [Header("Time")]
    private readonly float ScrollUpdateTime = .075f;
    private readonly float KeyHoldWaitTime = .5f;
    private readonly float KeyUpWaitTime = .2f;
    private bool isKeyUp, isKeyPress;
    private float keyUpTime, keyPressTime;
    private readonly uint waitPreviewTime = 500;
    private float playback;

    [Header("Contents")]
    public GameObject noContents;
    public GameObject middleInfomationText;
    public GameObject previewCanvas;
    public GameObject particle;

    private Song curSong;
    public event Action<Song> OnSelectSong;
    public event Action<Song> OnSoundRestart;

    private void Awake()
    {
        IsLoop = true;

        CurrentScene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Scene>();
        group = GetComponent<CustomVerticalLayoutGroup>();
        SoundManager.Inst.OnReload += OnBufferSetting;

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

        UpdateSongElements();
    }

    public void UpdateSongElements()
    {
        Length = NowPlaying.Inst.Songs.Count;
        
        noContents.SetActive( !HasAnySongs );
        particle.SetActive( !HasAnySongs );
        middleInfomationText.SetActive( HasAnySongs );
        previewCanvas.SetActive( HasAnySongs );

        if ( !HasAnySongs )
        {
            SoundManager.Inst.AllStop();
            SoundManager.Inst.Load( $@"{Application.streamingAssetsPath}\\Default\\Sounds\\Bgm\\NAV5J Hana.mp3", true, false );
            SoundManager.Inst.Play();
            SoundManager.Inst.FadeVolume( new Music( SoundManager.Inst.MainSound, SoundManager.Inst.MainChannel ), 0f, 1f, .5f );
            //SoundManager.Inst.Position = 141500;
            return;
        }

        // 이전 UI 이펙트 초기화
        curNode?.Value.SetSelectColor( false );
        curNode?.Value.rt.DOAnchorPosX( prefabOriginPos.x, .5f );

        int medianCount = 0;
        int count = NowPlaying.Inst.CurrentSongIndex - median < 0 ?
                    Global.Math.Abs( NowPlaying.Inst.CurrentSongIndex - median + Length ) % Length :
                    NowPlaying.Inst.CurrentSongIndex - median;
        curNode = songs.First;
        foreach ( var song in songs )
        {
            if  ( medianCount < median )
            {
                curNode = curNode.Next;
                medianCount++;
            }

            if ( count < 0 || count >= Length )
                 count = 0;

            song.gameObject.SetActive( HasAnySongs );
            song.SetInfo( NowPlaying.Inst.Songs[count++] );
            song.PositionReset();
        }
        Select( NowPlaying.Inst.CurrentSongIndex );


        // 레이아웃 갱신
        group.Initialize();
        group.SetLayoutVertical();
        
        prefabOriginPos = curNode.Value.rt.anchoredPosition;
        size            = curNode.Value.rt.sizeDelta.y + group.spacing;

        // Count Text
        maxText.text = $"{Length}";
        curText.text = $"{CurrentIndex + 1}";

        curNode.Value.SetSelectColor( true );
        curNode.Value.rt.DOAnchorPosX( prefabOriginPos.x - 125f, .5f );

        rt.anchoredPosition = contentOriginPos;
        curPos = contentOriginPos.y;

        UpdateSong();
    }

    private void Update()
    {
        if ( !HasAnySongs ) return;

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

    private void OnDestroy()
    {
        SoundManager.Inst.OnReload -= OnBufferSetting;
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
                        Global.Math.Abs( CurrentIndex - median + Length ) % Length :
                        CurrentIndex - median;
        last.SetInfo( NowPlaying.Inst.Songs[infoIndex] );

        // 노드 이동
        songs.RemoveLast();
        songs.AddFirst( last );
        last.rt.SetAsFirstSibling();

        // 위치 갱신
        curNode.Value.SetSelectColor( false );
        curNode.Value.rt.DOAnchorPosX( prefabOriginPos.x, .5f );
        curNode = curNode.Previous;
        curNode.Value.SetSelectColor( true );
        curNode.Value.rt.DOAnchorPosX( prefabOriginPos.x - 125f, .5f );

        curPos -= size;
        rt.DOAnchorPosY( curPos, .3f );

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
        int infoIndex = CurrentIndex + median >= Length ?
                        Global.Math.Abs( CurrentIndex + median - Length ) % Length :
                        CurrentIndex + median;
        first.SetInfo( NowPlaying.Inst.Songs[infoIndex] );

        // 노드 이동
        songs.RemoveFirst();
        songs.AddLast( first );
        first.rt.SetAsLastSibling();

        // 위치 갱신
        curNode.Value.SetSelectColor( false );
        curNode.Value.rt.DOAnchorPosX( prefabOriginPos.x, .5f );
        curNode = curNode.Next;
        curNode.Value.SetSelectColor( true );
        curNode.Value.rt.DOAnchorPosX( prefabOriginPos.x - 125f, .5f );

        curPos += size;
        rt.DOAnchorPosY( curPos, .3f );

        curText.text = $"{CurrentIndex + 1}";
    }

    private void UpdateSong()
    {
        NowPlaying.Inst.UpdateSong( CurrentIndex );
        curSong = NowPlaying.CurrentSong;

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

    private void OnBufferSetting()
    {
        SoundManager.Inst.Load( curSong.audioPath, false, true );
        SoundManager.Inst.Play();
        SoundManager.Inst.Position = ( uint )playback;
    }

    private uint GetPreviewTime( int _time ) => _time <= 0 ? ( uint )( curSong.totalTime * .314f ) : ( uint )_time;

    private void SelectChart()
    {
        if ( !HasAnySongs ) return;

        GameSetting.NoteSizeMultiplier = NowPlaying.CurrentSong.keyCount == 4 ? 1.25f : 1f;

        SoundManager.Inst.Play( SoundSfxType.MainClick );
        CurrentScene.LoadScene( SceneType.Game );
    }

    private void ScrollDown()
    {
        if ( !HasAnySongs ) return;

        isKeyUp = false;
        SoundManager.Inst.Play( SoundSfxType.MainSelect );
        PrevMove();
    }

    private void ScrollUp()
    {
        if ( !HasAnySongs ) return;

        isKeyUp = false;
        SoundManager.Inst.Play( SoundSfxType.MainSelect );
        NextMove();
    }

    private void KeyHold( Action _action )
    {
        if ( !HasAnySongs ) return;

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
        if ( !HasAnySongs ) return;

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
