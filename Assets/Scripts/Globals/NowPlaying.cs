using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NowPlaying : SingletonUnity<NowPlaying>
{
    public static Chart CurrentChart { get; private set; }
    public static float Playback        { get; private set; } // 노래 재생 시간
    public static float PlaybackChanged { get; private set; } // BPM 변화에 따른 노래 재생 시간

    public bool IsPlaying, IsMusicStart;
    private readonly int waitTime = -3000;

    private void Update()
    {
        if ( !IsPlaying ) return;

        Playback += Time.deltaTime * 1000f;
        PlaybackChanged = GetChangedTime( Playback );
    }

    public void Play() => StartCoroutine( MusicStart() );

    private IEnumerator MusicStart()
    {
        SoundManager.Inst.LoadBgm( GameManager.Inst.CurrentSong.audioPath );
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

    public void Select( Song _song )
    {
        IsPlaying = IsMusicStart = false;
        Playback = waitTime; 
        PlaybackChanged = 0;

        using ( FileParser parser = new FileParser() )
        {
            Chart chart;
            parser.TryParse( _song.filePath, out chart );
            CurrentChart = chart;
        }
    }

    public static float GetChangedTime( float _time ) // BPM 변화에 따른 시간 계산
    {
        double newTime = _time;
        double prevBpm = 0d;
        for ( int i = 0; i < CurrentChart.timings.Count; i++ )
        {
            double time = CurrentChart.timings[i].time;
            double bpm  = CurrentChart.timings[i].bpm;

            if ( time > _time ) break;
            newTime += ( bpm - prevBpm ) * ( _time - time );
            prevBpm = bpm;
        }
        return ( float )newTime;
    }
}
