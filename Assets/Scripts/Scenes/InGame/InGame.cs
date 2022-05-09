using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class InGame : Scene
{
    public GameObject loadingCanvas;
    public GameObject pauseCanvas;

    public delegate void DelSystemInitialize( in Chart _chart );
    public event DelSystemInitialize OnSystemInitialize;
    public event DelSystemInitialize OnSystemInitializeThread;

    public event Action OnKeySoundLoadEnd;
    public event Action OnGameStart;
    public event Action OnReLoad;

    private readonly float AdditionalLoadTime = 1f;

    protected override void Awake()
    {
        base.Awake();

        NowPlaying.Inst.ParseChart();
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
        yield return new WaitUntil( () => NowPlaying.Inst.IsLoadKeySounds );
        OnKeySoundLoadEnd?.Invoke();

        yield return new WaitUntil( () => NowPlaying.Inst.IsLoadBackground );

        yield return YieldCache.WaitForSeconds( AdditionalLoadTime );

        loadingCanvas.SetActive( false );

        OnGameStart?.Invoke();
        InputLock( false );
        NowPlaying.Inst.Play();
    }

    public void BackToLobby()
    {
        var judge = GameObject.FindGameObjectWithTag( "Judgement" );
        Destroy( judge );
        LoadScene( SceneType.FreeStyle );
    }

    public void Restart()
    {
        ChangeAction( SceneAction.Main );
        pauseCanvas.SetActive( false );
        NowPlaying.Inst.Stop();
        SoundManager.Inst.AllStop();

        OnReLoad?.Invoke();

        OnGameStart?.Invoke();
        NowPlaying.Inst.Play();
    }

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
        Bind( SceneAction.Main, KeyType.Hold, KeyCode.Alpha1, () => PressdSpeedControl( false ) );
        Bind( SceneAction.Main, KeyType.Up,   KeyCode.Alpha1, () => UpedSpeedControl() );
                                                           
        Bind( SceneAction.Main, KeyType.Down, KeyCode.Alpha2, () => SpeedControlProcess( true ) );
        Bind( SceneAction.Main, KeyType.Hold, KeyCode.Alpha2, () => PressdSpeedControl( true ) );
        Bind( SceneAction.Main, KeyType.Up,   KeyCode.Alpha2, () => UpedSpeedControl() );
    }
}
