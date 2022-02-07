using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGame : Scene
{
    public GameObject pauseCanvas;

    public delegate void DelSystemInitialize( in Chart _chart );
    public event DelSystemInitialize OnSystemInitialize;

    public event Action OnGameStart;
    public event Action OnScrollChanged;

    public event Action OnReLoad;

    protected override void Awake()
    {
        base.Awake();

        NowPlaying.Inst.Initialize();
    }

    private void Start()
    {
        OnSystemInitialize( NowPlaying.Inst.CurrentChart );
        OnGameStart();

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
                SoundManager.Inst.Play( SoundSfxType.Return );
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
            ChangeAction( SceneAction.Main );
        }
    }

    public override void KeyBind()
    {
        Bind( SceneAction.Main, KeyCode.Escape, () => Pause( true ) );

        Bind( SceneAction.Main, KeyCode.Alpha1, () => GameSetting.ScrollSpeed -= .1d );
        Bind( SceneAction.Main, KeyCode.Alpha1, () => SoundManager.Inst.Play( SoundSfxType.Decrease ) );
        Bind( SceneAction.Main, KeyCode.Alpha1, () => OnScrollChanged?.Invoke() );

        Bind( SceneAction.Main, KeyCode.Alpha2, () => GameSetting.ScrollSpeed += .1d );
        Bind( SceneAction.Main, KeyCode.Alpha2, () => SoundManager.Inst.Play( SoundSfxType.Increase ) );
        Bind( SceneAction.Main, KeyCode.Alpha2, () => OnScrollChanged?.Invoke() );

        Bind( SceneAction.Option, KeyCode.Escape, () => Pause( false ) );
        Bind( SceneAction.Option, KeyCode.Escape, () => SoundManager.Inst.Play( SoundSfxType.Escape ) );
    }
}
