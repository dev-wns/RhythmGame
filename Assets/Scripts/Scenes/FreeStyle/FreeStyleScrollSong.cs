using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeStyleScrollSong : SceneScrollOption
{
    public OptionBase songPrefab; // sound infomation prefab

    public delegate void DelSelectSong( Song _song );
    public event DelSelectSong OnSelectSong;

    private Song currentSong;
    private float playback;
    private uint previewTime;
    private readonly uint waitPreviewTime = 500;

    protected override void Awake()
    {
        base.Awake();

        Select( NowPlaying.Inst.CurrentSongIndex );
    }

    protected override void Start()
    {
        base.Start();

        OptionProcess();
    }

    private void Update()
    {
        playback += Time.deltaTime * 1000f;

        if ( SoundManager.Inst.Length + waitPreviewTime < playback &&
             !SoundManager.Inst.IsPlaying( ChannelGroupType.BGM ) )
        {
            SoundManager.Inst.PlayBgm();
            SoundManager.Inst.SetPosition( GetPreviewTime() );
            playback = previewTime;
        }
    }

    protected override void CreateOptions()
    {
        options.Capacity = NowPlaying.Inst.Count;
        for ( int i = 0; i < NowPlaying.Inst.Count; i++ )
        {
            // scrollview song contents
            var song = Instantiate( songPrefab, content );
            var info = song.GetComponent<SongInfomation>();
            info.SetInfo( NowPlaying.Inst.GetSong( i ) );

            options.Add( song );
        }
    }

    public override void PrevMove()
    {
        base.PrevMove();
        if ( !IsLoop && IsDuplicate ) return;

        OptionProcess();
    }

    public override void NextMove()
    {
        base.NextMove();
        if ( !IsLoop && IsDuplicate ) return;

        OptionProcess();
    }

    private void OptionProcess()
    {
        NowPlaying.Inst.SelectSong( currentIndex );

        currentSong = NowPlaying.Inst.CurrentSong;
        Globals.Timer.Start();
        {
            OnSelectSong( currentSong );

            SoundManager.Inst.LoadBgm( currentSong.audioPath, SoundLoadType.Stream );
            SoundManager.Inst.PlayBgm();
        }
        Debug.Log( $"Select Song {Globals.Timer.End} ms" );

        previewTime = GetPreviewTime();
        SoundManager.Inst.SetPosition( previewTime );
        playback = previewTime;
    }

    private uint GetPreviewTime()
    {
        int time = currentSong.previewTime;
        if ( time <= 0 ) return ( uint )( SoundManager.Inst.Length * 0.3141592f );
        else             return ( uint )currentSong.previewTime;
    }


    public override void KeyBind()
    {
        currentScene.Bind( SceneAction.Main, KeyCode.UpArrow, () => PrevMove() );
        currentScene.Bind( SceneAction.Main, KeyCode.UpArrow, () => SoundManager.Inst.PlaySfx( SoundSfxType.Move ) );

        currentScene.Bind( SceneAction.Main, KeyCode.DownArrow, () => NextMove() );
        currentScene.Bind( SceneAction.Main, KeyCode.DownArrow, () => SoundManager.Inst.PlaySfx( SoundSfxType.Move ) );
    }
}
