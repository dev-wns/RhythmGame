using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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

        SoundManager.Inst.LoadBgm( $@"{Application.streamingAssetsPath}\\Default\\Sounds\\Bgm\\{soundName}", true, false, true );
        SoundManager.Inst.Play( true );
        soundLength = SoundManager.Inst.Length;
        playback = SoundManager.Inst.Position = highlightPos;

        SoundManager.Inst.FadeIn( 2f );
        SoundManager.Inst.SetPaused( false, ChannelType.BGM );

        isStart = true;
    }

    private void OnDestroy()
    {
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

    public override void KeyBind()
    {
        Bind( ActionType.Main, KeyCode.Escape, () => LoadScene( SceneType.FreeStyle ) );
        Bind( ActionType.Main, KeyCode.Escape, () => SoundManager.Inst.Play( SoundSfxType.MainClick ) );
        Bind( ActionType.Main, KeyCode.Return, () => LoadScene( SceneType.FreeStyle ) );
        Bind( ActionType.Main, KeyCode.Return, () => SoundManager.Inst.Play( SoundSfxType.MainClick ) );
    }
}