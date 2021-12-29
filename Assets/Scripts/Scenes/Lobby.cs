using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lobby : Scene
{
    public string soundName;

    public GameObject optionCanvas;
    float masterVolume,bgVolume;

    protected override void Awake()
    {
        base.Awake();

        SoundManager.Inst.LoadBgm( System.IO.Path.Combine( Application.streamingAssetsPath, "Default", soundName + ".mp3" ), 
                                SOUND_LOAD_TYPE.DEFAULT, SOUND_PLAY_MODE.LOOP );
        SoundManager.Inst.PlayBgm();

        ChangeKeyAction( SceneAction.Lobby );
        masterVolume = SoundManager.Inst.GetVolume( CHANNEL_GROUP_TYPE.MASTER );
        bgVolume     = SoundManager.Inst.GetVolume( CHANNEL_GROUP_TYPE.BGM );
    }
    
    public override void KeyBind()
    {
        StaticSceneKeyAction scene = new StaticSceneKeyAction();
        scene.Bind( KeyCode.Return, KeyType.Down, () => SceneChanger.Inst.LoadScene( SCENE_TYPE.FREESTYLE ) );

        scene.Bind( KeyCode.Space, KeyType.Down, () => optionCanvas.SetActive( true ) );
        scene.Bind( KeyCode.Space, KeyType.Down, () => ChangeKeyAction( SceneAction.LobbyOption ) );
        scene.Bind( KeyCode.Space, KeyType.Down, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.RETURN ) );

        KeyBind( SceneAction.Lobby, scene );
    }
}
