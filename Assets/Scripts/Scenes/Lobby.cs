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

        SoundManager.Inst.Load( System.IO.Path.Combine( Application.streamingAssetsPath, "Default", soundName + ".mp3" ), 
                                SOUND_LOAD_TYPE.DEFAULT, SOUND_PLAY_MODE.LOOP );
        SoundManager.Inst.Play();

        ChangeKeyAction( SceneAction.Lobby );
        masterVolume = SoundManager.Inst.GetVolume( SOUND_GROUP_TYPE.MASTER );
        bgVolume     = SoundManager.Inst.GetVolume( SOUND_GROUP_TYPE.BACKGROUND );

        SoundManager.Inst.InterfaceSfxLoad( "Assets/Sounds/InterfaceSfx/confirm_style_2_001.wav", 0 );
        SoundManager.Inst.InterfaceSfxLoad( "Assets/Sounds/InterfaceSfx/confirm_style_2_echo_001.wav", 1 );
    }

    protected override void Update()
    {
        base.Update();

        if ( Input.GetKeyDown( KeyCode.A ) )
        {
            SoundManager.Inst.PlayInterfaceSfx( 0 );
        }
        if ( Input.GetKeyDown( KeyCode.S ) )
        {
            SoundManager.Inst.PlayInterfaceSfx( 1 );
        }
    }

    
    public override void KeyBind()
    {
        StaticSceneKeyAction scene = new StaticSceneKeyAction();
        scene.Bind( KeyCode.Return, KeyType.Down, () => SceneChanger.Inst.LoadScene( SCENE_TYPE.FREESTYLE ) );

        scene.Bind( KeyCode.Space, KeyType.Down, () => optionCanvas.SetActive( true ) );
        scene.Bind( KeyCode.Space, KeyType.Down, () => ChangeKeyAction( SceneAction.LobbyOption ) );

        KeyBind( SceneAction.Lobby, scene );
    }
}
