using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FreeStyle : Scene
{
    public GameObject optionCanvas;

    protected override void Awake()
    {
        base.Awake();

        var judge = GameObject.FindGameObjectWithTag( "Judgement" );
        if ( judge ) Destroy( judge );
    }

    public override void KeyBind()
    {
        Bind( SceneAction.Main, KeyCode.Space, () => optionCanvas.SetActive( true ) );
        Bind( SceneAction.Main, KeyCode.Space, () => SoundManager.Inst.UseLowEqualizer( true ) );
        Bind( SceneAction.Main, KeyCode.Space, () => ChangeAction( SceneAction.Option ) );
        Bind( SceneAction.Main, KeyCode.Space, () => SoundManager.Inst.Play( SoundSfxType.MenuClick ) );

        Bind( SceneAction.Main, KeyCode.Escape, () => SceneChanger.Inst.LoadScene( SceneType.Lobby ) );
        Bind( SceneAction.Main, KeyCode.Escape, () => SoundManager.Inst.Play( SoundSfxType.MainHover ) );
    }
}