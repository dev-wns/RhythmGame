using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InGame : Scene
{
    [Header( "InGame" )]
    public GameObject loadingCanvas;
    public OptionController pause, gameOver;
    public event Action OnLoadEnd;

    [Header("Fill Timer")]
    public Image timeImage;
    private double length;

    protected override void Awake()
    {
        base.Awake();

        IsInputLock = true;
        int antiAliasing = ( int )SystemSetting.CurrentAntiAliasing;
        QualitySettings.antiAliasing = antiAliasing == 1 ? 2 :
                                       antiAliasing == 2 ? 4 :
                                       antiAliasing == 3 ? 8 :
                                       antiAliasing == 4 ? 16 : 0;


        length = NowPlaying.CurrentSong.totalTime / GameSetting.CurrentPitch;
        NowPlaying.Inst.Initialize();
    }

    protected override void Start()
    {
        base.Start();

        NowPlaying.Inst.Load();
        StartCoroutine( Play() );
    }

    private void Update()
    {
        timeImage.fillAmount = ( float )Global.Math.Clamp( ( NowPlaying.Playback / length ), 0d, 1d );
    }

    public override void Connect()
    {
        AudioManager.Inst.SetPitch( GameSetting.CurrentPitch, ChannelType.BGM );
        if ( GameSetting.CurrentPitchType != PitchType.None )
             AudioManager.Inst.AddDSP( FMOD.DSP_TYPE.PITCHSHIFT, ChannelType.BGM );
    }

    public override void Disconnect()
    {
        if ( GameSetting.CurrentPitchType != PitchType.None )
             AudioManager.Inst.RemoveDSP( FMOD.DSP_TYPE.PITCHSHIFT, ChannelType.BGM );
    }

    private IEnumerator Play()
    {
        // 로딩 완료 체크
        yield return new WaitUntil( () => NowPlaying.IsLoaded );
        OnLoadEnd?.Invoke();

        // 로딩 후 대기시간
        yield return YieldCache.WaitForSeconds( 3.5f );
        if ( loadingCanvas.TryGetComponent( out CanvasGroup loadingGroup ) )
        {
            DOTween.To( () => 1f, x => loadingGroup.alpha = x, 0f, Global.Const.CanvasFadeDuration );
            
            yield return new WaitUntil( () => loadingGroup.alpha <= 0f );
            loadingCanvas.SetActive( false );
        }

        // 게임 시작
        IsInputLock = false;
        NowPlaying.Inst.GameStart();
        //OnGameStart?.Invoke();

        // 게임 종료
        yield return new WaitUntil( () => NowPlaying.TotalJudge <= Judgement.CurrentResult.Count );
        Debug.Log( $"All lanes are empty ( {Judgement.CurrentResult.Count} Judgements )" );

        //if ( NowPlaying.CurrentSong.isOnlyKeySound )
        //     yield return new WaitUntil( () => NowPlaying.UseAllSamples && AudioManager.Inst.ChannelsInUse == 0 );

        AudioManager.Inst.FadeVolume( AudioManager.Inst.Volume, 0f, 2.5f );
        yield return YieldCache.WaitForSeconds( 5f ); // 5초 후 결과창으로

        NowPlaying.Inst.Release();
        LoadScene( SceneType.Result );
    }

    public void BackToLobby()
    {
        NowPlaying.Inst.Release();
        LoadScene( SceneType.FreeStyle );
    }

    public void Restart() => StartCoroutine( RestartAfterFade() );

    protected IEnumerator RestartAfterFade()
    {
        IsInputLock = true;
        yield return StartCoroutine( FadeOut() );

        ImmediateDisableCanvas( ActionType.Main, pause );
        ImmediateDisableCanvas( ActionType.Main, gameOver );

        Disconnect();
        Connect();

        NowPlaying.Inst.Clear();
        yield return StartCoroutine( FadeIn() );
        NowPlaying.Inst.GameStart();

        IsInputLock = false;
    }

    public void Pause( bool _isPause )
    {
        if ( NowPlaying.TotalJudge <= Judgement.CurrentResult.Count )
        {
            LoadScene( SceneType.Result );
        }
        else
        {
            NowPlaying.Inst.Pause( _isPause );
            if ( _isPause ) EnableCanvas( ActionType.Pause, pause );
            else            DisableCanvas( ActionType.Main, pause );
        }
    }

    public IEnumerator GameOver()
    {
        IsInputLock = true;

        //OnGameOver?.Invoke();
        yield return StartCoroutine( NowPlaying.Inst.GameOver() );
        EnableCanvas( ActionType.GameOver, gameOver, false );

        IsInputLock = false;
    }

    public override void KeyBind()
    {
        // Main
        // Scroll Speed Down
        Bind( ActionType.Main, KeyState.Down, KeyCode.Alpha1, () => SpeedControlProcess( false ) );
        Bind( ActionType.Main, KeyState.Hold, KeyCode.Alpha1, () => PressedSpeedControl( false ) );
        Bind( ActionType.Main, KeyState.Up,   KeyCode.Alpha1, () => UpedSpeedControl() );
        // Scroll Speed Up                               
        Bind( ActionType.Main, KeyState.Down, KeyCode.Alpha2, () => SpeedControlProcess( true ) );
        Bind( ActionType.Main, KeyState.Hold, KeyCode.Alpha2, () => PressedSpeedControl( true ) );
        Bind( ActionType.Main, KeyState.Up,   KeyCode.Alpha2, () => UpedSpeedControl() );

        // Pause
        Bind( ActionType.Main,  KeyCode.Escape,    () => { Pause( true ); } );
        Bind( ActionType.Pause, KeyCode.Escape,    () => { Pause( false ); } );
        Bind( ActionType.Pause, KeyCode.DownArrow, () => { MoveToNextOption( pause ); } );
        Bind( ActionType.Pause, KeyCode.UpArrow,   () => { MoveToPrevOption( pause ); } );
        // Scroll Speed Down
        Bind( ActionType.Pause, KeyState.Down, KeyCode.Alpha1, () => SpeedControlProcess( false ) );
        Bind( ActionType.Pause, KeyState.Hold, KeyCode.Alpha1, () => PressedSpeedControl( false ) );
        Bind( ActionType.Pause, KeyState.Up,   KeyCode.Alpha1, () => UpedSpeedControl() );
        // Scroll Speed Up
        Bind( ActionType.Pause, KeyState.Down, KeyCode.Alpha2, () => SpeedControlProcess( true ) );
        Bind( ActionType.Pause, KeyState.Hold, KeyCode.Alpha2, () => PressedSpeedControl( true ) );
        Bind( ActionType.Pause, KeyState.Up,   KeyCode.Alpha2, () => UpedSpeedControl() );

        // GameOver
        Bind( ActionType.GameOver, KeyCode.DownArrow, () => { MoveToNextOption( gameOver ); } );
        Bind( ActionType.GameOver, KeyCode.UpArrow,   () => { MoveToPrevOption( gameOver ); } );
    }
}