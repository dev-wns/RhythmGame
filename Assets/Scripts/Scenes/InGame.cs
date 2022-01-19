using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGame : Scene
{
    public delegate void DelSystemInitialize( in Chart _chart );
    public event DelSystemInitialize OnSystemInitialize;

    public event Action OnGameStart;
    public event Action OnScrollChanged;

    private void Start()
    {
        OnSystemInitialize( NowPlaying.Inst.CurrentChart );
        OnGameStart();
        NowPlaying.Inst.Play();
        Debug.Log( NowPlaying.Inst.CurrentSong.medianBpm );
    }

    public override void KeyBind()
    {
        Bind( SceneAction.Main, KeyCode.Escape, () => SceneChanger.Inst.LoadScene( SceneType.FreeStyle ) );

        Bind( SceneAction.Main, KeyCode.Alpha1, () => GameSetting.ScrollSpeed -= 1 );
        Bind( SceneAction.Main, KeyCode.Alpha1, () => OnScrollChanged?.Invoke() );

        Bind( SceneAction.Main, KeyCode.Alpha2, () => GameSetting.ScrollSpeed += 1 );
        Bind( SceneAction.Main, KeyCode.Alpha2, () => OnScrollChanged?.Invoke() );
    }
}
