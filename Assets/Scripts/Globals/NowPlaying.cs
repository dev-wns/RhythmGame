using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NowPlaying : SingletonUnity<NowPlaying>
{
    private List<Song> songs = new List<Song>();
    public Song  CurrentSong   
    {
        get 
        {
            if ( CurrentSongIndex >= Count ) 
                 throw new System.Exception( "Out of Range. " );

            return songs[CurrentSongIndex];
        } 
    }
    public  Chart CurrentChart  { get { return currentChart; } }
    private Chart currentChart;

    public int Count 
    {
        get 
        {
            if ( songs is null ) return 0;
            else                 return songs.Count; 
        } 
    }
    public int CurrentSongIndex { get; private set; }
    public float MedianBpm { get { return CurrentSong.medianBpm; } }

    public static float Playback        { get; private set; } // 노래 재생 시간
    public static float PlaybackChanged { get; private set; } // BPM 변화에 따른 노래 재생 시간

    public bool IsPlaying    { get; private set; }
    public bool IsMusicStart { get; private set; }
    private readonly int waitTime = -3000;

    private void Awake()
    {
        using ( FileConverter converter = new FileConverter() )
                converter.ReLoad();

        using ( FileParser parser = new FileParser() )
                parser.ParseFilesInDirectories( ref songs );

        currentChart.notes   ??= new List<Note>();
        currentChart.timings ??= new List<Timing>();

        if ( songs.Count > 0 ) { SelectSong( 0 ); }
    }

    private void Update()
    {
        if ( !IsPlaying ) return;

        if ( !IsMusicStart ) Playback += Time.deltaTime * 1000f;
        else                 Playback  = Globals.Timer.elapsedMilliSeconds;
        PlaybackChanged = GetChangedTime( Playback );
    }

    public void Play() => StartCoroutine( MusicStart() );

    private IEnumerator MusicStart()
    {
        SoundManager.Inst.LoadBgm( CurrentSong.audioPath );
        SoundManager.Inst.PlayBgm( true );
        IsPlaying = true;

        yield return new WaitUntil( () => Playback >= 0 );

        Globals.Timer.Start();
        SoundManager.Inst.PauseBgm( false );
        IsMusicStart = true;
    }

    public void Stop()
    {
        IsPlaying = IsMusicStart = false;
    }

    public void ChartUpdate()
    {
        IsPlaying = IsMusicStart = false;
        Playback = waitTime; 
        PlaybackChanged = 0;

        using ( FileParser parser = new FileParser() )
        {
            currentChart.notes ??= new List<Note>();
            currentChart.notes?.Clear();

            currentChart.timings ??= new List<Timing>();
            currentChart.timings?.Clear();

            parser.TryParse( CurrentSong.filePath, out currentChart );
        }
    }

    public void SelectSong( int _index )
    {
        if ( _index < 0 || _index > songs.Count - 1 )
        {
            Debug.Log( $"Sound Select Out Of Range. Index : {_index}" );
            return;
        }

        CurrentSongIndex = _index;
    }

    public Song GetSong( int _index )
    {
        if ( _index > Count )
        {
            Debug.Log( $"Sound Select Out Of Range. Index : {_index}" );
            return new Song();
        }

        return songs[_index];
    }

    public static float GetChangedTime( float _time ) // BPM 변화에 따른 시간 계산
    {
        var timings = Inst.CurrentChart.timings;
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
