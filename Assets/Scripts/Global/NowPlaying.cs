#define START_FREESTYLE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class NowPlaying : Singleton<NowPlaying>
{
    #region Variables
    public static Scene CurrentScene;
    private ReadOnlyCollection<Song> OriginSongs;
    public List<Song> Songs = new List<Song>();

    public static Song CurrentSong { get; private set; }
    public static Chart CurrentChart { get; private set; }
    public static string Directory { get; private set; }
    public static int OriginKeyCount => CurrentSong.keyCount;
    public static int KeyCount => GameSetting.CurrentGameMode.HasFlag( GameMode.KeyConversion ) && OriginKeyCount == 7 ? 6 : OriginKeyCount;
    public int CurrentSongIndex { get; private set; }
    public int SearchCount { get; private set; }
    private double mainBPM;
    private int timingIndex;

    #region Time
    public double SaveTime { get; private set; }
    private double startTime;
    public static double WaitTime { get; private set; }
    public static readonly double StartWaitTime = -3d;
    public static readonly double PauseWaitTime = -2d;
    public static float GameTime { get; private set; }
    public static double Playback { get; private set; }
    public  static double Distance { get; private set; }
    private static double DistanceCache;
    #endregion

    public int TotalFileCount { get; private set; }
    public static event Action<Song> OnParsing;
    public static event Action       OnParsingEnd;

    public static bool IsStart { get; private set; }
    public static bool IsParsing { get; private set; }
    public static bool IsLoadBGA { get; set; }
    public static bool IsLoadKeySound { get; set; }
    #endregion

    private CancellationTokenSource cancelSource = new CancellationTokenSource();

    #region Unity Callback
    protected override void Awake()
    {
        base.Awake();
        Load();
    }

    private async void Start()
    {
        await Task.Run( () => UpdateTime( cancelSource.Token ) );
    }

    private void Update()
    {
        GameTime += Time.deltaTime;
        if ( !IsStart ) return;

        //Debug.Log( Playback );
        //Playback = SaveTime + ( DateTime.Now.TimeOfDay.TotalMilliseconds - startTime );
    }

    private void OnApplicationQuit()
    {
        cancelSource?.Cancel();
    }
    #endregion

    private async void UpdateTime( CancellationToken _token )
    {
        while ( !_token.IsCancellationRequested )
        {
            if ( IsStart )
            {
                Playback = SaveTime + ( DateTime.Now.TimeOfDay.TotalMilliseconds - startTime ) * .001d;

                var timings = CurrentChart.timings;
                for ( int i = timingIndex; i < timings.Count; i++ )
                {
                    double time = timings[i].time;
                    double bpm  = timings[i].bpm / mainBPM;

                    if ( Playback < time )
                         break;

                    if ( i + 1 < timings.Count && timings[i + 1].time < Playback )
                    {
                        timingIndex++;
                        DistanceCache += bpm * ( timings[i + 1].time - time );
                        Distance = DistanceCache;
                        break;
                    }

                    Distance = DistanceCache + ( bpm * ( Playback - time ) );
                }
            }

            await Task.Delay( 1 );
        }
    }

    #region Parsing
    public void Initalize()
    {
        Clear();
        WaitTime = StartWaitTime;
        mainBPM  = CurrentSong.mainBPM * GameSetting.CurrentPitch;

        // 채보 파싱
        using ( FileParser parser = new FileParser() )
        {
            if ( !parser.TryParse( CurrentSong.filePath, out Chart chart ) )
            {
                CurrentScene.LoadScene( SceneType.FreeStyle );
                Debug.LogWarning( $"Parsing failed  Current Chart : {CurrentSong.title}" );
            }
            else
                CurrentChart = chart;
        }
    }

    private void ConvertSong()
    {
        string[] files = Global.FILE.GetFilesInSubDirectories( GameSetting.SoundDirectoryPath, "*.osu" );
        for ( int i = 0; i < files.Length; i++ )
        {
            using ( FileConverter converter = new FileConverter() )
            {
                converter.Load( files[i] );
            }
        }
    }

    public void Load()
    {
        Timer timer = new Timer();
        IsParsing = true;
        ConvertSong();
        List<Song> newSongList = new List<Song>();

        // StreamingAsset\\Songs 안의 모든 파일 순회하며 파싱
        string[] files = Global.FILE.GetFilesInSubDirectories( GameSetting.SoundDirectoryPath, "*.wns" );
        TotalFileCount = files.Length;
        for ( int i = 0; i < TotalFileCount; i++ )
        {
            using ( FileParser parser = new FileParser() )
            {
                if ( parser.TryParse( files[i], out Song newSong ) )
                {
                    newSong.UID = newSongList.Count;
                    newSongList.Add( newSong );
                    OnParsing?.Invoke( newSong );
                }
            }
        }

        newSongList.Sort( ( _left, _right ) => _left.title.CompareTo( _right.title ) );
        for ( int i = 0; i < newSongList.Count; i++ )
        {
            var song = newSongList[i];
            song.UID = i;
            newSongList[i] = song;
        }

        Songs = newSongList.ToList();
        OriginSongs = new ReadOnlyCollection<Song>( Songs.ToList() );

        if ( Songs.Count > 0 )
            UpdateSong( 0 );

        OnParsingEnd?.Invoke();
        IsParsing = false;

        Debug.Log( $"Update Songs {timer.End} ms" );
    }
    #endregion

    #region Search
    public void Search( string _keyword )
    {
        if ( OriginSongs.Count == 0 )
            return;

        if ( _keyword.Replace( " ", string.Empty ) == string.Empty )
        {
            Songs = OriginSongs.ToList();
            SearchCount = OriginSongs.Count;
            UpdateSong( CurrentSong.UID );

            return;
        }

        Songs.Clear();
        bool isSV = _keyword.Replace( " ", string.Empty ).ToUpper().CompareTo( "SV" ) == 0;
        for ( int i = 0; i < OriginSongs.Count; i++ )
        {
            if ( isSV )
            {
                if ( OriginSongs[i].minBpm != OriginSongs[i].maxBpm )
                     Songs.Add( OriginSongs[i] );
            }
            else
            {
                if ( OriginSongs[i].title.Replace(   " ", string.Empty ).Contains( _keyword, StringComparison.OrdinalIgnoreCase ) ||
                     OriginSongs[i].version.Replace( " ", string.Empty ).Contains( _keyword, StringComparison.OrdinalIgnoreCase ) ||
                     OriginSongs[i].artist.Replace(  " ", string.Empty ).Contains( _keyword, StringComparison.OrdinalIgnoreCase ) ||
                     OriginSongs[i].source.Replace(  " ", string.Empty ).Contains( _keyword, StringComparison.OrdinalIgnoreCase ) )
                     Songs.Add( OriginSongs[i] );
            }
        }

        SearchCount = Songs.Count;
        if ( SearchCount != 0 )
             UpdateSong( 0 );
    }

    public void Search( Song _song )
    {
        for ( int i = 0; i < OriginSongs.Count; i++ )
        {
            if ( OriginSongs[i].title.Contains(   _song.title,   StringComparison.OrdinalIgnoreCase ) &&
                 OriginSongs[i].version.Contains( _song.version, StringComparison.OrdinalIgnoreCase ) )
            { 
                UpdateSong( i );
                return;
            }
        }

        UpdateSong( 0 );
    }

    #endregion
   
    #region Sound Process
    public void Play()
    {
        AudioManager.Inst.SetPaused( false, ChannelType.BGM );

        startTime     = DateTime.Now.TimeOfDay.TotalMilliseconds;
        SaveTime      = WaitTime;
        IsStart       = true;
    }

    public void Clear()
    {
        StopAllCoroutines();
        Playback       = WaitTime;
        SaveTime       = WaitTime;
        Distance       = 0d;
        DistanceCache  = 0d;
        timingIndex    = 0;
        IsStart        = false;
        IsLoadBGA      = false;
        IsLoadKeySound = false;
    }

    public IEnumerator GameOver()
    {
        IsStart = false;
        float slowTimeOffset = 1f / 3f;
        float speed = 1f;
        float pitchOffset = GameSetting.CurrentPitch * .3f;
        Timing timing = CurrentChart.timings[timingIndex];
        while ( true )
        {
            Playback += speed * Time.deltaTime;
            Distance = DistanceCache + ( ( timing.bpm / mainBPM ) * ( Playback - timing.time ) );

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
            IsStart = false;
            SaveTime = Playback + PauseWaitTime;
            AudioManager.Inst.SetPaused( true, ChannelType.BGM );
        }
        else
        {
            StartCoroutine( Continue() );
        }
    }

    private IEnumerator Continue()
    {
        Timing timing = CurrentChart.timings[timingIndex];
        while ( Playback > SaveTime )
        {
            yield return null;

            Playback -= Time.deltaTime * 1.2f;
            Distance = DistanceCache + ( ( timing.bpm / mainBPM ) * ( Playback - timing.time ) );
        }

        startTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
        IsStart = true;

        WaitUntil waitPlayback = new WaitUntil( () => Playback > SaveTime - PauseWaitTime );
        yield return waitPlayback;
        AudioManager.Inst.SetPaused( false, ChannelType.BGM );

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
        CurrentSong = Songs[_index];
        Directory = Path.GetDirectoryName( Songs[_index].filePath );
    }
    #endregion

    /// <returns> Time including BPM. </returns>
    //public double GetScaledPlayback( double _time )
    //{
    //    var timings = CurrentChart.timings;
    //    double newTime = 0d;
    //    double prevBpm = 0d;
    //    for ( int i = 0; i < timings.Count; i++ )
    //    {
    //        double time = timings[i].time;
    //        double bpm  = timings[i].bpm / mainBPM;
    //
    //        if ( time > _time ) break;
    //
    //        newTime += ( bpm - prevBpm ) * ( _time - time );
    //        prevBpm = bpm;
    //    }
    //    return newTime;
    //}

    /// <returns> _time까지 이동한 거리 </returns>
    public double GetDistance( double _time )
    {
        double result = 0d;
        var timings = CurrentChart.timings;
        for ( int i = 0; i < timings.Count; i++ )
        {
            double time = timings[i].time;
            double bpm  = timings[i].bpm / mainBPM;

            // 구간별 타이밍에 대한 거리 추가
            if ( i + 1 < timings.Count && timings[i + 1].time < _time )
            {
                result += bpm * ( timings[i + 1].time - time );
                continue;
            }

            // 마지막 타이밍에 대한 거리 추가
            result += bpm * ( _time - time );
            break;
        }
        return result;
    }
}