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
                                Sound.LoadType.Default, Sound.Mode.Loop );
        SoundManager.Inst.Play();

        ChangeKeyAction( SceneAction.Lobby );
        masterVolume = SoundManager.Inst.GetVolume( Sound.ChannelType.MasterGroup );
        bgVolume     = SoundManager.Inst.GetVolume( Sound.ChannelType.BackgroundGroup );
    }

    protected override void Update()
    {
        base.Update();
    }

    
    public override void KeyBind()
    {
        StaticSceneKeyAction scene = new StaticSceneKeyAction();
        scene.Bind( KeyCode.Return, KeyType.Down, () => SceneChanger.Inst.LoadScene( SceneType.FreeStyle ) );

        scene.Bind( KeyCode.Space, KeyType.Down, () => optionCanvas.SetActive( true ) );
        scene.Bind( KeyCode.Space, KeyType.Down, () => ChangeKeyAction( SceneAction.LobbyOption ) );

        KeyBind( SceneAction.Lobby, scene );
    }
}
