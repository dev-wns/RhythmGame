using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Result : Scene
{
    public string soundName;
    public uint highlightPos;
    private float playback, soundLength;
    private bool isStart = false;
    
    protected override void Awake()
    {
        base.Awake();
        SoundManager.Inst.LoadBgm( $@"{Application.streamingAssetsPath}\\Default\\Sounds\\Bgm\\{soundName}", true, false, true );
        SoundManager.Inst.Play( 1f, true );
        soundLength = SoundManager.Inst.Length;
        playback = SoundManager.Inst.Position = highlightPos;

        SoundManager.Inst.FadeIn( 2f );
        SoundManager.Inst.SetPaused( false, ChannelType.BGM );

        SoundManager.Inst.RemovePitchShift();
        isStart = true;
    }

    protected override void Update()
    {
        base.Update();
        if ( !isStart )
            return;

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

    private void OnDestroy()
    {
        //SoundManager.Inst.SetVolume( volume, ChannelType.BGM );
    }

    public override void KeyBind()
    {
        Bind( SceneAction.Main, KeyCode.Escape, () => LoadScene( SceneType.FreeStyle ) );
        Bind( SceneAction.Main, KeyCode.Escape, () => SoundManager.Inst.Play( SoundSfxType.MainClick ) );
        Bind( SceneAction.Main, KeyCode.Return, () => LoadScene( SceneType.FreeStyle ) );
        Bind( SceneAction.Main, KeyCode.Return, () => SoundManager.Inst.Play( SoundSfxType.MainClick ) );
    }
}