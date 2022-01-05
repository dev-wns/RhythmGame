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
                                   SOUND_LOAD_TYPE.DEFAULT, SOUND_PLAY_MODE.LOOP );
        SoundManager.Inst.PlayBgm();

        ChangeAction( SceneAction.Lobby );
    }
    
    public override void KeyBind()
    {
        Bind( SceneAction.Lobby, KeyCode.Return, () => SceneChanger.Inst.LoadScene( SCENE_TYPE.FREESTYLE ) );

        Bind( SceneAction.Lobby, KeyCode.Space, () => optionCanvas.SetActive( true ) );
        Bind( SceneAction.Lobby, KeyCode.Space, () => ChangeAction( SceneAction.LobbyOption ) );
        Bind( SceneAction.Lobby, KeyCode.Space, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.RETURN ) );

        Bind( SceneAction.Lobby, KeyCode.Escape, () => exitCanvas.SetActive( true ) );
        Bind( SceneAction.Lobby, KeyCode.Escape, () => ChangeAction( SceneAction.Exit ) );
        Bind( SceneAction.Lobby, KeyCode.Escape, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.RETURN ) );
    }
}
