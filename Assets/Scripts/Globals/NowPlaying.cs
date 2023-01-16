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
    private int timingIndex;

    #region Time
    private Timer timer = new Timer();
    private double startTime, saveTime;
    public  static readonly double StartWaitTime = -3d;
    private static readonly double PauseWaitTime = -1.5d;
    public  static double Playback        { get; private set; }
    public  static double PlaybackInBPM   { get; private set; }
    private static double PlaybackInBPMChache;
    private static double Sync;
    #endregion


    //#region Event
    //public event Action<bool/* isPause */> OnPause;
    //#endregion

    public bool IsStart        { get; private set; }
    public bool IsParseSong    { get; private set; }
    public bool IsLoadBGA      { get; set; }
    public bool IsLoadKeySound { get; set; }
    #endregion

    #region Input
    //public static bool IsGameInputLock { get; set; }
    #endregion

    #region Unity Callback
    protected override async void Awake()
    {
        base.Awake();

        Stop();
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

        Playback = saveTime + ( timer.CurrentTime - startTime ) + Sync;
        UpdatePlayback();
    }

    private void UpdatePlayback()
    {
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
                    PlaybackInBPMChache += ( curBPM / medianBPM ) * ( nextTime - curTime );
                    timingIndex += 1;
                    continue;
                }
            }

            PlaybackInBPM = PlaybackInBPMChache + ( ( curBPM / medianBPM ) * ( Playback - curTime ) );
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
        Timer perfomenceTimer = new Timer( true );
        ConvertSong();
        // StreamingAsset\\Songs 안의 모든 파일 순회하며 파싱
        using ( FileParser parser = new FileParser() )
        {
            ReadOnlyCollection<Song> songs;
            parser.ParseFileInDirectories( out songs );
            Songs = songs;
            Debug.Log( $"Parsing completed ( {perfomenceTimer.End} ms )  TotalSongs : {Songs.Count}" );
        }
        IsParseSong = true;
        UpdateSong( 0 );
    }

    public void ParseChart()
    {
        Timer perfomenceTimer = new Timer( true );
        Stop();
        using ( FileParser parser = new FileParser() )
        {
            Chart chart;
            if ( !parser.TryParse( CurrentSong.filePath, out chart ) )
            {
                CurrentScene.LoadScene( SceneType.FreeStyle );
                Debug.LogWarning( $"Parsing failed  Current Chart : {CurrentSong.title}" );
            }
            else 
            {
                CurrentChart     = chart;
                medianBPM        = CurrentSong.medianBpm * GameSetting.CurrentPitch;
                Debug.Log( $"Parsing completed ( {perfomenceTimer.End} ms )  CurrentChart : {CurrentSong.title}" );
            }
        }
    }

    #endregion
    #region Sound Process
    public void SoundSynchronized( double _time )
    {
        Sync = _time - Playback;
        Debug.Log( $"Synchronized : {Sync} s" );
    }

    public void Play()
    {
        SoundManager.Inst.SetPaused( false, ChannelType.BGM );

        startTime       = timer.CurrentTime;
        saveTime        = StartWaitTime;
        IsStart         = true;
        Debug.Log( $"Playback start." );
    }

    public void Stop()
    {
        StopAllCoroutines();
        Playback = StartWaitTime;
        saveTime = StartWaitTime;
        PlaybackInBPM       = 0d;
        PlaybackInBPMChache = 0d;
        timingIndex         = 0;

        IsStart         = false;
        IsLoadBGA       = false;
        IsLoadKeySound  = false;
    }

    public IEnumerator GameOver()
    {
        IsStart         = false;
        float slowTimeOffset = 1f / 3f;
        float speed = 1f;
        float pitchOffset = GameSetting.CurrentPitch * .3f;
        while ( true )
        {
            Playback += speed * Time.deltaTime;
            UpdatePlayback();

            CurrentScene.UpdatePitch( GameSetting.CurrentPitch - ( ( 1f - speed ) * pitchOffset ) );
            speed -= slowTimeOffset * Time.deltaTime;
            if ( speed < 0f )
                 break;

            yield return null;
        }
    }

    /// <returns>   FALSE : Playback is higher than the last note time. </returns>
    public void Pause( bool _isPause )
    {
        if ( _isPause )
        {
            IsStart  = false;
            saveTime = Playback + PauseWaitTime;
            SoundManager.Inst.SetPaused( true, ChannelType.BGM );
        }
        else
        {
            StartCoroutine( Continue() );
        }
    }

    private IEnumerator Continue()
    {
        Timing timing = CurrentChart.timings[timingIndex];
        while ( Playback > saveTime )
        {
            Playback     -= Time.deltaTime * 2d;
            PlaybackInBPM = PlaybackInBPMChache + ( ( timing.bpm / medianBPM ) * ( Playback - timing.time ) );

            yield return null;
        }

        startTime = timer.CurrentTime;
        IsStart   = true;

        yield return new WaitUntil( () => Playback > saveTime - PauseWaitTime );
        SoundSynchronized( saveTime - PauseWaitTime );
        SoundManager.Inst.SetPaused( false, ChannelType.BGM );

        yield return YieldCache.WaitForSeconds( 3f );
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