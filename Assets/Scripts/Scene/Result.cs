using UnityEngine;

public class Result : Scene
{
    public string soundName;
    public uint highlightPos;
    private float playback, soundLength;
    private bool isStart = false;

    protected override void Awake()
    {
        base.Awake();

        QualitySettings.antiAliasing = 8;

        SoundManager.Inst.Load( $@"{Application.streamingAssetsPath}\\Default\\Sounds\\Bgm\\{soundName}", true, false );
        SoundManager.Inst.Play();
        soundLength = SoundManager.Inst.Length;
        playback = SoundManager.Inst.Position = highlightPos;

        SoundManager.Inst.FadeVolume( 0f, SoundManager.Inst.Volume, 2f );

        isStart = true;
    }

    private void Update()
    { 
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

    public void BackToLobby()
    {
        NowPlaying.Inst.ResetData();
        LoadScene( SceneType.FreeStyle );
        SoundManager.Inst.Play( SoundSfxType.MainClick );
    }

    public override void Connect() { }

    public override void Disconnect() { }

    public override void KeyBind()
    {
        Bind( ActionType.Main, KeyCode.Escape, BackToLobby );
        Bind( ActionType.Main, KeyCode.Return, BackToLobby );
    }
}