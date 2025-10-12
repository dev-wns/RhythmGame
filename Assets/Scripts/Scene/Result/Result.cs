using UnityEngine;

public class Result : Scene
{
    public string soundName;
    public uint highlightPos;
    //private float playback, soundLength;
    //private bool isStart = false;

    protected override void Awake()
    {
        base.Awake();

        QualitySettings.antiAliasing = 8;

        //AudioManager.Inst.Load( $@"{Application.streamingAssetsPath}\\Default\\Sounds\\Bgm\\{soundName}", true, false );
        //AudioManager.Inst.Play();
        //soundLength = AudioManager.Inst.Length;
        //playback    = AudioManager.Inst.Position = highlightPos;

        //AudioManager.Inst.Fade( AudioManager.Inst.MainChannel, 0f, 1f, 2f );

        if ( !GameSetting.HasFlag( GameMode.AutoPlay ) )
              DataStorage.Inst.CreateNewRecord();

        //isStart = true;
    }

    //private void Update()
    //{
    //    if ( !isStart ) return;

    //    playback += Time.deltaTime * 1000f;
    //    if ( playback <= highlightPos && playback >= 0 )
    //    {
    //        AudioManager.Inst.Position = highlightPos;
    //        playback = highlightPos;
    //    }

    //    if ( playback > soundLength )
    //    {
    //        playback = AudioManager.Inst.Position;
    //    }
    //}

    public async void BackToLobby()
    {
        //DataStorage.Inst.Clear();
        await LoadScene( SceneType.FreeStyle );
        AudioManager.Inst.Play( SFX.MainClick );
    }

    public override void Connect() { }

    public override void Disconnect() { }

    public override void KeyBind()
    {
        Bind( ActionType.Main, KeyCode.Escape, BackToLobby );
        Bind( ActionType.Main, KeyCode.Return, BackToLobby );
    }
}