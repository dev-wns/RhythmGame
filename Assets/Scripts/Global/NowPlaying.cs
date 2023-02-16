// #define ASYNC_PARSE

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

public struct HitData
{
    public HitResult result;
    public double diff;
    public double time;

    public HitData( HitResult _res, double _diff, double _time )
    {
        result = _res;
        diff = _diff;
        time = _time;
    }
}
public struct ResultData
{
    // counts
    public int maximum;
    public int perfect;
    public int great;
    public int good;
    public int bad;
    public int miss;
    public int fast;
    public int slow;
    public int accuracy;
    public int combo;
    public int score;

    public int random;
    public int pitch;

    public ResultData( int _random, int _pitch )
    {
        random  = _random;
        pitch   = _pitch;
        maximum = perfect = great    = good  = bad   = miss = 0;
        fast    = slow    = accuracy = combo = score        = 0;
    }
}

public struct RecordData
{
    public int score;
    public int accuracy;
    public int random;
    public float pitch;
    public string date;
}

public class NowPlaying : Singleton<NowPlaying>
{
    #region Variables
    public static Scene CurrentScene;
    private ReadOnlyCollection<Song> Songs;//{ get; private set; }
    public List<Song> ChangedSongs;

    public static Song  CurrentSong    { get; private set; }
    public static Chart CurrentChart   { get; private set; }
    public static string Directory     { get; private set; }
    public int CurrentSongIndex { get; private set; }

    private double medianBPM;
    private int timingIndex;

    #region Time
    private Timer timer = new Timer();
    private double startTime, saveTime;
    public  static readonly double StartWaitTime = -3d;
    private static readonly double PauseWaitTime = -1.5d;
    public  static float GameTime         { get; private set; }
    public  static double Playback        { get; private set; }
    public  static double PlaybackInBPM   { get; private set; }
    private static double PlaybackInBPMChache;
    private static double Sync;
    #endregion
    public List<HitData> HitDatas { get; private set; } = new List<HitData>();
    public  ResultData CurrentResult => currentResult;
    private ResultData currentResult = new ResultData();

    public List<RecordData> RecordDatas { get; private set; } = new List<RecordData>( 5 );

    public event Action<string> OnParse;

    public bool IsStart        { get; private set; }
    public bool IsParseSong    { get; private set; }
    public bool IsLoadBGA      { get; set; }
    public bool IsLoadKeySound { get; set; }
    #endregion

    public void Search( string _keyword )
    {
        string keyword = _keyword.TrimStart();
        if ( keyword == string.Empty )
        {
            ChangedSongs = Songs.ToList();
            UpdateSong( 0 );
            return;
        }

        List<Song> newList = new List<Song>();
        for ( int i = 0; i < Songs.Count; i++ )
        {
            if ( Songs[i].title.Replace(   " ", string.Empty ).Contains( keyword, StringComparison.OrdinalIgnoreCase ) || 
                 Songs[i].version.Replace( " ", string.Empty ).Contains( keyword, StringComparison.OrdinalIgnoreCase ) || 
                 Songs[i].artist.Replace(  " ", string.Empty ).Contains( keyword, StringComparison.OrdinalIgnoreCase ) )
            {
                newList.Add( Songs[i] );
            }
        }

        ChangedSongs = newList.Count == 0 ? Songs.ToList() : newList;
        UpdateSong( 0 );
    }

    #region Unity Callback
    protected override async void Awake()
    {
        base.Awake();

#if ASYNC_PARSE
        Task parseSongsAsyncTask = Task.Run( ParseSongs );
        await parseSongsAsyncTask;
#else
        Load();
        await Task.CompletedTask;
        #endif        
    }

    public void UpdateRecord()
    {
        RecordDatas.Clear();
        string path = Path.Combine( Directory, GameSetting.RecordFileName );
        if ( !File.Exists( path ) )
             return;

        using ( StreamReader stream = new StreamReader( path ) )
        {
            RecordDatas.AddRange( JsonConvert.DeserializeObject<RecordData[]>( stream.ReadToEnd() ) );
        }
    }

