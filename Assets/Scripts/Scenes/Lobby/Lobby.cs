using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lobby : Scene
{
    public string soundName;

    public GameObject loadIcon;

    private float playback, soundLength;
    private bool isStart = false;

    protected override void Awake()
    {
        base.Awake();
        SoundManager.Inst.OnSoundSystemReLoad += SoundReStart;
        SoundReStart();

        StartCoroutine( LoadingEndCheck() );
        isStart = true;
    }

    private IEnumerator LoadingEndCheck()
    {
        if ( !NowPlaying.Inst.IsParseSongs )
             loadIcon.SetActive( true );

        yield return new WaitUntil( () => NowPlaying.Inst.IsParseSongs );

        loadIcon.SetActive( false );
    }

    private void SoundReStart()
    {
        SoundManager.Inst.LoadBgm( $@"{Application.streamingAssetsPath}\\Default\\Sounds\\Bgm\\{soundName}", true, false, true );
        SoundManager.Inst.Play( true );
        SoundManager.Inst.Position = ( uint )playback;
        soundLength = SoundManager.Inst.Length;
        SoundManager.Inst.SetPaused( false, ChannelType.BGM );
    }

    protected override void Update()
    {
        base.Update();
        if ( !isStart ) return;

        playback += Time.deltaTime * 1000f;
        if( playback >= soundLength )
            playback = 0;
    }

    public override void KeyBind()
    {
  
    }
}