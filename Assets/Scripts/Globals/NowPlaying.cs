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
    public ReadOnlyCollection<Song> Songs { get; private set; }
    public List<FMOD.Sound> KeySounds { get; private set; }

    public Song CurrentSong => curSong;
    private Song curSong;

    public Chart CurrentChart => curChart;
    private Chart curChart;

    public int CurrentSongIndex
    {
        get => curSongIndex;
        set
        {
            if ( value >= Songs.Count )
                throw new Exception( "Out of Range. " );

            curSongIndex = value;
            curSong = Songs[value];
        }
    }
    private int curSongIndex;
    public Song GetSongIndexAt( int _index )
    {
        if ( _index > Songs.Count )
            throw new Exception( "out of range" );

        return Songs[_index];
    }

    public  static double Playback;        // 노래 재생 시간
    public  static double PlaybackChanged; // BPM 변화에 따른 노래 재생 시간
    public  static double PlaybackOffset;
    private double prevPlayback;

    private readonly double waitTime = -1.25d;
    private double startTime;
    private double savedTime;
    private double totalTime;

    public event Action       OnResult;
    public event Action       OnStart;
    public event Action<bool> OnPause;
    private bool isStart;

    public bool IsLoadKeySounds { get; set; }  = false;
    public bool IsLoadBackground { get; set; } = false;
    public bool IsParseSongs { get; private set; } = false;

    private async void Awake()
    {
#if ASYNC_PARSE
        Task parseSongsAsyncTask = Task.Run( ParseSongs );
        await parseSongsAsyncTask;
#else
        ParseSongs();
        await Task.CompletedTask;
#endif        
    }

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

        IsParseSongs = true;
        CurrentSongIndex = 0;

        Debug.Log( "Parsing Completed." );
    }

    private void Update()
    {
        if ( !isStart )
             return;

        prevPlayback = Playback;
        Playback = savedTime + ( Globals.Timer.CurrentTime - startTime );
        PlaybackOffset = Globals.Abs( prevPlayback - Playback );
        PlaybackChanged = GetChangedTime( Playback );
        
        if ( Playback >= totalTime + 3d )
        {
            Stop();
            OnResult?.Invoke();
            CurrentScene?.LoadScene( SceneType.Result );
        }
    }

    public void Stop()
    {
        StopAllCoroutines();
        Playback = waitTime;
        savedTime = 0d;
        PlaybackChanged = 0d;
        isStart = false;

        IsLoadKeySounds = false;
        IsLoadBackground = false;
    }

    public void ParseChart()
    {
        Stop();

        totalTime = curSong.totalTime * .001d;
        using ( FileParser parser = new FileParser() )
        {
            parser.TryParse( curSong.filePath, out curChart );
        }
    }

    public void Play()
    {
        StartCoroutine( MusicStart() );
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
            isStart = false;

            SoundManager.Inst.SetPaused( true, ChannelType.KeySound );
            SoundManager.Inst.SetPaused( true, ChannelType.BGM );
            OnPause?.Invoke( true );
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
        CurrentScene.InputLock( true );
        while ( Playback >= savedTime )
        {
            Playback -= Time.deltaTime;
            PlaybackChanged = GetChangedTime( Playback );
            yield return null;
        }

        startTime = Globals.Timer.CurrentTime;

        isStart = true;

        yield return new WaitUntil( () => Playback >= savedTime - waitTime );
        SoundManager.Inst.SetPaused( false, ChannelType.BGM );
        SoundManager.Inst.SetPaused( false, ChannelType.KeySound );
        OnPause?.Invoke( false );

        yield return YieldCache.WaitForSeconds( 2f );
        CurrentScene.InputLock( false );
    }

    private IEnumerator MusicStart()
    {
        if ( !curSong.isOnlyKeySound )
        {
            SoundManager.Inst.LoadBgm( curSong.audioPath, false, false, false );
            SoundManager.Inst.Play( true );
            SoundManager.Inst.Position = 0;
        }

        startTime = Globals.Timer.CurrentTime;
        savedTime = waitTime;
        isStart = true;

        yield return new WaitUntil( () => Playback >= GameSetting.SoundOffset * .001d ); ;

        OnStart?.Invoke();

        SoundManager.Inst.SetPaused( false, ChannelType.BGM );
        SoundManager.Inst.SetPaused( false, ChannelType.KeySound );
    }

    /// <returns> Time including BPM. </returns>
    public double GetChangedTime( double _time )
    {
        var timings = curChart.timings;
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
