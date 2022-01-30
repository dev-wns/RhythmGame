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
    public event Action OnPause;

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
        SceneChanger.Inst.LoadScene( SceneType.FreeStyle );
    }

    public void Restart()
    {
        SceneChanger.Inst.LoadScene( SceneType.Game );
    }

    public void Pause( bool _isPuase )
    {
        if ( !NowPlaying.Inst.Pause( _isPuase ) )
        {
            NowPlaying.Inst.Stop();
        }
        else
        {
            pauseCanvas.SetActive( true );
            SoundManager.Inst.PlaySfx( SoundSfxType.Return );
            ChangeAction( SceneAction.Option );
            OnPause?.Invoke();
        }
    }

    public override void KeyBind()
    {
        Bind( SceneAction.Main, KeyCode.Escape, () => Pause( true ) );

        Bind( SceneAction.Main, KeyCode.Alpha1, () => GameSetting.ScrollSpeed -= 1f );
        Bind( SceneAction.Main, KeyCode.Alpha1, () => SoundManager.Inst.PlaySfx( SoundSfxType.Decrease ) );
        Bind( SceneAction.Main, KeyCode.Alpha1, () => OnScrollChanged?.Invoke() );

        Bind( SceneAction.Main, KeyCode.Alpha2, () => GameSetting.ScrollSpeed += 1f );
        Bind( SceneAction.Main, KeyCode.Alpha2, () => SoundManager.Inst.PlaySfx( SoundSfxType.Increase ) );
        Bind( SceneAction.Main, KeyCode.Alpha2, () => OnScrollChanged?.Invoke() );
    }
}
