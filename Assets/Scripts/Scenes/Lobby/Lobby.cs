using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lobby : Scene
{
    public string soundName;

    public GameObject optionCanvas;
    public GameObject exitCanvas;

    private float playback;
    private bool isStart = false;

    protected override void Awake()
    {
        base.Awake();

        SoundManager.Inst.LoadBgm( System.IO.Path.Combine( "Assets", "Sounds", "Bgms", soundName + ".mp3" ), 
                                   SoundLoadType.Default, SoundPlayMode.Loop );
        SoundManager.Inst.PlayBgm( true );
        SoundManager.Inst.OnSoundSystemReLoad += SoundReStart;
        SoundManager.Inst.PauseBgm( false );
        isStart = true;
    }

    private void SoundReStart()
    {
        SoundManager.Inst.LoadBgm( System.IO.Path.Combine( "Assets", "Sounds", "Bgms", soundName + ".mp3" ),
                                   SoundLoadType.Default, SoundPlayMode.Loop );
        SoundManager.Inst.PlayBgm( true );
        SoundManager.Inst.SetPosition( ( uint )playback );
        SoundManager.Inst.PauseBgm( false );
    }

    protected override void Update()
    {
        base.Update();
        if ( !isStart ) return;

        playback += Time.deltaTime * 1000f;
        if( playback >= SoundManager.Inst.Length )
            playback = 0;
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
