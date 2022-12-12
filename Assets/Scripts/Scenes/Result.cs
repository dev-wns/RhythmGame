using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Result : Scene
{
    public Judgement Judge = null;


    public string soundName;
    public uint highlightPos;
    private float playback, soundLength;
    private bool isStart = false;

    protected override void Awake()
    {
        base.Awake();

        var obj = GameObject.FindGameObjectWithTag( "Judgement" );
        obj?.TryGetComponent( out Judge );

        SoundManager.Inst.Load( $@"{Application.streamingAssetsPath}\\Default\\Sounds\\Bgm\\{soundName}", true, false );
        SoundManager.Inst.Play();
        soundLength = SoundManager.Inst.Length;
        playback = SoundManager.Inst.Position = highlightPos;

        SoundManager.Inst.FadeVolume( 0f, SoundManager.Inst.Volume, 2f );

        isStart = true;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Destroy( Judge );
    }

    protected override void Update()
    {
        base.Update();
        if ( !isStart ) return;

        playback += Time.deltaTime * 1000f;
        if ( playback <= highlightPos && playback >= 0 )
        {
            SoundManager.Inst.Position = highlightPos;
            playback = highlightPos;
        }

        if ( playback > soundLength )
        {
            playback = SoundManager.Inst.Position;
        }
    }

    public override void Connect() { }

    public override void Disconnect() { }

    public override void KeyBind()
    {
        Bind( ActionType.Main, KeyCode.Escape, () => LoadScene( SceneType.FreeStyle ) );
        Bind( ActionType.Main, KeyCode.Escape, () => SoundManager.Inst.Play( SoundSfxType.MainClick ) );
        Bind( ActionType.Main, KeyCode.Return, () => LoadScene( SceneType.FreeStyle ) );
        Bind( ActionType.Main, KeyCode.Return, () => SoundManager.Inst.Play( SoundSfxType.MainClick ) );
    }
}