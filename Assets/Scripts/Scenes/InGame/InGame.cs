using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class InGame : Scene
{
    public GameObject pauseCanvas;

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
        OnSystemInitialize( NowPlaying.Inst.CurrentChart );
        Task LoadkeySoundAsyncTask = Task.Run( () => OnSystemInitializeThread( NowPlaying.Inst.CurrentChart ) );

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
        SoundManager.Inst.AddDSP( FMOD.DSP_TYPE.PITCHSHIFT, ChannelType.BGM );
    }

    public override void Disconnect()
    {
        SoundManager.Inst.RemoveDSP( FMOD.DSP_TYPE.PITCHSHIFT, ChannelType.BGM );
    }

    private IEnumerator Play()
    {
        yield return new WaitUntil( () => NowPlaying.Inst.IsLoadKeySound && NowPlaying.Inst.IsLoadBGA );

        yield return YieldCache.WaitForSeconds( AdditionalLoadTime );
        OnLoadEnd?.Invoke();

        OnGameStart?.Invoke();
        IsInputLock = false;
        StartCoroutine( NowPlaying.Inst.Play() );
    }

    public void BackToLobby()
    {
        var judge = GameObject.FindGameObjectWithTag( "Judgement" );
        Destroy( judge );
        LoadScene( SceneType.FreeStyle );
    }

    protected IEnumerator RestartProcess()
    {
        ChangeAction( ActionType.Main );
        yield return StartCoroutine( FadeOut() );

        pauseCanvas.SetActive( false );
        NowPlaying.Inst.Stop();
        SoundManager.Inst.AllStop();

        OnReLoad?.Invoke();

        yield return StartCoroutine( FadeIn() );
        OnGameStart?.Invoke();
        yield return StartCoroutine( NowPlaying.Inst.Play() );
    }

    public void Restart() => StartCoroutine( RestartProcess() );

    public void Pause( bool _isPuase )
    {
        if ( _isPuase )
        {
            if ( NowPlaying.Inst.Pause( true ) )
            {
                pauseCanvas.SetActive( true );
                SoundManager.Inst.Play( SoundSfxType.MenuClick );
                ChangeAction( ActionType.Option );
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
            pauseCanvas.SetActive( false );
            SoundManager.Inst.Play( SoundSfxType.MenuHover );
            ChangeAction( ActionType.Main );
        }
    }

    public override void KeyBind()
    {
        Bind( ActionType.Option, KeyCode.Escape, () => Pause( false ) );
        Bind( ActionType.Main,   KeyCode.Escape, () => Pause( true ) );

        Bind( ActionType.Main, InputType.Down, KeyCode.Alpha1, () => SpeedControlProcess( false ) );
        Bind( ActionType.Main, InputType.Hold, KeyCode.Alpha1, () => PressedSpeedControl( false ) );
        Bind( ActionType.Main, InputType.Up,   KeyCode.Alpha1, () => UpedSpeedControl() );
                                                           
        Bind( ActionType.Main, InputType.Down, KeyCode.Alpha2, () => SpeedControlProcess( true ) );
        Bind( ActionType.Main, InputType.Hold, KeyCode.Alpha2, () => PressedSpeedControl( true ) );
        Bind( ActionType.Main, InputType.Up,   KeyCode.Alpha2, () => UpedSpeedControl() );

        Bind( ActionType.Main, InputType.Down, KeyCode.F1, () => GameSetting.IsAutoRandom = !GameSetting.IsAutoRandom );
    }
}
