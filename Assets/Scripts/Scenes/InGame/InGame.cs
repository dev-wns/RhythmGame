using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class InGame : Scene
{
    public GameObject pauseCanvas;

    public delegate void DelSystemInitialize( in Chart _chart );
    public event DelSystemInitialize OnSystemInitialize;
    public event DelSystemInitialize OnSystemInitializeThread;

    public event Action OnGameStart;
    public event Action OnReLoad;
    public event Action OnLoadEnd;

    private readonly float AdditionalLoadTime = 1f;

    protected override void Awake()
    {
        base.Awake();

        NowPlaying.Inst.ParseChart();

        FMOD.DSP pitchShift;
        SoundManager.Inst.GetDSP( FMOD.DSP_TYPE.PITCHSHIFT, out pitchShift );
        SoundManager.Inst.AddDSP( in pitchShift, ChannelType.KeySound );
        SoundManager.Inst.SetPitch( GameSetting.CurrentPitch, ChannelType.KeySound );
    }

    protected async override void Start()
    {
        base.Start();
        InputLock( true );
        OnSystemInitialize( NowPlaying.Inst.CurrentChart );
        Task LoadkeySoundAsyncTask = Task.Run( () => OnSystemInitializeThread( NowPlaying.Inst.CurrentChart ) );

        await LoadkeySoundAsyncTask;

        StartCoroutine( Play() );
    }

    private void OnDestroy()
    {
        SoundManager.Inst.KeyRelease();
    }

    private IEnumerator Play()
    {
        yield return new WaitUntil( () => NowPlaying.Inst.IsLoadKeySound && NowPlaying.Inst.IsLoadBGA );

        yield return YieldCache.WaitForSeconds( AdditionalLoadTime );
        OnLoadEnd?.Invoke();

        OnGameStart?.Invoke();
        InputLock( false );
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
        ChangeAction( SceneAction.Main );
        yield return StartCoroutine( FadeOut() );

        pauseCanvas.SetActive( false );
        NowPlaying.Inst.Stop();
        SoundManager.Inst.AllStop();
        SoundManager.Inst.AllRemoveDSP();

        OnReLoad?.Invoke();

        FMOD.DSP pitchShift;
        SoundManager.Inst.GetDSP( FMOD.DSP_TYPE.PITCHSHIFT, out pitchShift );
        SoundManager.Inst.AddDSP( in pitchShift, ChannelType.KeySound );

        yield return StartCoroutine( FadeIn() );
        OnGameStart?.Invoke();
        yield return NowPlaying.Inst.Play();
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
                ChangeAction( SceneAction.Option );
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
            ChangeAction( SceneAction.Main );
        }
    }

    public override void KeyBind()
    {
        Bind( SceneAction.Option, KeyCode.Escape, () => Pause( false ) );
        Bind( SceneAction.Main,   KeyCode.Escape, () => Pause( true ) );

        Bind( SceneAction.Main, KeyType.Down, KeyCode.Alpha1, () => SpeedControlProcess( false ) );
        Bind( SceneAction.Main, KeyType.Hold, KeyCode.Alpha1, () => PressedSpeedControl( false ) );
        Bind( SceneAction.Main, KeyType.Up,   KeyCode.Alpha1, () => UpedSpeedControl() );
                                                           
        Bind( SceneAction.Main, KeyType.Down, KeyCode.Alpha2, () => SpeedControlProcess( true ) );
        Bind( SceneAction.Main, KeyType.Hold, KeyCode.Alpha2, () => PressedSpeedControl( true ) );
        Bind( SceneAction.Main, KeyType.Up,   KeyCode.Alpha2, () => UpedSpeedControl() );

        Bind( SceneAction.Main, KeyType.Down, KeyCode.F1, () => GameSetting.IsAutoRandom = !GameSetting.IsAutoRandom );
    }
}
