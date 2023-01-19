using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lobby : Scene
{
    public GameObject loadIcon;

    public string soundName;
    private float playback, soundLength;
    private bool isStart = false;

    protected override void Awake()
    {
        base.Awake();
        SoundManager.Inst.OnReload += SoundReStart;
    }

    protected override void Start()
    {
        base.Start();
        SoundReStart();
        StartCoroutine( LoadingEndCheck() );
        isStart = true;
    }

    private void Update()
    {
        if ( !isStart ) return;

        playback = playback < soundLength ? playback += Time.deltaTime * 1000f : 0;
    }

    public override void Connect() 
    {
        SoundManager.Inst.AddDSP( FMOD.DSP_TYPE.FFT, ChannelType.BGM ); 
    }

    public override void Disconnect()
    {
        SoundManager.Inst.RemoveDSP( FMOD.DSP_TYPE.FFT, ChannelType.BGM );
        SoundManager.Inst.OnReload -= SoundReStart;
    }

    private IEnumerator LoadingEndCheck()
    {
        if ( !NowPlaying.Inst.IsParseSong )
             loadIcon.SetActive( true );

        yield return new WaitUntil( () => NowPlaying.Inst.IsParseSong );

        loadIcon.SetActive( false );
    }

    private void SoundReStart()
    {
        SoundManager.Inst.AddDSP( FMOD.DSP_TYPE.FFT, ChannelType.BGM );
        SoundManager.Inst.Stop( new Music( SoundManager.Inst.MainSound, SoundManager.Inst.MainChannel ) );

        SoundManager.Inst.Load( $@"{Application.streamingAssetsPath}\\Default\\Sounds\\Bgm\\{soundName}", true, false );
        SoundManager.Inst.Play();
        SoundManager.Inst.Position = ( uint )playback;
        soundLength = SoundManager.Inst.Length;
    }

    public override void KeyBind() { }
}