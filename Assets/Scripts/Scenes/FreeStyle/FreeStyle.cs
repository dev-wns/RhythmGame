using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FreeStyle : Scene
{
    //public VerticalScrollSound scrollSound;
    public GameObject optionCanvas;

    protected override void Awake()
    {
        base.Awake();

        ChangeAction( SceneAction.FreeStyle );
    }

    public override void KeyBind()
    {
        Bind( SceneAction.FreeStyle, KeyCode.Space, () => optionCanvas.SetActive( true ) );
        Bind( SceneAction.FreeStyle, KeyCode.Space, () => SoundManager.Inst.UseLowEqualizer( true ) );
        Bind( SceneAction.FreeStyle, KeyCode.Space, () => ChangeAction( SceneAction.FreeStyleOption ) );
              
        Bind( SceneAction.FreeStyle, KeyCode.Escape, () => SceneChanger.Inst.LoadScene( SCENE_TYPE.LOBBY ) );
    }
}