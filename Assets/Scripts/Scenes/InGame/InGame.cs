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

    public event Action OnGameStart;
    public event Action OnScrollChanged;

    public event Action OnReLoad;

    protected override void Awake()
    {
        base.Awake();

        NowPlaying.Inst.Initialize();
    }

    private async void Start()
    {
        InputLock( true );
        OnSystemInitialize( NowPlaying.Inst.CurrentChart );
        Task LoadkeySoundAsyncTask = Task.Run( () => OnSystemInitializeThread( NowPlaying.Inst.CurrentChart ) );

        await LoadkeySoundAsyncTask;

        Debug.Log( " KeySample Async End " );
        StartCoroutine( Play() );
    }

    private void OnDestroy()
    {
        SoundManager.Inst.KeyRelease();
    }

    private IEnumerator Play()
    {
        yield return new WaitUntil( () => !NowPlaying.Inst.IsLoadKeySounds && !NowPlaying.Inst.IsLoadBackground );

        loadingCanvas.SetActive( false );

        OnGameStart?.Invoke();
        InputLock( false );
        NowPlaying.Inst.Play();
    }

    public void BackToLobby()
    {
        var judge = GameObject.FindGameObjectWithTag( "Judgement" );
        Destroy( judge );
        SceneChanger.Inst.LoadScene( SceneType.FreeStyle );
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
                SceneChanger.Inst.LoadScene( SceneType.Result );
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

        Bind( SceneAction.Main, KeyCode.Alpha1, () => GameSetting.ScrollSpeed -= .1d );
        Bind( SceneAction.Main, KeyCode.Alpha1, () => SoundManager.Inst.Play( SoundSfxType.Slider ) );
        Bind( SceneAction.Main, KeyCode.Alpha1, () => OnScrollChanged?.Invoke() );

        Bind( SceneAction.Main, KeyCode.Alpha2, () => GameSetting.ScrollSpeed += .1d );
        Bind( SceneAction.Main, KeyCode.Alpha2, () => SoundManager.Inst.Play( SoundSfxType.Slider ) );
        Bind( SceneAction.Main, KeyCode.Alpha2, () => OnScrollChanged?.Invoke() );

    }
}
