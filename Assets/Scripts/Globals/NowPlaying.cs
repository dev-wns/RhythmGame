// #define ASYNC_PARSE

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class NowPlaying : Singleton<NowPlaying>
{
    #region Variables
    public static Scene CurrentScene;
    public ReadOnlyCollection<Song> Songs { get; private set; } = new ReadOnlyCollection<Song>( new List<Song>() );
    public static Song  CurrentSong    { get; private set; }
    public static Chart CurrentChart   { get; private set; }
    public int CurrentSongIndex { get; private set; }

    private double medianBPM;
    private int    timingIndex;

    #region Time
    private Timer timer = new Timer();
    private double startTime, saveTime, totalTime;
    public  static readonly double StartWaitTime = -3d;
    private static readonly double PauseWaitTime = -1.5d;
    public static double Playback        { get; private set; }
    public static double PlaybackInBPM   { get; private set; }
    private       double playbackInBPMChache;
    #endregion


    #region Event
    public event Action                    OnResult;
    public event Action                    OnStart;
    public event Action<bool/* isPause */> OnPause;
    #endregion

    public bool IsStart        { get; private set; }
    public bool IsParseSong    { get; private set; }
    public bool IsLoadBGA      { get; set; }
    public bool IsLoadKeySound { get; set; }
    #endregion

    #region Unity Callback
    protected override async void Awake()
    {
        base.Awake();

#if ASYNC_PARSE
        Task parseSongsAsyncTask = Task.Run( ParseSongs );
        await parseSongsAsyncTask;
#else
        ParseSong();
        await Task.CompletedTask;
        #endif        
    }
    
    private void Update()
    {
        if ( !IsStart ) return;

        Playback += Time.deltaTime;
        // saveTime + ( timer.CurrentTime - startTime );
        //PlaybackInBPM = GetChangedTime( Playback );

        var timings = CurrentChart.timings;
        for ( int i = timingIndex; i < timings.Count; i++ )
        {
            double curTime = timings[i].time;
            if ( Playback < curTime )
                 break;

            double curBPM  = timings[i].bpm;
            if ( i + 1 < timings.Count )
            {
                double nextTime = timings[i + 1].time;
                if ( nextTime < Playback )
                {
                    playbackInBPMChache += ( curBPM / medianBPM ) * ( nextTime - curTime );
                    timingIndex += 1;
                    continue;
                }
            }

            PlaybackInBPM = playbackInBPMChache + ( ( curBPM / medianBPM ) * ( Playback - curTime ) );
        }

        if ( Playback >= totalTime + 3d )
        {
            Stop();
            OnResult?.Invoke();
            CurrentScene?.LoadScene( SceneType.Result );
        }
    }
    #endregion
    #region Parsing
    private void ConvertSong()
    {
        using ( FileConverter converter = new FileConverter() )
        {
            converter.ReLoad();
        }
    }

    private void ParseSong()
    {
        ConvertSong();
        // StreamingAsset\\Songs 안의 모든 파일 순회하며 파싱
        using ( FileParser parser = new FileParser() )
        {
            ReadOnlyCollection<Song> songs;
            parser.ParseFileInDirectories( out songs );
            Songs = songs;
        }
        IsParseSong = true;
        UpdateSong( 0 );
    }

    public void ParseChart()
    {
        Stop();
        totalTime = CurrentSong.totalTime * .001d / GameSetting.CurrentPitch;
        using ( FileParser parser = new FileParser() )
        {
            Chart chart;
            if ( !parser.TryParse( CurrentSong.filePath, out chart ) )
            {
                CurrentScene.LoadScene( SceneType.FreeStyle );
            }
            else 
            {
                CurrentChart = chart;
                medianBPM = CurrentSong.medianBpm * GameSetting.CurrentPitch;

                //StartWaitTime = chart.uninheritedTimings[0].time;
                //Playback = StartWaitTime;
            }
        }
    }

    #endregion
    #region Sound Process
    public void Play()
    {
        startTime = timer.CurrentTime;
        saveTime = StartWaitTime;
        IsStart = true;

        OnStart?.Invoke();
        SoundManager.Inst.SetPaused( false, ChannelType.BGM );
    }

    public void Stop()
    {
        StopAllCoroutines();
        Playback = StartWaitTime;
        saveTime = 0d;
        PlaybackInBPM       = 0d;
        playbackInBPMChache = 0d;
        timingIndex = 0;

        IsStart        = false;
        IsLoadBGA      = false;
        IsLoadKeySound = false;
    }

    /// <returns>   FALSE : Playback is higher than the last note time. </returns>
    public bool Pause( bool _isPause )
    {
        if ( Playback >= totalTime )
        {
            OnResult?.Invoke();
            return false;
        }

        if ( _isPause )
        {
            IsStart = false;

            SoundManager.Inst.SetPaused( true, ChannelType.BGM );
            OnPause?.Invoke( true );
            saveTime = Playback >= 0d ? PauseWaitTime + Playback : 0d;
        }
        else
        {
            StartCoroutine( Continue() );
        }

        return true;
    }

    private IEnumerator Continue()
    {
        CurrentScene.IsInputLock = true;
        while ( Playback >= saveTime )
        {
            Playback -= Time.deltaTime * 2d;

            Timing timing = CurrentChart.timings[timingIndex];
            PlaybackInBPM = playbackInBPMChache + ( ( timing.bpm / medianBPM ) * ( Playback - timing.time ) );
            //PlaybackInBPM = GetChangedTime( Playback );

            yield return null;
        }

        startTime = timer.CurrentTime;
        IsStart   = true;

        yield return new WaitUntil( () => Playback >= saveTime - PauseWaitTime );
        SoundManager.Inst.SetPaused( false, ChannelType.BGM );
        OnPause?.Invoke( false );

        yield return YieldCache.WaitForSeconds( 2f );
        CurrentScene.IsInputLock = false;
    }
    #endregion
    #region Etc.
    public void UpdateSong( int _index )
    {
        if ( _index >= Songs.Count )
            throw new Exception( "out of range" );

        CurrentSongIndex = _index;
        CurrentSong      = Songs[_index];
    }
    #endregion
    /// <returns> Time including BPM. </returns>
    public double GetIncludeBPMTime( double _time )
    {
        var timings = CurrentChart.timings;
        double newTime = 0d;
        double prevBpm = 0d;
        for ( int i = 0; i < timings.Count; i++ )
        {
            double time = timings[i].time;
            double bpm  = timings[i].bpm;

            if ( time > _time ) break;
            bpm = bpm / medianBPM;
            newTime += ( bpm - prevBpm ) * ( _time - time );
            prevBpm = bpm;
        }
        return newTime;
    }
}