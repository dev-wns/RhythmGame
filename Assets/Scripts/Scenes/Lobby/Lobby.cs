using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lobby : Scene
{
    public string soundName;

    public GameObject optionCanvas;
    public GameObject exitCanvas;

    protected override void Awake()
    {
        base.Awake();

        SoundManager.Inst.LoadBgm( System.IO.Path.Combine( "Assets", "Sounds", "Bgms", soundName + ".mp3" ), 
                                   SoundLoadType.Default, SoundPlayMode.Loop );
        SoundManager.Inst.PlayBgm();
    }
    
    public override void KeyBind()
    {
        Bind( SceneAction.Main, KeyCode.Return, () => SceneChanger.Inst.LoadScene( SceneType.FreeStyle ) );

        Bind( SceneAction.Main, KeyCode.Space, () => optionCanvas.SetActive( true ) );
        Bind( SceneAction.Main, KeyCode.Space, () => ChangeAction( SceneAction.Option ) );
        Bind( SceneAction.Main, KeyCode.Space, () => SoundManager.Inst.PlaySfx( SoundSfxType.Return ) );

        Bind( SceneAction.Main, KeyCode.Escape, () => exitCanvas.SetActive( true ) );
        Bind( SceneAction.Main, KeyCode.Escape, () => ChangeAction( SceneAction.Exit ) );
        Bind( SceneAction.Main, KeyCode.Escape, () => SoundManager.Inst.PlaySfx( SoundSfxType.Return ) );
    }
}
