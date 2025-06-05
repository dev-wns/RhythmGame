#define START_FREESTYLE

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

#pragma warning disable CS0162

public enum SoundType { BGM, KeySound, }


public class NowPlaying : Singleton<NowPlaying>
{
    public static Scene CurrentScene;

    [Header( "Song" )]
    public ReadOnlyCollection<Song> Songs { get; private set; }
    public static Song CurrentSong { get; private set; }
    public static int  CurrentIndex { get; private set; }
    public static int  KeyCount => GameSetting.HasFlag( GameMode.KeyConversion ) && CurrentSong.keyCount == 7 ? 6 : CurrentSong.keyCount;
    public static double MainBPM => CurrentSong.mainBPM * GameSetting.CurrentPitch;

    [Header( "Time" )]
    public  static double Playback { get; private set; }
    public  static double Distance { get; private set; }
    private static double DistanceCache;

    private static readonly double AudioLeadIn   = 3000d;
    private static readonly double WaitPauseTime = 2000d;

    private static double StartTime;
    private static double SaveTime;

    [Header( "BGM" )]
    private List<KeySound> bgms = new ();
    private int bgmIndex;
    public static bool UseAllSamples { get; private set; }

    [Header( "Thread" )]
    private CancellationTokenSource breakPoint = new ();
    private int timingIndex;

    //
    public static bool IsStart { get; private set; }
    public static bool IsLoadBGA { get; set; }

    public static Action OnPreInitialize;
    public static Action OnPostInitialize;
    public static event Action<Song> OnParsing;

    protected override async void Awake()
    {
        base.Awake();

        DataStorage.Inst.LoadSongs();
        Songs = DataStorage.OriginSongs;
        if ( Songs.Count > 0 )
             UpdateSong( 0 );

        OnPreInitialize += Initialize; // 변수 초기화
        OnPreInitialize += () => DataStorage.Inst.LoadChart();


        KeySetting   keySetting   = KeySetting.Inst;
        InputManager inputManager = InputManager.Inst;

        await Task.Run( () => UpdateTime( breakPoint.Token ) );
    }

    private void OnApplicationQuit()
    {
        Release();
        breakPoint?.Cancel();
    }

    private async void UpdateTime( CancellationToken _token )
    {
        ReadOnlyCollection<Timing> timings = DataStorage.Timings;
        while ( !_token.IsCancellationRequested )
        {
            // FMOD System Update
            AudioManager.Inst.SystemUpdate();

            if ( IsStart )
            {
                // 시간 갱신
                Playback = SaveTime + ( DateTime.Now.TimeOfDay.TotalMilliseconds - StartTime );

                // 이동된 거리 계산
                for ( int i = timingIndex; i < timings.Count; i++ )
                {
                    double time = timings[i].time;
                    double bpm  = timings[i].bpm / MainBPM;

                    // 지나간 타이밍에 대한 거리
                    if ( i + 1 < timings.Count && timings[i + 1].time < Playback )
                    {
                        timingIndex   += 1;
                        DistanceCache += bpm * ( timings[i + 1].time - time );
                        break;
                    }

                    // 이전 타이밍까지의 거리( 캐싱 ) + 현재 타이밍에 대한 거리
                    Distance = DistanceCache + ( bpm * ( Playback - time ) );
                    break;
                }

                // 배경음 처리( 시간의 흐름에 따라 자동재생 )
                while ( bgmIndex < bgms.Count && bgms[bgmIndex].time <= Playback )
                {
                    if ( DataStorage.Inst.TryGetSound( bgms[bgmIndex].name, out FMOD.Sound sound ) )
                         AudioManager.Inst.Play( sound, bgms[bgmIndex].volume );

                    if ( ++bgmIndex < bgms.Count )
                         UseAllSamples = true;
                }
            }

            await Task.Delay( 1 );
        }
    }

    #region Parsing
    //public void LoadChart()
    //{
    //    // 채보 파싱
    //    using ( FileParser parser = new FileParser() )
    //    {
    //        if ( parser.TryParse( CurrentSong.filePath, out Chart chart ) )
    //        {
    //            CurrentChart = chart;
    //            Timings      = chart.timings;

