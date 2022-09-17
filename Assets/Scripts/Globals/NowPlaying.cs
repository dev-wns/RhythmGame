// #define ASYNC_PARSE

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class NowPlaying : SingletonUnity<NowPlaying>
{
    public static Scene CurrentScene;
    public ReadOnlyCollection<Song> Songs { get; private set; } = new ReadOnlyCollection<Song>( new List<Song>() );
    public Song CurrentSong     { get; private set; }
    public Chart CurrentChart   { get; private set; }
    public int CurrentSongIndex { get; private set; }

    private Timer timer = new Timer();
    public static double  Playback        { get; private set; }
    public  static double PlaybackChanged { get; private set; }

    private readonly double waitTime = -1.25d;
    private double startTime, saveTime, totalTime;

    public event Action       OnResult;
    public event Action       OnStart;
    public event Action<bool> OnPause;

    public bool IsStart        { get; private set; }
    public bool IsParseSong    { get; private set; }
    public bool IsLoadBGA      { get; set; }
    public bool IsLoadKeySound { get; set; }

    #region Unity Callback
    protected override async void Awake()
    {
        base.Awake();
        #if ASYNC_PARSE
        Task parseSongsAsyncTask = Task.Run( ParseSongs );
        await parseSongsAsyncTask;
        #else
        ParseSongs();
        await Task.CompletedTask;
        #endif        
    }
    
    private void Update()
    {
        if ( !IsStart ) return;

        Playback        = saveTime + ( timer.CurrentTime - startTime );
        PlaybackChanged = GetChangedTime( Playback );

        if ( Playback >= totalTime + 3d )
        {
            Stop();
            OnResult?.Invoke();
            CurrentScene?.LoadScene( SceneType.Result );
        }
    }
    #endregion

    #region Parsing
    private void ParseSongs()
    {
        //using ( FileConverter converter = new FileConverter() )
        //{
        //    converter.ReLoad();
        //}

        using ( FileParser parser = new FileParser() )
        {
            ReadOnlyCollection<Song> songs;
            parser.ParseFilesInDirectories( out songs );
            Songs = songs;
        }

        IsParseSong = true;
        UpdateSong( 0 );

        Debug.Log( "Parsing Completed." );
    }

    public void ParseChart()
    {
        Stop();

        totalTime = CurrentSong.totalTime * .001d / GameSetting.CurrentPitch;
        using ( FileParser parser = new FileParser() )
        {
            Chart chart;
            parser.TryParse( CurrentSong.filePath, out chart );
            CurrentChart = chart;
        }
    }
    #endregion

    #region Sound Process
    public IEnumerator Play()
    {
        startTime = timer.CurrentTime;
        saveTime = waitTime;
        IsStart = true;

        yield return new WaitUntil( () => Playback >= GameSetting.SoundOffset * .001d );

        OnStart?.Invoke();

        SoundManager.Inst.SetPaused( false, ChannelType.KeySound );
    }

    public void Stop()
    {
        StopAllCoroutines();
        Playback = waitTime;
        saveTime = 0d;
        PlaybackChanged = 0d;

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

            SoundManager.Inst.SetPaused( true, ChannelType.KeySound );
            OnPause?.Invoke( true );
            saveTime = Playback >= 0d ? waitTime + Playback : 0d;
        }
        else
        {
            StartCoroutine( Continue() );
        }

        return true;
    }

    private IEnumerator Continue()
    {
        CurrentScene.InputLock( true );
        while ( Playback >= saveTime )
        {
            Playback -= Time.deltaTime * 2d;
            PlaybackChanged = GetChangedTime( Playback );

            yield return null;
        }

        startTime = timer.CurrentTime;
        IsStart   = true;

        yield return new WaitUntil( () => Playback >= saveTime - waitTime );
        SoundManager.Inst.SetPaused( false, ChannelType.KeySound );
        OnPause?.Invoke( false );

        yield return YieldCache.WaitForSeconds( 2f );
        CurrentScene.InputLock( false );
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
    public double GetChangedTime( double _time )
    {
        var timings = CurrentChart.timings;
        double newTime = 0d;
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
