using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;

public class NowPlaying : SingletonUnity<NowPlaying>
{
    public ReadOnlyCollection<Song> Songs { get; private set; }
    public List<FMOD.Sound> KeySounds { get; private set; }

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
    public Song GetSongIndexAt( int _index )
    {
        if ( _index > Songs.Count )
            throw new Exception( "out of range" );

        return Songs[_index];
    }

    public static double Playback;        // 노래 재생 시간
    public static double PlaybackChanged; // BPM 변화에 따른 노래 재생 시간

    private readonly double waitTime = -2d;
    private double startTime;
    private double savedTime;
    private double totalTime;

    public event Action OnResult;
    public event Action OnStart; 
    public delegate void DelPause( bool _isPause );
    public event DelPause OnPause;
    private Coroutine timeCoroutine;

    public bool IsLoadKeySounds { get; set; }
    public bool IsLoadBackground { get; set; }

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

    private IEnumerator TimeUpdate()
    {
        while ( true )
        {
            Playback = savedTime + ( System.DateTime.Now.TimeOfDay.TotalSeconds - startTime );
            PlaybackChanged = GetChangedTime( Playback );

            if ( Playback >= totalTime + 3d )
            {
                Stop();
                OnResult?.Invoke();
                SceneChanger.Inst.LoadScene( SceneType.Result );
            }
            yield return null;
        }
    }

    public void Stop()
    {
        StopAllCoroutines();
        timeCoroutine = null;
        Playback = waitTime;
        savedTime = 0d;
        PlaybackChanged = 0d;

        IsLoadKeySounds  = true;
        IsLoadBackground = true;
    }

    public void Initialize()
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
            if ( timeCoroutine != null )
            {
                StopCoroutine( timeCoroutine );
                timeCoroutine = null;
            }

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
        SceneChanger.CurrentScene.InputLock( true );
        while ( Playback >= savedTime )
        {
            Playback -= Time.deltaTime * 3f;
            PlaybackChanged = GetChangedTime( Playback );
            yield return null;
        }

        startTime = System.DateTime.Now.TimeOfDay.TotalSeconds;
        timeCoroutine = StartCoroutine( TimeUpdate() );

        yield return new WaitUntil( () => Playback >= savedTime - waitTime );
        SoundManager.Inst.SetPaused( false, ChannelType.BGM );
        SoundManager.Inst.SetPaused( false, ChannelType.KeySound );
        OnPause?.Invoke( false );

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
        savedTime = waitTime;
        timeCoroutine = StartCoroutine( TimeUpdate() );

        yield return new WaitUntil( () => Playback >= GameSetting.SoundOffset * .001d ); ;

        OnStart?.Invoke();

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
