using System;
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

    public bool IsPlaying { get; private set; }
    private readonly double waitTime = -2d;
    private double startTime;
    private double savedTime;

    private double totalTime;

    public event Action OnResult;

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

        if ( Playback >= totalTime + 3d )
        {
            Stop();
            OnResult?.Invoke();
            SceneChanger.Inst.LoadScene( SceneType.Result );
        }
    }

    public void Stop()
    {
        StopAllCoroutines();
        IsPlaying = false;
        Playback = waitTime;
        savedTime = 0d;
        PlaybackChanged = 0d;
    }

    public void Initialize()
    {
        Stop();

        totalTime = curSong.totalTime * .001d;
        using ( FileParser parser = new FileParser() )
        {
            parser.TryParse( curSong.filePath, out curChart );
        }

        SoundManager.Inst.KeyRelease();
        string dir = System.IO.Path.GetDirectoryName( curSong.filePath );
        for ( int i = 0; i < curChart.keySoundNames.Count; i++ )
        {
            SoundManager.Inst.LoadKeySound( System.IO.Path.Combine( dir, curChart.keySoundNames[i] ) );
        }
    }

    public void Play()
    {
        SoundManager.Inst.ReleaseTemps();
        StartCoroutine( MusicStart() );
    }

    /// <returns>False : Playback is higher than the Last Note Time.</returns>
    public bool Pause( bool _isPause )
    {
        if ( Playback >= totalTime )
        {
            OnResult?.Invoke();
            return false;
        }


        if ( _isPause )
        {
            IsPlaying = false;
            SoundManager.Inst.SetPaused( true, ChannelType.KeySound );
            SoundManager.Inst.SetPaused( true, ChannelType.BGM );
            savedTime = Playback >= 0d ? waitTime + Playback : 0d;
        }
        else
        {
            StartCoroutine( PauseStart() );
        }

        return true;
    }

    private IEnumerator PauseStart()
    {
        SceneChanger.CurrentScene.InputLock( true );
        while ( Playback >= savedTime )
        {
            Playback -= Time.deltaTime * 3f;
            PlaybackChanged = GetChangedTime( Playback );
            yield return null;
        }

        startTime = System.DateTime.Now.TimeOfDay.TotalSeconds;
        IsPlaying = true;

        yield return new WaitUntil( () => Playback >= savedTime - waitTime );
        SoundManager.Inst.SetPaused( false, ChannelType.BGM );
        SoundManager.Inst.SetPaused( false, ChannelType.KeySound );

        yield return YieldCache.WaitForSeconds( 3f );
        SceneChanger.CurrentScene.InputLock( false );
    }

    private IEnumerator MusicStart()
    {
        if ( !curSong.isOnlyKeySound )
        {
            SoundManager.Inst.LoadBgm( CurrentSong.audioPath, false, false, false );
            SoundManager.Inst.Play( true );
            SoundManager.Inst.Position = 0;
        }

        startTime = System.DateTime.Now.TimeOfDay.TotalSeconds;
        IsPlaying = true;
        savedTime = waitTime;

        yield return new WaitUntil( () => Playback >= GameSetting.SoundOffset * .001d ); ;

        SoundManager.Inst.SetPaused( false, ChannelType.BGM );
        SoundManager.Inst.SetPaused( false, ChannelType.KeySound );
    }

    /// <returns> Time including BPM. </returns>
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