    //            // 단일 배경음은 자동재생되는 사운드샘플로 재생
    //            if ( !CurrentSong.isOnlyKeySound )
    //                  AddSample( new KeySound( GameSetting.SoundOffset, CurrentSong.audioName, 1f ), SoundType.BGM );

    //            // 사운드샘플 로딩 ( 자동재생 )
    //            for ( int i = 0; i < chart.samples.Count; i++ )
    //                  AddSample( chart.samples[i], SoundType.BGM );
    //        }
    //        else
    //        {
    //            CurrentScene.LoadScene( SceneType.FreeStyle );
    //            Debug.LogWarning( $"Parsing failed  Current Chart : {CurrentSong.title}" );
    //        }
    //    }
    //}

    //public bool LoadSongs()
    //{
    //    List<Song> newSongs = new List<Song>();
    //    Timer timer = new Timer();

    //    // StreamingAsset\\Songs 안의 모든 파일 순회하며 파싱
    //    string[] files = Global.Path.GetFilesInSubDirectories( Global.Path.SoundDirectory, "*.osu" );
    //    for ( int i = 0; i < files.Length; i++ )
    //    {
    //        using ( FileParser parser = new FileParser() )
    //        {
    //            if ( parser.TryParse( files[i], out Song newSong ) )
    //            {
    //                newSong.index = newSongs.Count;
    //                newSongs.Add( newSong );
    //                OnParsing?.Invoke( newSong );
    //            }
    //        }
    //    }

    //    newSongs.Sort( ( _left, _right ) => _left.title.CompareTo( _right.title ) );
    //    for ( int i = 0; i < newSongs.Count; i++ )
    //    {
    //        var song    = newSongs[i];
    //        song.index  = i;
    //        newSongs[i] = song;
    //    }

    //    Songs       = new ReadOnlyCollection<Song>( newSongs );
    //    OriginSongs = new ReadOnlyCollection<Song>( Songs );

    //    if ( Songs.Count > 0 )
    //         UpdateSong( 0 );

    //    Debug.Log( $"Update Songs {timer.End} ms" );

    //    // 파일 수정하고 싶을 때 사용
    //    //for ( int i = 0; i < OriginSongs.Count; i++ )
    //    //{
    //    //    using ( FileParser parser = new FileParser() )
    //    //        parser.ReWrite( OriginSongs[i] );
    //    //}

    //    return true;
    //}
    #endregion

    #region Sound

    /// <summary> 시간의 흐름에 따라 자동으로 재생되는 사운드샘플 </summary>
    public void AddSound( in KeySound _sample, SoundType _type )
    {
        DataStorage.Inst.LoadSound( _sample );
        if ( _type == SoundType.BGM )
             bgms.Add( _sample );
    }
    #endregion

    #region Search
    public void Search( string _keyword )
    {
        ReadOnlyCollection<Song> originSongs = DataStorage.OriginSongs;
        if ( originSongs.Count == 0 )
             return;

        if ( _keyword.Replace( " ", string.Empty ) == string.Empty )
        {
            Songs = originSongs;
            UpdateSong( CurrentSong.index );

            return;
        }

        List<Song> newSongs = new List<Song>();
        bool isSV = _keyword.Replace( " ", string.Empty ).ToUpper().CompareTo( "SV" ) == 0;
        for ( int i = 0; i < originSongs.Count; i++ )
        {
            if ( isSV )
            {
                if ( originSongs[i].minBpm != originSongs[i].maxBpm )
                     newSongs.Add( originSongs[i] );
            }
            else
            {
                if ( originSongs[i].title.Replace(   " ", string.Empty ).Contains( _keyword, StringComparison.OrdinalIgnoreCase ) ||
                     originSongs[i].version.Replace( " ", string.Empty ).Contains( _keyword, StringComparison.OrdinalIgnoreCase ) ||
                     originSongs[i].artist.Replace(  " ", string.Empty ).Contains( _keyword, StringComparison.OrdinalIgnoreCase ) ||
                     originSongs[i].source.Replace(  " ", string.Empty ).Contains( _keyword, StringComparison.OrdinalIgnoreCase ) )
                     newSongs.Add( originSongs[i] );
            }
        }

        Songs = new ReadOnlyCollection<Song>( newSongs );
        if ( Songs.Count != 0 )
             UpdateSong( 0 );
    }

