using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Result : Scene
{
    protected override void Awake()
    {
        base.Awake();

        SoundManager.Inst.LoadBgm( $@"{Application.streamingAssetsPath}\\Default\\Sounds\\Bgm\\Cover - Patrick Patrikios.mp3", true, false, true );
        SoundManager.Inst.Play();
    }

    public override void KeyBind()
    {
        Bind( SceneAction.Main, KeyCode.Escape, () => SceneChanger.Inst.LoadScene( SceneType.FreeStyle ) );
        Bind( SceneAction.Main, KeyCode.Escape, () => SoundManager.Inst.Play( SoundSfxType.Return ) );
        Bind( SceneAction.Main, KeyCode.Return, () => SceneChanger.Inst.LoadScene( SceneType.FreeStyle ) );
        Bind( SceneAction.Main, KeyCode.Return, () => SoundManager.Inst.Play( SoundSfxType.Return ) );
    }
}