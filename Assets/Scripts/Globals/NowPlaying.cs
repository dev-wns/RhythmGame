using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;

public class NowPlaying : SingletonUnity<NowPlaying>
{
    public ReadOnlyCollection<Song> Songs { get; private set; }

    public  Song CurrentSong => curSong;
    private Song curSong;

    public  Chart CurrentChart => curChart;
    private Chart curChart;

    public  int CurrentSongIndex 
    {
        get => curSongIndex;
        set
        {
            if ( value >= Songs.Count )
                throw new System.Exception( "Out of Range. " );

            curSongIndex = value;
            curSong      = Songs[value];
        }
    }
    private int curSongIndex;

    public static double Playback;        // 노래 재생 시간
    public static double PlaybackChanged; // BPM 변화에 따른 노래 재생 시간

    public bool IsPlaying { get; set; }
    private bool IsPause;
    public bool IsLoad { get; private set; } = false;
    private readonly double waitTime = -3d;
    private double startTime;
    private double savedTime;

    private void Awake()
    {
        //using ( FileConverter converter = new FileConverter() )
        //    converter.ReLoad();

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

        Playback = savedTime + ( System.DateTime.Now.TimeOfDay.TotalSeconds - startTime );
        PlaybackChanged = GetChangedTime( Playback );
    }

    public void Initialize()
    {
        IsLoad = IsPlaying = false;
        Playback = waitTime;
        PlaybackChanged = 0;

        using ( FileParser parser = new FileParser() )
        {
            parser.TryParse( curSong.filePath, out curChart );
        }
    }

    public void Play() => StartCoroutine( MusicStart() );

    public void Stop()
    {
        IsLoad = IsPlaying = false;
        Playback = waitTime;
        PlaybackChanged = 0;
    }

    public void Pause( bool _isPause )
    {
        if ( _isPause )
        {
            IsPlaying = false;
            SoundManager.Inst.Pause = true;
        }
        else
        {
            startTime = System.DateTime.Now.TimeOfDay.TotalSeconds;
            savedTime = ( SoundManager.Inst.Position * .001d );
            SoundManager.Inst.Pause = false;
            IsPlaying = true;
        }
    }

    private IEnumerator MusicStart()
    {
        SoundManager.Inst.LoadBgm( CurrentSong.audioPath, false, false, false );
        SoundManager.Inst.PlayBgm( true );
        IsPlaying = true;
        savedTime = waitTime;
        startTime = System.DateTime.Now.TimeOfDay.TotalSeconds;

        yield return new WaitUntil( () => Playback >= GameSetting.SoundOffset );

        SoundManager.Inst.Pause = false;
        //savedTime = ( SoundManager.Inst.Position * .001d );
        IsLoad = true;
    }

    public double GetChangedTime( double _time ) // BPM 변화에 따른 시간 계산
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
        return newTime;
    }
}
