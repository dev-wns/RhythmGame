using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;

public class NowPlaying : SingletonUnity<NowPlaying>
{
    public ReadOnlyCollection<Song> Songs { get; private set; }

    public  Song CurrentSong => currentSong;
    private Song currentSong;

    public  Chart CurrentChart => currentChart;
    private Chart currentChart;

    public  int CurrentSongIndex 
    {
        get => currentSongIndex;
        set
        {
            if ( value >= Songs.Count )
                throw new System.Exception( "Out of Range. " );

            currentSongIndex = value;
            currentSong      = Songs[value];
        }
    }
    private int currentSongIndex;

    public static float Playback; // 노래 재생 시간
    public static float PlaybackChanged; // BPM 변화에 따른 노래 재생 시간

    public bool IsPlaying { get; set; }
    public bool IsLoad { get; private set; } = false;
    private readonly int waitTime = -3000;

    private void Awake()
    {
        using ( FileConverter converter = new FileConverter() )
            converter.ReLoad();

        using ( FileParser parser = new FileParser() )
        {
            ReadOnlyCollection<Song> songs;
            parser.ParseFilesInDirectories( out songs );
            Songs = songs;
        }

        CurrentSongIndex = 0; 
    }

    private void Update()
    {
        if ( !IsPlaying ) return;

        Playback += Time.deltaTime * 1000f;
        PlaybackChanged = GetChangedTime( Playback );

        if ( IsLoad )
        {
            Playback = SoundManager.Inst.Position;
            IsLoad = false;
        }
    }

    public void Initialize()
    {
        IsPlaying = false;
        Playback = waitTime;
        PlaybackChanged = 0;

        using ( FileParser parser = new FileParser() )
        {
            parser.TryParse( currentSong.filePath, out currentChart );
        }
    }

    public void Play() => StartCoroutine( MusicStart() );

    private IEnumerator MusicStart()
    {
        SoundManager.Inst.LoadBgm( CurrentSong.audioPath, false, false, false );
        SoundManager.Inst.PlayBgm( true );
        IsPlaying = true;

        yield return new WaitUntil( () => Playback >= GameSetting.SoundOffset );

        SoundManager.Inst.Pause = false;
        IsLoad = true;
    }

    public float GetChangedTime( float _time ) // BPM 변화에 따른 시간 계산
    {
        var timings = CurrentChart.timings;
        double newTime = _time;
        double prevBpm = 0d;
        for ( int i = 0; i < timings.Count; i++ )
        {
            double time = timings[i].time;
            double bpm  = timings[i].bpm;

            if ( time > _time ) break;
            newTime += ( bpm - prevBpm ) * ( _time - time );
            prevBpm = bpm;
        }
        return ( float )newTime;
    }
}
