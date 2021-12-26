using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lobby : Scene
{
    public ParticleSystem particle;
    public string soundName;

    public GameObject optionCanvas;

    protected override void Awake()
    {
        base.Awake();

        SoundManager.Inst.Load( System.IO.Path.Combine( Application.streamingAssetsPath, "Default", soundName + ".mp3" ), 
                                Sound.LoadType.Default, Sound.Mode.Loop );
        SoundManager.Inst.Play();

        ChangeKeyAction( SceneAction.Lobby );
    }

    protected override void Update()
    {
        base.Update();
    }

    public override void KeyBind()
    {
        StaticSceneKeyAction scene = new StaticSceneKeyAction();
        scene.Bind( KeyCode.Return, KeyType.Down, () => particle.gameObject.SetActive( false ) );
        scene.Bind( KeyCode.Return, KeyType.Down, () => SceneChanger.Inst.LoadScene( SceneType.FreeStyle ) );

        scene.Bind( KeyCode.Space, KeyType.Down, () => optionCanvas.SetActive( true ) );
        scene.Bind( KeyCode.Space, KeyType.Down, () => ChangeKeyAction( SceneAction.LobbyOption ) );

        KeyBind( SceneAction.Lobby, scene );
    }
}
