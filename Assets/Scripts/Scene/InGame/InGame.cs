using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class InGame : Scene
{
    public GameObject loadingCanvas;
    public OptionController pause, gameOver;
    // public GameObject hitCount, debug;

    public event Action<Chart> OnSystemInitialize;
    public event Action<Chart> OnSystemInitializeThread;

    public event Action OnGameStart;
    public event Action OnGameOver;
    public event Action OnReLoad;
    public event Action OnResult;
    public event Action<bool/* isPause */> OnPause;
    public event Action OnLoadEnd;
    public bool IsEnd { get; private set; }
    private bool[] isHitLastNotes;

    private readonly float AdditionalLoadTime = 1f;

    protected override void Awake()
    {
        base.Awake();

        int antiAliasing = ( int )SystemSetting.CurrentAntiAliasing;
        QualitySettings.antiAliasing =  antiAliasing == 1 ? 2  :
                                        antiAliasing == 2 ? 4  :
                                        antiAliasing == 3 ? 8  :
                                        antiAliasing == 4 ? 16 : 0;

        isHitLastNotes  = new bool[NowPlaying.KeyCount];
        IsGameInputLock = true;
        IsInputLock     = true;

        //hitCount.SetActive( GameSetting.CurrentVisualFlag.HasFlag( GameVisualFlag.ShowHitCount ) );
        //debug.SetActive( GameSetting.CurrentVisualFlag.HasFlag( GameVisualFlag.ShowDebug ) );

        NowPlaying.Inst.ParseChart();
    }

    protected async override void Start()
    {
        base.Start();

        OnSystemInitialize?.Invoke( NowPlaying.CurrentChart );
        await Task.Run( () => OnSystemInitializeThread?.Invoke( NowPlaying.CurrentChart ) );
        StartCoroutine( Play() );
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        SoundManager.Inst.KeyRelease();
    }

    public override void Connect()
    {
        SoundManager.Inst.SetPitch( GameSetting.CurrentPitch, ChannelType.BGM );
        if ( GameSetting.CurrentPitchType != PitchType.None )
             SoundManager.Inst.AddDSP( FMOD.DSP_TYPE.PITCHSHIFT, ChannelType.BGM );

        //SoundManager.Inst.AddDSP( FMOD.DSP_TYPE.FFT, ChannelType.BGM );
    }

    public override void Disconnect()
    {
        if ( GameSetting.CurrentPitchType != PitchType.None )
             SoundManager.Inst.RemoveDSP( FMOD.DSP_TYPE.PITCHSHIFT, ChannelType.BGM );

        //SoundManager.Inst.RemoveDSP( FMOD.DSP_TYPE.FFT, ChannelType.BGM );
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

    //public void HitLastNote( int _lane )
    //{
    //    isHitLastNotes[_lane] = true;
    //    bool isEnd = true;
    //    for ( int i = 0; i < isHitLastNotes.Length; i++ )
    //    {
    //        isEnd &= isHitLastNotes[i];
    //    }
    //    IsEnd = isEnd;

    //    if ( IsEnd )
    //    {
    //        StartCoroutine( GameEnd() );
    //        Debug.Log( "All lanes are empty." );
    //    }
    //}

    public IEnumerator GameEnd()
    {
        if ( NowPlaying.CurrentSong.isOnlyKeySound )
             yield return new WaitUntil( () => KeySampleSystem.UseAllSamples && SoundManager.Inst.ChannelsInUse == 0 );

        SoundManager.Inst.FadeVolume( SoundManager.Inst.Volume, 0f, 2.5f );
        yield return YieldCache.WaitForSeconds( 3f );

        Stop();
        OnResult?.Invoke();
        LoadScene( SceneType.Result );
    }

    private IEnumerator Play()
    {
        yield return new WaitUntil( () => NowPlaying.IsLoadKeySound && NowPlaying.IsLoadBGA );

        yield return YieldCache.WaitForSeconds( AdditionalLoadTime );
        OnLoadEnd?.Invoke();
        loadingCanvas.SetActive( false );

        OnGameStart?.Invoke();
        IsGameInputLock = false;
        IsInputLock     = false;
        NowPlaying.Inst.Play();
    }

    public void BackToLobby()
    {
        //Destroy( GameObject.FindGameObjectWithTag( "Judgement" ) );
        NowPlaying.Inst.ResetData();
        LoadScene( SceneType.FreeStyle );
    }

    public void Restart() => StartCoroutine( RestartProcess() );

    protected IEnumerator RestartProcess()
    {
        IsInputLock     = true;
        IsGameInputLock = true;
        yield return StartCoroutine( FadeOut() );

        ImmediateDisableCanvas( ActionType.Main, pause );
        ImmediateDisableCanvas( ActionType.Main, gameOver );
        NowPlaying.Inst.Stop();
        SoundManager.Inst.AllStop();

        Disconnect();
        Connect();

        OnReLoad?.Invoke();

        yield return StartCoroutine( FadeIn() );
        OnGameStart?.Invoke();
        NowPlaying.Inst.Play();
        IsInputLock     = false;
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
        if ( _isPause ) EnableCanvas(  ActionType.Pause, pause );
        else            DisableCanvas( ActionType.Main,  pause );
    }

    public IEnumerator GameOver()
    {
        IsInputLock     = true;

        yield return StartCoroutine( NowPlaying.Inst.GameOver() );

        IsGameInputLock = true;
        IsInputLock     = false;
        EnableCanvas( ActionType.GameOver, gameOver, false );

        OnGameOver?.Invoke();
    }

    public override void KeyBind()
    {
        // Main
        Bind( ActionType.Main, InputType.Down, KeyCode.Alpha1, () => SpeedControlProcess( false ) );
        Bind( ActionType.Main, InputType.Hold, KeyCode.Alpha1, () => PressedSpeedControl( false ) );
        Bind( ActionType.Main, InputType.Up,   KeyCode.Alpha1, () => UpedSpeedControl() );
                                                           
        Bind( ActionType.Main, InputType.Down, KeyCode.Alpha2, () => SpeedControlProcess( true ) );
        Bind( ActionType.Main, InputType.Hold, KeyCode.Alpha2, () => PressedSpeedControl( true ) );
        Bind( ActionType.Main, InputType.Up,   KeyCode.Alpha2, () => UpedSpeedControl() );

        // Pause
        Bind( ActionType.Main,  KeyCode.Escape,    () => { Pause( true  ); } );
        Bind( ActionType.Pause, KeyCode.Escape,    () => { Pause( false ); } );
        Bind( ActionType.Pause, KeyCode.DownArrow, () => { MoveToNextOption( pause ); } );
        Bind( ActionType.Pause, KeyCode.UpArrow,   () => { MoveToPrevOption( pause ); } );

        // GameOver
        Bind( ActionType.GameOver, KeyCode.DownArrow, () => { MoveToNextOption( gameOver ); } );
        Bind( ActionType.GameOver, KeyCode.UpArrow,   () => { MoveToPrevOption( gameOver ); } );

        // Etc.
        Bind( ActionType.Main, InputType.Down, KeyCode.F1, () => GameSetting.IsAutoRandom = !GameSetting.IsAutoRandom );
        Bind( ActionType.Main, InputType.Down, KeyCode.F2, () => GameSetting.UseClapSound = !GameSetting.UseClapSound );
    }
}