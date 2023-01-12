using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using DG.Tweening;

public class InGame : Scene
{
    public GameObject loadingCanvas;
    public InGamePause pause;

    public event Action<Chart> OnSystemInitialize;
    public event Action<Chart> OnSystemInitializeThread;

    public event Action OnGameStart;
    public event Action OnReLoad;
    public event Action OnLoadEnd;

    private readonly float AdditionalLoadTime = 1f;

    protected override void Awake()
    {
        base.Awake();
        NowPlaying.Inst.ParseChart();
    }

    protected async override void Start()
    {
        base.Start();
        IsInputLock = true;
        OnSystemInitialize( NowPlaying.CurrentChart );
        Task LoadkeySoundAsyncTask = Task.Run( () => OnSystemInitializeThread( NowPlaying.CurrentChart ) );

        await LoadkeySoundAsyncTask;

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
    }

    public override void Disconnect()
    {
        if ( GameSetting.CurrentPitchType != PitchType.None )
             SoundManager.Inst.RemoveDSP( FMOD.DSP_TYPE.PITCHSHIFT, ChannelType.BGM );
    }

    private IEnumerator Play()
    {
        yield return new WaitUntil( () => NowPlaying.Inst.IsLoadKeySound && NowPlaying.Inst.IsLoadBGA );

        yield return YieldCache.WaitForSeconds( AdditionalLoadTime );
        OnLoadEnd?.Invoke();
        loadingCanvas.SetActive( false );

        OnGameStart?.Invoke();
        IsInputLock = false;
        NowPlaying.Inst.Play();
    }

    public void BackToLobby()
    {
        //Destroy( GameObject.FindGameObjectWithTag( "Judgement" ) );
        LoadScene( SceneType.FreeStyle );
    }
    public void Restart() => StartCoroutine( RestartProcess() );

    protected IEnumerator RestartProcess()
    {
        IsInputLock = true;
        yield return StartCoroutine( FadeOut() );

        DisableCanvas( ActionType.Main, pause );
        NowPlaying.Inst.Stop();
        SoundManager.Inst.AllStop();

        Disconnect();
        Connect();

        OnReLoad?.Invoke();

        yield return StartCoroutine( FadeIn() );
        OnGameStart?.Invoke();
        NowPlaying.Inst.Play();
        IsInputLock = false;
    }

    public void Pause( bool _isPuase )
    {
        if ( _isPuase )
        {
            if ( NowPlaying.Inst.Pause( true ) )
            {
                EnableCanvas( ActionType.Pause, pause );
            }
            else
            {
                NowPlaying.Inst.Stop();
                LoadScene( SceneType.Result );
            }
        }
        else
        {
            NowPlaying.Inst.Pause( false );
            DisableCanvas( ActionType.Main, pause );
        }
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

        // Etc.
        Bind( ActionType.Main, InputType.Down, KeyCode.F1, () => GameSetting.IsAutoRandom   = !GameSetting.IsAutoRandom );
        Bind( ActionType.Main, InputType.Down, KeyCode.F2, () => GameSetting.IsNoteBodyGray = !GameSetting.IsNoteBodyGray );
    }
}