    public void Search( Song _song )
    {
        ReadOnlyCollection<Song> originSongs = DataStorage.OriginSongs;
        for ( int i = 0; i < originSongs.Count; i++ )
        {
            if ( originSongs[i].title.Contains(   _song.title,   StringComparison.OrdinalIgnoreCase ) &&
                 originSongs[i].version.Contains( _song.version, StringComparison.OrdinalIgnoreCase ) )
            { 
                UpdateSong( i );
                return;
            }
        }

        UpdateSong( 0 );
    }

    #endregion
   
    #region Playing
    public void Initialize()
    {
        SaveTime      = -AudioLeadIn;
        Playback      = 0d;
        Distance      = 0d;
        DistanceCache = 0d;
        bgmIndex      = 0;
        timingIndex   = 0;
        IsStart       = false;
        UseAllSamples = false;
    }

    public void Play()
    {
        // 사운드샘플 오름차순 정렬 ( 시간기준 )
        bgms.Sort( delegate ( KeySound _A, KeySound _B )
        {
            if      ( _A.time > _B.time ) return 1;
            else if ( _A.time < _B.time ) return -1;
            else                          return 0;
        } );

        AudioManager.Inst.SetPaused( false, ChannelType.BGM );
        StartTime      = DateTime.Now.TimeOfDay.TotalMilliseconds;
        SaveTime       = -AudioLeadIn;
        IsStart        = true;
    }

    public void Release()
    {
        IsLoadBGA = false;
        
        StopAllCoroutines();
        bgms.Clear();
    }

    public IEnumerator GameOver()
    {
        IsStart = false;
        float slowTimeOffset = 1f / 3f;
        float speed = 1f;
        float pitchOffset = GameSetting.CurrentPitch * .3f;
        ReadOnlyCollection<Timing> timings = DataStorage.Timings;
        while ( true )
        {
            Playback += speed * Time.deltaTime;
            Distance = DistanceCache + ( ( timings[timingIndex].bpm / MainBPM ) * ( Playback - timings[timingIndex].time ) );

            CurrentScene.UpdatePitch( GameSetting.CurrentPitch - ( ( 1f - speed ) * pitchOffset ) );
            speed -= slowTimeOffset * Time.deltaTime;
            if ( speed < 0f )
                 break;

            yield return null;
        }
    }

    /// <returns> FALSE : Playback is higher than the last note time. </returns>
    public void Pause( bool _isPause )
    {
        if ( _isPause )
        {
            IsStart = false;
            SaveTime = Playback - WaitPauseTime;
            AudioManager.Inst.SetPaused( true, ChannelType.BGM );
        }
        else
        {
            StartCoroutine( Continue() );
        }
    }

    private IEnumerator Continue()
    {
        ReadOnlyCollection<Timing> timings = DataStorage.Timings;
        while ( Playback > SaveTime )
        {
            yield return null;

            Playback -= Time.deltaTime * 1.2f;
            Distance = DistanceCache + ( ( timings[timingIndex].bpm / MainBPM ) * ( Playback - timings[timingIndex].time ) );
        }

        StartTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
        IsStart = true;

        WaitUntil waitPlayback = new WaitUntil( () => Playback > SaveTime + WaitPauseTime );
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

        CurrentIndex = _index;
        CurrentSong  = Songs[_index];
    }
    #endregion

    public double GetDistance( double _time )
    {
        double result = 0d;
        ReadOnlyCollection<Timing> timings = DataStorage.Timings;
        for ( int i = 0; i < timings.Count; i++ )
        {
            double time = timings[i].time;
            double bpm  = timings[i].bpm / MainBPM;

            // 지나간 타이밍에 대한 거리
            if ( i + 1 < timings.Count && timings[i + 1].time < _time )
            {
                result += bpm * ( timings[i + 1].time - time );
                continue;
            }

            // 현재 타이밍에 대한 거리
            result += bpm * ( _time - time );
            break;
        }

        return result;
    }
}