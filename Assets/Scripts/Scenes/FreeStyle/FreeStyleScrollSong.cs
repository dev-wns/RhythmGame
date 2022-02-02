using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeStyleScrollSong : SceneScrollOption
{
    public OptionBase songPrefab; // sound infomation prefab

    public delegate void DelSelectSong( Song _song );
    public event DelSelectSong OnSelectSong;

    private Song curSong;
    private float playback;
    private float soundLength;
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

        if ( soundLength + waitPreviewTime < playback &&
             !SoundManager.Inst.IsPlaying( ChannelType.BGM ) )
        {
            SoundManager.Inst.Play();
            SoundManager.Inst.Position = GetPreviewTime();
            playback = previewTime;
        }
    }

    protected override void CreateOptions()
    {
        options.Capacity = NowPlaying.Inst.Songs.Count;
        for ( int i = 0; i < NowPlaying.Inst.Songs.Count; i++ )
        {
            // scrollview song contents
            var song = Instantiate( songPrefab, content );
            var info = song.GetComponent<SongInfomation>();
            info.SetInfo( NowPlaying.Inst.Songs[i] );

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
        NowPlaying.Inst.CurrentSongIndex = CurrentIndex;

        curSong = NowPlaying.Inst.CurrentSong;
        soundLength = curSong.totalTime;

        OnSelectSong( curSong );

        SoundManager.Inst.LoadBgm( curSong.audioPath, false, true, false );
        SoundManager.Inst.Play();

        previewTime = GetPreviewTime();
        SoundManager.Inst.Position = previewTime;
        playback = previewTime;
    }

    private uint GetPreviewTime()
    {
        int time = curSong.previewTime;
        if ( time <= 0 ) return ( uint )( soundLength * 0.3141592f );
        else             return ( uint )curSong.previewTime;
    }


    public override void KeyBind()
    {
        CurrentScene.Bind( SceneAction.Main, KeyCode.UpArrow, () => PrevMove() );
        CurrentScene.Bind( SceneAction.Main, KeyCode.UpArrow, () => SoundManager.Inst.Play( SoundSfxType.Move ) );

        CurrentScene.Bind( SceneAction.Main, KeyCode.DownArrow, () => NextMove() );
        CurrentScene.Bind( SceneAction.Main, KeyCode.DownArrow, () => SoundManager.Inst.Play( SoundSfxType.Move ) );
    }
}
