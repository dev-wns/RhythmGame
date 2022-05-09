using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lobby : Scene
{
    public string soundName;

    public GameObject optionCanvas;
    public GameObject exitCanvas;

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
        SoundManager.Inst.LoadBgm( $@"{Application.streamingAssetsPath}\\Default\\Sounds\\Bgm\\{soundName + ".mp3"}", true, false, true );
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

    private void GotoFreeStyle()
    {
        if ( NowPlaying.Inst.IsParseSongs )
        {
            LoadScene( SceneType.FreeStyle );
            SoundManager.Inst.Play( SoundSfxType.MainClick );
        }
    }

    public override void KeyBind()
    {
        Bind( SceneAction.Main, KeyCode.Return, GotoFreeStyle );

        Bind( SceneAction.Main, KeyCode.Space, () => optionCanvas.SetActive( true ) );
        Bind( SceneAction.Main, KeyCode.Space, () => ChangeAction( SceneAction.Option ) );
        Bind( SceneAction.Main, KeyCode.Space, () => SoundManager.Inst.Play( SoundSfxType.MenuClick ) );

        Bind( SceneAction.Main, KeyCode.Escape, () => exitCanvas.SetActive( true ) );
        Bind( SceneAction.Main, KeyCode.Escape, () => ChangeAction( SceneAction.Exit ) );
        Bind( SceneAction.Main, KeyCode.Escape, () => SoundManager.Inst.Play( SoundSfxType.MenuClick ) );
    }
}