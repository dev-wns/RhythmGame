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

        ChangeKeyAction( SceneAction.Lobby );
    }
    
    public override void KeyBind()
    {
        StaticSceneKeyAction scene = new StaticSceneKeyAction();
        scene.Bind( KeyCode.Return, KeyType.Down, () => SceneChanger.Inst.LoadScene( SCENE_TYPE.FREESTYLE ) );

        scene.Bind( KeyCode.Space, KeyType.Down, () => optionCanvas.SetActive( true ) );
        scene.Bind( KeyCode.Space, KeyType.Down, () => ChangeKeyAction( SceneAction.LobbyOption ) );
        scene.Bind( KeyCode.Space, KeyType.Down, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.RETURN ) );

        scene.Bind( KeyCode.Escape, KeyType.Down, () => exitCanvas.SetActive( true ) );
        scene.Bind( KeyCode.Escape, KeyType.Down, () => ChangeKeyAction( SceneAction.Exit ) );
        scene.Bind( KeyCode.Escape, KeyType.Down, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.ESCAPE ) );

        KeyBind( SceneAction.Lobby, scene );
    }
}
