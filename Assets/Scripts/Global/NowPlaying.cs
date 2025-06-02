#define START_FREESTYLE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

#pragma warning disable CS0162

public class NowPlaying : Singleton<NowPlaying>
{
    #region Variables
    public static Scene CurrentScene;
    public List<Song> Songs = new List<Song>();
    private ReadOnlyCollection<Song> OriginSongs;

    public static Song CurrentSong { get; private set; }
    public static Chart CurrentChart { get; private set; }
    public static ReadOnlyCollection<Timing> Timings { get; private set; }
    public static int OriginKeyCount => CurrentSong.keyCount;
    public static int KeyCount => GameSetting.CurrentGameMode.HasFlag( GameMode.KeyConversion ) && OriginKeyCount == 7 ? 6 : OriginKeyCount;
    public int CurrentSongIndex { get; private set; }
    public int SearchCount { get; private set; }
    public static int TotalNotes 
    {
        get 
        {
            bool hasKeyConversion = GameSetting.CurrentGameMode.HasFlag( GameMode.KeyConversion ) && CurrentSong.keyCount == 7;
            var note   = hasKeyConversion ? CurrentSong.noteCount   - CurrentSong.delNoteCount   : CurrentSong.noteCount;
            var slider = hasKeyConversion ? CurrentSong.sliderCount - CurrentSong.delSliderCount : CurrentSong.sliderCount;
            return note + ( slider * 2 );
        }
    }


    #region Time
    private double startTime;
    public double  SaveTime { get; private set; }
    public static double WaitTime { get; private set; }
    public static readonly double StartWaitTime = -3000d;
    public static readonly double PauseWaitTime = -2000d;
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

    private double mainBPM;
    private int numTiming;
    private CancellationTokenSource cancelSource = new CancellationTokenSource();

    #region Unity Callback
    protected override async void Awake()
    {
        base.Awake();
        Load();
        await Task.Run( () => UpdateTime( cancelSource.Token ) );
    }

    private void OnApplicationQuit()
    {
        cancelSource?.Cancel();
    }
    #endregion

    private async void UpdateTime( CancellationToken _token )
    {
        //Stopwatch stopwatch = Stopwatch.StartNew();
        //long interval  = 10000000L / 8000L;
        //long prevTick  = stopwatch.ElapsedTicks;
        //long startTick = stopwatch.ElapsedTicks;
        //int _8000Hz    = 0;
        //int noLimitHz  = 0;

        while ( !_token.IsCancellationRequested )
        {
            //noLimitHz++;
            //long curTick  = stopwatch.ElapsedTicks;
            //if ( ( curTick - prevTick ) >= interval )
            //{
            //    prevTick = curTick;
            //    _8000Hz++;
            //}

            //if ( ( curTick - startTick ) >= 10000000L )
            //{
            //    //Debug.Log( $"NOLIMIT : {noLimitHz}" );
            //    //Debug.Log( $"8000HZ  : {_8000Hz}" );
            //    _8000Hz   = 0;
            //    noLimitHz = 0;
            //    startTick = stopwatch.ElapsedTicks;
            //}

            if ( IsStart )
            {

                Playback = SaveTime + ( DateTime.Now.TimeOfDay.TotalMilliseconds - startTime );
                for ( int i = numTiming; i < Timings.Count; i++ )
                {
                    double time = Timings[i].time;
                    double bpm  = Timings[i].bpm / mainBPM;

                    // 지나간 타이밍에 대한 거리
                    if ( i + 1 < Timings.Count && Timings[i + 1].time < Playback )
                    {
                        numTiming += 1;
                        DistanceCache += bpm * ( Timings[i + 1].time - time );
                        break;
                    }

                    // 이전 타이밍까지의 거리( 캐싱 ) + 현재 타이밍에 대한 거리
                    Distance = DistanceCache + ( bpm * ( Playback - time ) );
                    break;
                }
            }

            await Task.Delay( 1 );
        }
    }

    #region Parsing
    public void Initalize()
    {
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
            {
                CurrentChart   = chart;
                Timings = chart.timings;
            }
        }
        Stop();
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
        startTime      = DateTime.Now.TimeOfDay.TotalMilliseconds;
        SaveTime       = WaitTime;
        IsStart        = true;
    }

    public void Clear()
    {
        StopAllCoroutines();
        Playback       = WaitTime;
        SaveTime       = WaitTime;
        Distance       = 0d;
        DistanceCache  = 0d;
        numTiming      = 0;
        IsStart        = false;
    }

    public void Stop()
    {
        Clear();
        IsLoadBGA      = false;
        IsLoadKeySound = false;
    }


    public IEnumerator GameOver()
    {
        IsStart = false;
        float slowTimeOffset = 1f / 3f;
        float speed = 1f;
        float pitchOffset = GameSetting.CurrentPitch * .3f;
        while ( true )
        {
            Playback += speed * Time.deltaTime;
            Distance = DistanceCache + ( ( Timings[numTiming].bpm / mainBPM ) * ( Playback - Timings[numTiming].time ) );

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
        while ( Playback > SaveTime )
        {
            yield return null;

            Playback -= Time.deltaTime * 1.2f;
            Distance = DistanceCache + ( ( Timings[numTiming].bpm / mainBPM ) * ( Playback - Timings[numTiming].time ) );
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
    }
    #endregion

    public double GetDistance( double _time )
    {
        double result = 0d;
        for ( int i = 0; i < Timings.Count; i++ )
        {
            double time = Timings[i].time;
            double bpm  = Timings[i].bpm / mainBPM;

            // 지나간 타이밍에 대한 거리
            if ( i + 1 < Timings.Count && Timings[i + 1].time < _time )
            {
                result += bpm * ( Timings[i + 1].time - time );
                continue;
            }

            // 현재 타이밍에 대한 거리
            result += bpm * ( _time - time );
            break;
        }
        return result;
    }
}