    public RecordData MakeNewRecord()
    {
        var newRecord = new RecordData()
        {
            score    = currentResult.score,
            accuracy = currentResult.accuracy,
            random   = ( int )GameSetting.CurrentRandom,
            pitch    = GameSetting.CurrentPitch,
            date     = DateTime.Now.ToString( "yyyy. MM. dd @ hh:mm:ss tt" )
        };
        RecordDatas.Add( newRecord );
        RecordDatas.Sort( delegate ( RecordData A, RecordData B )
        {
            if ( A.score < B.score )      return 1;
            else if ( A.score > B.score ) return -1;
            else                          return 0;
        } );
        if ( RecordDatas.Count > 10 )
             RecordDatas.Remove( RecordDatas.Last() );

        using ( FileStream stream = new FileStream( Path.Combine( Directory, GameSetting.RecordFileName ), FileMode.OpenOrCreate ) )
        {
            using ( StreamWriter writer = new StreamWriter( stream ) )
            {
                writer.Write( JsonConvert.SerializeObject( RecordDatas.ToArray(), Formatting.Indented ) );
            }
        }

        return newRecord;
    }

    private void Update()
    {
        GameTime += Time.deltaTime;
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
            string[] files = Global.IO.GetFilesInSubDirectories( GameSetting.SoundDirectoryPath, "*.osu" );
            for ( int i = 0; i < files.Length; i++ )
            {
                converter.Load( files[i] );
            }
        }
    }

    public void Load()
    {
        Timer perfomenceTimer = new Timer( true );
        ConvertSong();
        // StreamingAsset\\Songs 안의 모든 파일 순회하며 파싱
        using ( FileParser parser = new FileParser() )
        {
            List<Song> newSongList = new List<Song>();
            string[] files = Global.IO.GetFilesInSubDirectories( GameSetting.SoundDirectoryPath, "*.wns" );
            for( int i = 0; i < files.Length; i++ )
            {
                OnParse?.Invoke( System.IO.Path.GetFileName( files[i] ) );
                if ( parser.TryParse( files[i], out Song newSong ) )
                {
                    newSongList.Add( newSong );
                }
            }
            newSongList.Sort( delegate ( Song _a, Song _b ) { return _a.title.CompareTo( _b.title ); } );
            Songs = new ReadOnlyCollection<Song>( newSongList );
            ChangedSongs = Songs.ToList();

            Debug.Log( $"Parsing completed ( {perfomenceTimer.End} ms )  Total : {Songs.Count}" );
        }
        IsParseSong = true;

        if ( Songs.Count > 0 )
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
                CurrentChart = chart;
                medianBPM    = CurrentSong.medianBpm * GameSetting.CurrentPitch;
                Debug.Log( $"Parsing completed ( {perfomenceTimer.End} ms )  CurrentChart : {CurrentSong.title}" );
            }
        }
    }

    #endregion
    #region ResultData

    public void ResetData()
    {
        currentResult = new ResultData( ( int )GameSetting.CurrentRandom, Mathf.RoundToInt( GameSetting.CurrentPitch * 100f ) );
        HitDatas.Clear();
    }

    public void AddHitData( HitResult _key, double _diff )
    {
        HitDatas.Add( new HitData( _key, _diff, Playback ) );
    }

    public void SetResultCount( HitResult _key, int _count )
    {
        switch ( _key )
        {
            case HitResult.Maximum:  currentResult.maximum  = _count; break;
            case HitResult.Perfect:  currentResult.perfect  = _count; break;
            case HitResult.Great:    currentResult.great    = _count; break;
            case HitResult.Good:     currentResult.good     = _count; break;
            case HitResult.Bad:      currentResult.bad      = _count; break;
            case HitResult.Miss:     currentResult.miss     = _count; break;
            case HitResult.Fast:     currentResult.fast     = _count; break;
            case HitResult.Slow:     currentResult.slow     = _count; break;
            case HitResult.Accuracy: currentResult.accuracy = _count; break;
            case HitResult.Combo:    currentResult.combo    = _count; break;
            case HitResult.Score:    currentResult.score    = _count; break;
        }
    }
    #endregion
    #region Sound Process
    public void SoundSynchronized( double _time )
    {
        Sync = _time - Playback;
        Debug.Log( $"Synchronized : {( int )( Sync * 1000d )} ms" );
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
        if ( _index >= ChangedSongs.Count )
            throw new Exception( "out of range" );

        CurrentSongIndex = _index;
        CurrentSong      = ChangedSongs[_index];
        Directory        = Path.GetDirectoryName( ChangedSongs[_index].filePath );
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