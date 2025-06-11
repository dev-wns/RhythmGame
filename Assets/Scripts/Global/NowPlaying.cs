#define START_FREESTYLE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

#pragma warning disable CS0162

public class NowPlaying : Singleton<NowPlaying>
{
    public static Scene CurrentScene;

    [Header( "Song" )]
    public ReadOnlyCollection<Song> Songs { get; private set; }
    public static Song CurrentSong { get; private set; }
    public static int CurrentIndex { get; private set; }
    public static int KeyCount     { get; private set; }
    public static int TotalJudge   { get; private set; }
    public static int TotalNote    { get; private set; }
    public static int TotalSlider  { get; private set; }
    public static double MainBPM   { get; private set; }

    [Header( "Time" )]
    private ReadOnlyCollection<Timing>   Timings;
    private ReadOnlyCollection<KeySound> Samples;
    public  static double Playback { get; private set; }
    public  static double Distance { get; private set; }
    private static double DistanceCache;

    private static readonly double AudioLeadIn   = 3000d;
    private static readonly double WaitPauseTime = 2000d;

    private static double StartTime;
    private static double SaveTime;
    private int timingIndex;

    [Header( "BGM" )]
    private int bgmIndex;

    [Header( "Thread" )]
    private CancellationTokenSource breakPoint = new ();

    //
    public static bool IsStart { get; private set; }

    [Header( "Event" )]
    public static bool IsLoaded { get; private set; }
    // public static event Action OnPreInitialize;  // Main  Thread
    // public static event Action OnPostInitialize; // Main  Thread
    // public static event Action OnPreInitAsync;   // Other Thread
    // public static event Action OnPostInitAsync;  // Other Thread

    public static event Action OnPreInit;     // Main  Thread
    public static event Action OnPostInit;    // Main  Thread
    public static event Action OnAsyncInit;   // Other Thread



    public static event Action OnUpdateInThread;

    public static event Action<Song> OnParsing;


    protected override async void Awake()
    {
        base.Awake();
        
        // 싱글톤 활성화
        AudioManager audioManager = AudioManager.Inst;
        InputManager inputManager = InputManager.Inst;
        DataStorage  dataStorage  = DataStorage.Inst;
        Judgement    judgement    = Judgement.Inst;
        Network      network      = Network.Inst;

        DataStorage.Inst.LoadSongs();
        Songs = DataStorage.OriginSongs;
        if ( Songs.Count > 0 )
             UpdateSong( 0 );

        await Task.Run( () => UpdateTime( breakPoint.Token ) );
    }

    private void OnApplicationQuit()
    {
        Release();
        breakPoint?.Cancel();
    }

    /// <summary> 게임 시작할 때마다 초기화 </summary>
    public async void Initialize()
    {
        Clear(); // 기본 변수 초기화
        MainBPM = CurrentSong.mainBPM * GameSetting.CurrentPitch;

        // 모드를 선택한 상태로 InGame 진입 후 계산
        bool isNoSlider = GameSetting.CurrentGameMode.HasFlag( GameMode.NoSlider );
        bool isConvert  = GameSetting.CurrentGameMode.HasFlag( GameMode.ConvertKey ) &&  CurrentSong.keyCount == 7;
        KeyCount        = isConvert  ? 6 : CurrentSong.keyCount;
        TotalNote       = isConvert  ? CurrentSong.noteCount - CurrentSong.delNoteCount : CurrentSong.noteCount;
        TotalSlider     = isNoSlider ? 0 :
                          isConvert  ? CurrentSong.sliderCount - CurrentSong.delSliderCount : 
                                       CurrentSong.sliderCount;
        TotalNote       = isNoSlider ? TotalNote + TotalSlider : TotalNote;
        TotalJudge      = TotalNote + ( TotalSlider * 2 );

        // 선 파싱
        DataStorage.Inst.LoadChart();

        // 가독성을 위한 자주쓰는 변수 캐싱
        Timings = DataStorage.Timings;
        Samples = DataStorage.Samples;

        // 후 계산
        OnPreInit?.Invoke(); // 작업 전 클래스 초기화 등
        await Task.Run( () => OnAsyncInit?.Invoke() );
        OnPostInit?.Invoke();
        IsLoaded = true;
    }

    private async void UpdateTime( CancellationToken _token )
    {
        Debug.Log( $"Time Thread Start" );

        while ( !_token.IsCancellationRequested )
        {
            // FMOD System Update
            AudioManager.Inst.SystemUpdate();

            if ( IsStart )
            {
                // 시간 갱신
                Playback = SaveTime + ( DateTime.Now.TimeOfDay.TotalMilliseconds - StartTime );

                // 이동된 거리 계산
                for ( int i = timingIndex; i < Timings.Count; i++ )
                {
                    double time = Timings[i].time;
                    double bpm  = Timings[i].bpm / MainBPM;

                    // 지나간 타이밍에 대한 거리
                    if ( i + 1 < Timings.Count && Timings[i + 1].time < Playback )
                    {
                        timingIndex   += 1;
                        DistanceCache += bpm * ( Timings[i + 1].time - time );
                        break;
                    }

                    // 이전 타이밍까지의 거리( 캐싱 ) + 현재 타이밍에 대한 거리
                    Distance = DistanceCache + ( bpm * ( Playback - time ) );
                    break;
                }

                // 배경음 처리( 시간의 흐름에 따라 자동재생 )
                while ( bgmIndex < Samples.Count && Samples[bgmIndex].time <= Playback )
                {
                    if ( DataStorage.Inst.TryGetSound( Samples[bgmIndex].name, out FMOD.Sound sound ) )
                         AudioManager.Inst.Play( sound, Samples[bgmIndex].volume );

                    bgmIndex += 1;
                }

                OnUpdateInThread?.Invoke();
            }

            await Task.Delay( 1 );
        }

        Debug.Log( $"Time Thread End" );
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
    /// <summary> 기본 변수 초기화 </summary>
    public void Clear()
    {
        SaveTime      = -AudioLeadIn;
        Playback      = 0d;
        Distance      = 0d;
        DistanceCache = 0d;
        bgmIndex      = 0;
        timingIndex   = 0;
        IsStart       = false;
    }

    public void Play()
    {
        AudioManager.Inst.SetPaused( false, ChannelType.BGM );
        StartTime      = DateTime.Now.TimeOfDay.TotalMilliseconds;
        SaveTime       = -AudioLeadIn;
        IsStart        = true;
    }

    public void Release()
    {
        IsLoaded  = false;
        StopAllCoroutines();
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
            Distance = DistanceCache + ( ( Timings[timingIndex].bpm / MainBPM ) * ( Playback - Timings[timingIndex].time ) );

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
        while ( Playback > SaveTime )
        {
            yield return null;

            Playback -= Time.deltaTime * 1.2f;
            Distance = DistanceCache + ( ( Timings[timingIndex].bpm / MainBPM ) * ( Playback - Timings[timingIndex].time ) );
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
        for ( int i = 0; i < Timings.Count; i++ )
        {
            double time = Timings[i].time;
            double bpm  = Timings[i].bpm / MainBPM;

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