using DG.Tweening;
using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class InGame : Scene
{
    [Header( "InGame" )]
    public GameObject loadingCanvas;
    //public GameObject scoreMeterCanvas;
    public OptionController pause, gameOver;

    public event Action<Chart> OnSystemInitialize;
    public event Action<Chart> OnSystemInitializeThread;

    public event Action OnGameStart;
    public event Action OnGameOver;
    public event Action OnReLoad;
    public event Action OnResult;
    public event Action OnLoadEnd;
    public event Action<bool/* isPause */> OnPause;
    public bool IsEnd { get; private set; }
    private bool[] isHitLastNotes;

    private readonly float AdditionalLoadTime = 5f;

    [Header( "Loading" )]
    public TextMeshProUGUI loadingText;
    private Timer timer  = new Timer();
    private uint loadingTime;

    public TextMeshProUGUI soundText;
    public TextMeshProUGUI etcText;

    protected override void Awake()
    {
        base.Awake();

        int antiAliasing = ( int )SystemSetting.CurrentAntiAliasing;
        QualitySettings.antiAliasing = antiAliasing == 1 ? 2 :
                                        antiAliasing == 2 ? 4 :
                                        antiAliasing == 3 ? 8 :
                                        antiAliasing == 4 ? 16 : 0;

        isHitLastNotes = new bool[NowPlaying.KeyCount];
        IsGameInputLock = true;
        IsInputLock = true;

        timer.Start();
        NowPlaying.Inst.ParseChart();
        loadingText.text = $"{timer.End} ms";
    }

    protected async override void Start()
    {
        base.Start();

        OnSystemInitialize?.Invoke( NowPlaying.CurrentChart );
        await Task.Run( () => OnSystemInitializeThread?.Invoke( NowPlaying.CurrentChart ) );

        soundText.text = $"{LaneSystem.soundSampleTime + LaneSystem.keySoundTime} ms";

        StartCoroutine( Play() );
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        AudioManager.Inst.KeyRelease();
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

    private void Stop()
    {
        NowPlaying.Inst.Stop();
        IsEnd = false;
        for ( int i = 0; i < isHitLastNotes.Length; i++ )
        {
            isHitLastNotes[i] = false;
        }
    }

    public IEnumerator GameEnd()
    {
        IsEnd = true;

        if ( NowPlaying.CurrentSong.isOnlyKeySound )
            yield return new WaitUntil( () => KeySampleSystem.UseAllSamples && AudioManager.Inst.ChannelsInUse == 0 );

        AudioManager.Inst.FadeVolume( AudioManager.Inst.Volume, 0f, 2.5f );
        yield return YieldCache.WaitForSeconds( 3f );

        Stop();
        OnResult?.Invoke();
        LoadScene( SceneType.Result );
    }

    private IEnumerator Play()
    {
        WaitUntil waitLoadDatas = new WaitUntil( () => NowPlaying.IsLoadKeySound && NowPlaying.IsLoadBGA );
        yield return waitLoadDatas;

        uint etcTime = ( LaneSystem.noteTime - LaneSystem.keySoundTime ) +
                         MeasureSystem.MeasureCalcTime;

        etcText.text = $"{etcTime} ms";
        OnLoadEnd?.Invoke();

        yield return YieldCache.WaitForSeconds( AdditionalLoadTime );

        if ( loadingCanvas.TryGetComponent( out CanvasGroup loadingGroup ) )
        {
            DOTween.To( () => 1f, x => loadingGroup.alpha = x, 0f, Global.Const.OptionFadeDuration );

            WaitUntil waitCanvasDisabled = new WaitUntil( () => loadingGroup.alpha <= 0f );
            yield return waitCanvasDisabled;
            loadingCanvas.SetActive( false );
        }

        //if ( !GameSetting.CurrentGameMode.HasFlag( GameMode.AutoPlay ) &&
        //     scoreMeterCanvas.TryGetComponent( out CanvasGroup scoreMeterGroup ) )
        //{
        //    scoreMeterCanvas.SetActive( true );
        //    DOTween.To( () => 0f, x => scoreMeterGroup.alpha = x, 1f, Global.Const.OptionFadeDuration );
        //}

        OnGameStart?.Invoke();
        IsGameInputLock = false;
        IsInputLock = false;
        NowPlaying.Inst.Play();
    }

    public void BackToLobby()
    {
        NowPlaying.Inst.ResetData();
        LoadScene( SceneType.FreeStyle );
    }

    public void Restart() => StartCoroutine( RestartProcess() );

    protected IEnumerator RestartProcess()
    {
        IsInputLock = true;
        IsGameInputLock = true;
        yield return StartCoroutine( FadeOut() );

        ImmediateDisableCanvas( ActionType.Main, pause );
        ImmediateDisableCanvas( ActionType.Main, gameOver );
        NowPlaying.Inst.Stop();
        AudioManager.Inst.AllStop();

        Disconnect();
        Connect();

        OnReLoad?.Invoke();

        yield return StartCoroutine( FadeIn() );
        OnGameStart?.Invoke();
        NowPlaying.Inst.Play();
        IsInputLock = false;
        IsGameInputLock = false;
    }

    public void Pause( bool _isPause )
    {
        if ( IsEnd )
        {
            Stop();
            OnResult?.Invoke();
            LoadScene( SceneType.Result );
        }
        else
        {
            IsGameInputLock = _isPause;
            NowPlaying.Inst.Pause( _isPause );
            ShowPauseCanvas( _isPause );
            OnPause?.Invoke( _isPause );
        }
    }

    private void ShowPauseCanvas( bool _isPause )
    {
        if ( _isPause ) EnableCanvas( ActionType.Pause, pause );
        else DisableCanvas( ActionType.Main, pause );
    }

    public IEnumerator GameOver()
    {
        IsInputLock = true;

        yield return StartCoroutine( NowPlaying.Inst.GameOver() );

        IsGameInputLock = true;
        IsInputLock = false;
        EnableCanvas( ActionType.GameOver, gameOver, false );

        OnGameOver?.Invoke();
    }

    public override void KeyBind()
    {
        // Main
        // Scroll Speed Down
        Bind( ActionType.Main, InputType.Down, KeyCode.Alpha1, () => SpeedControlProcess( false ) );
        Bind( ActionType.Main, InputType.Hold, KeyCode.Alpha1, () => PressedSpeedControl( false ) );
        Bind( ActionType.Main, InputType.Up, KeyCode.Alpha1, () => UpedSpeedControl() );
        // Scroll Speed Up                               
        Bind( ActionType.Main, InputType.Down, KeyCode.Alpha2, () => SpeedControlProcess( true ) );
        Bind( ActionType.Main, InputType.Hold, KeyCode.Alpha2, () => PressedSpeedControl( true ) );
        Bind( ActionType.Main, InputType.Up, KeyCode.Alpha2, () => UpedSpeedControl() );

        // Pause
        Bind( ActionType.Main, KeyCode.Escape, () => { Pause( true ); } );
        Bind( ActionType.Pause, KeyCode.Escape, () => { Pause( false ); } );
        Bind( ActionType.Pause, KeyCode.DownArrow, () => { MoveToNextOption( pause ); } );
        Bind( ActionType.Pause, KeyCode.UpArrow, () => { MoveToPrevOption( pause ); } );
        // Scroll Speed Down
        Bind( ActionType.Pause, InputType.Down, KeyCode.Alpha1, () => SpeedControlProcess( false ) );
        Bind( ActionType.Pause, InputType.Hold, KeyCode.Alpha1, () => PressedSpeedControl( false ) );
        Bind( ActionType.Pause, InputType.Up, KeyCode.Alpha1, () => UpedSpeedControl() );
        // Scroll Speed Up
        Bind( ActionType.Pause, InputType.Down, KeyCode.Alpha2, () => SpeedControlProcess( true ) );
        Bind( ActionType.Pause, InputType.Hold, KeyCode.Alpha2, () => PressedSpeedControl( true ) );
        Bind( ActionType.Pause, InputType.Up, KeyCode.Alpha2, () => UpedSpeedControl() );

        // GameOver
        Bind( ActionType.GameOver, KeyCode.DownArrow, () => { MoveToNextOption( gameOver ); } );
        Bind( ActionType.GameOver, KeyCode.UpArrow, () => { MoveToPrevOption( gameOver ); } );

        // Etc.
        Bind( ActionType.Main, InputType.Down, KeyCode.F1, () => GameSetting.IsAutoRandom = !GameSetting.IsAutoRandom );
        Bind( ActionType.Main, InputType.Down, KeyCode.F2, () => GameSetting.UseClapSound = !GameSetting.UseClapSound );
    }
}