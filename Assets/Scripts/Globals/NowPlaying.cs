using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NowPlaying : SingletonUnity<NowPlaying>
{
    private List<Song> Songs = new List<Song>();
    public Song CurrentSong   { get; private set; }
    public Chart CurrentChart { get; private set; }

    public int Count { get { return Songs.Count; } }
    public int CurrentSongIndex { get; private set; }
    public float MedianBpm { get; private set; }

    public static float Playback        { get; private set; } // 노래 재생 시간
    public static float PlaybackChanged { get; private set; } // BPM 변화에 따른 노래 재생 시간

    public bool IsPlaying, IsMusicStart;
    private readonly int waitTime = -3000;

    private void Awake()
    {
        using ( FileConverter converter = new FileConverter() )
        {
            converter.ReLoad();
        }

        using ( FileParser parser = new FileParser() )
        {
            parser.TryParseArray( ref Songs );
        }

        if ( Songs.Count > 0 ) { SelectSong( 0 ); }
    }

    private void Update()
    {
        if ( !IsPlaying ) return;

        Playback += Time.deltaTime * 1000f;
        PlaybackChanged = GetChangedTime( Playback );
    }

    public void Play() => StartCoroutine( MusicStart() );

    private IEnumerator MusicStart()
    {
        SoundManager.Inst.LoadBgm( CurrentSong.audioPath );
        SoundManager.Inst.PlayBgm( true );
        IsPlaying = true;

        yield return new WaitUntil( () => Playback >= 0 );

        Playback = 0;
        SoundManager.Inst.PauseBgm( false );
        IsMusicStart = true;
    }

    public void Stop()
    {
        IsPlaying = false;
        IsMusicStart = false;
    }

    public void ChartUpdate()
    {
        IsPlaying = IsMusicStart = false;
        Playback = waitTime; 
        PlaybackChanged = 0;

        using ( FileParser parser = new FileParser() )
        {
            Chart chart;
            parser.TryParse( CurrentSong.filePath, out chart );
            CurrentChart = chart;
        }
    }

    public void SelectSong( int _index )
    {
        if ( _index < 0 || _index > Songs.Count - 1 )
        {
            Debug.Log( $"Sound Select Out Of Range. Index : {_index}" );
            return;
        }

        CurrentSongIndex = _index;
        CurrentSong = Songs[_index];
        MedianBpm = Songs[_index].medianBpm;
    }

    public Song GetSong( int _index )
    {
        if ( _index > Count )
        {
            Debug.Log( $"Sound Select Out Of Range. Index : {_index}" );
            return new Song();
        }

        return Songs[_index];
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
