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

    public override void KeyBind()
    {
        Bind( SceneAction.Main, KeyCode.Escape, () => pauseCanvas.SetActive( true ) );
        Bind( SceneAction.Main, KeyCode.Escape, () => SoundManager.Inst.PlaySfx( SoundSfxType.Return ) );
        Bind( SceneAction.Main, KeyCode.Escape, () => ChangeAction( SceneAction.Option ) );
        Bind( SceneAction.Main, KeyCode.Escape, () => NowPlaying.Inst.Pause( true ) );

        Bind( SceneAction.Main, KeyCode.Alpha1, () => GameSetting.ScrollSpeed -= 1f );
        Bind( SceneAction.Main, KeyCode.Alpha1, () => SoundManager.Inst.PlaySfx( SoundSfxType.Decrease ) );
        Bind( SceneAction.Main, KeyCode.Alpha1, () => OnScrollChanged?.Invoke() );

        Bind( SceneAction.Main, KeyCode.Alpha2, () => GameSetting.ScrollSpeed += 1f );
        Bind( SceneAction.Main, KeyCode.Alpha2, () => SoundManager.Inst.PlaySfx( SoundSfxType.Increase ) );
        Bind( SceneAction.Main, KeyCode.Alpha2, () => OnScrollChanged?.Invoke() );
    }
}
