using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
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

    //private static double StartTime;
    private static double SaveTime;
    private int bpmIndex;

    [Header( "BGM" )]
    private int bgmIndex;

    [Header( "Thread" )]
    private Task timeTask;
    private CancellationTokenSource breakPoint = new();
    private readonly long TargetFrame = 8000;
    public static Action OnUpdateThread;

    [DllImport( "Kernel32.dll" )]
    private static extern bool QueryPerformanceCounter( out long lpPerformanceCount );

    [DllImport( "Kernel32.dll" )]
    private static extern bool QueryPerformanceFrequency( out long lpFrequency );

    //
    public static bool IsStart { get; private set; }

    [Header( "Event" )]
    public static bool IsLoaded { get; private set; }

    // Initialize Event
    public static event Action OnPreInit;     // Main  Thread
    public static event Action OnPostInit;    // Main  Thread
    public static event Action OnAsyncInit;   // Other Thread
    public static event Action OnUpdateInThread;

    // Game Event
    public static event Action OnGameStart;
    public static event Action OnGameOver;
    public static event Action OnClear;
    public static event Action OnRelease;
    public static event Action<bool> OnPause;

    protected override void Awake()
    {
        base.Awake();
        // 싱글톤 활성화
        Config        config        = Config.Inst;
        GameSetting   gameSetting   = GameSetting.Inst;
        SystemSetting systemSetting = SystemSetting.Inst;
        Network       network       = Network.Inst;
        AudioManager  audioManager  = AudioManager.Inst;
        InputManager  inputManager  = InputManager.Inst;
        DataStorage   dataStorage   = DataStorage.Inst;
        Judgement     judgement     = Judgement.Inst;

        DataStorage.Inst.LoadSongs();
        Songs = DataStorage.OriginSongs;
        
        if ( Songs.Count > 0 )
             UpdateSong( 0 );
    }

    private async void OnApplicationQuit()
    {
        await Release();
    }

    /// <summary> 게임 시작할 때마다 초기화 </summary>
    public void Initialize()
    {
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
        OnPreInit?.Invoke(); // 객체 생성 등의 초기화( 채보 선택 후 한번만 실행 )
    }

    public async Task Load()
    {
        Task task = Task.Run( () => OnAsyncInit?.Invoke() ); // Other Thread Loading
        OnPostInit?.Invoke();                                // Main  Thread Loading
        Clear();                                             // 기본 변수 초기화

        await task;
        IsLoaded = true;
    }

    private void TimeUpdate( long _targetFrame, CancellationToken _token )
    {
        QueryPerformanceFrequency( out long frequency );
        QueryPerformanceCounter( out long start );

        long end         = 0;
        long startTime   = start;
        long targetTicks = frequency / _targetFrame; // 1 seconds = 10,000,000 ticks
        SpinWait spinner = new SpinWait();

        Debug.Log( $"Time Thread Start( {_targetFrame} Frame, {targetTicks} ticks )" );
        while ( true )
        {
            _token.ThrowIfCancellationRequested();

            QueryPerformanceCounter( out end );
            if ( targetTicks <= ( end - start ) )
            {
                // 시간 갱신
                Playback = SaveTime + ( ( double )( end - startTime ) / frequency * 1000d ); // ms
                // 이동된 거리 계산
                for ( int i = bpmIndex; i < Timings.Count; i++ )
                {
                    double time = Timings[i].time;
                    double bpm  = Timings[i].bpm / MainBPM;

                    // 지나간 타이밍에 대한 거리
                    if ( i + 1 < Timings.Count && Timings[i + 1].time < Playback )
                    {
                        bpmIndex += 1;
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
                    if ( DataStorage.Inst.GetSound( Samples[bgmIndex].name, out FMOD.Sound sound ) )
                    {
                        AudioManager.Inst.Play( sound, Samples[bgmIndex].volume );
                        Debug.Log( "Music Play" );
                    }

                    bgmIndex += 1;
                }

                OnUpdateInThread?.Invoke();
                QueryPerformanceCounter( out start );
            }
            else
            {
                //System.Threading.Thread.SpinWait( 1 );
                spinner.SpinOnce();
                spinner.Reset();
            }
        }
    }
    private async Task ThreadCancel()
    {
        breakPoint?.Cancel();
        try
        {
            if ( timeTask is not null )
                 await timeTask;
        }
        catch ( OperationCanceledException )
        {
            Debug.Log( "Time Thread Cancel Completed" );
        }
        finally
        {
            breakPoint?.Dispose();
            breakPoint = null;
            timeTask   = null;
        }
    }

    #region Search
    public int Search( string _keyword )
    {
        ReadOnlyCollection<Song> originSongs = DataStorage.OriginSongs;
        if ( originSongs.Count == 0 || _keyword.Replace( " ", string.Empty ) == string.Empty )
        {
            Songs = originSongs;
            UpdateSong( CurrentSong.index );

            return -1;
        }

        List<Song> newSongs = new List<Song>();
        string keyword = _keyword.Replace( " ", string.Empty );
        bool   isSV    = keyword.ToUpper().CompareTo( "SV" ) == 0;
        for ( int i = 0; i < originSongs.Count; i++ )
        {
            if ( isSV )
            {
                if ( originSongs[i].minBpm != originSongs[i].maxBpm )
                     newSongs.Add( originSongs[i] );
            }
            else
            {
                if ( originSongs[i].title.Replace(   " ", string.Empty ).Contains( keyword, StringComparison.OrdinalIgnoreCase ) ||
                     originSongs[i].version.Replace( " ", string.Empty ).Contains( keyword, StringComparison.OrdinalIgnoreCase ) ||
                     originSongs[i].artist.Replace(  " ", string.Empty ).Contains( keyword, StringComparison.OrdinalIgnoreCase ) ||
                     originSongs[i].source.Replace(  " ", string.Empty ).Contains( keyword, StringComparison.OrdinalIgnoreCase ) )
                     newSongs.Add( originSongs[i] );
            }
        }

        int prevIndex = 0;
        Songs = newSongs.Count == 0 ? originSongs : new ReadOnlyCollection<Song>( newSongs );
        for ( int i = 0; i < Songs.Count; i++ )
        {
            if ( Songs[i].title.Contains(   CurrentSong.title,   StringComparison.OrdinalIgnoreCase ) &&
                 Songs[i].version.Contains( CurrentSong.version, StringComparison.OrdinalIgnoreCase ) )
            {
                prevIndex = i;
                break;
            }
        }

        UpdateSong( prevIndex );

        return newSongs.Count;
    }

    public void Search( Song _song )
    {
        int prevIndex = 0;
        ReadOnlyCollection<Song> originSongs = DataStorage.OriginSongs;
        for ( int i = 0; i < originSongs.Count; i++ )
        {
            if ( originSongs[i].title.Contains(   _song.title,   StringComparison.OrdinalIgnoreCase ) &&
                 originSongs[i].version.Contains( _song.version, StringComparison.OrdinalIgnoreCase ) )
            {
                prevIndex = i;
                break;
            }
        }

        UpdateSong( prevIndex );
    }
    #endregion
   
    #region Playing
    /// <summary> 기본 변수 초기화 </summary>
    public void Clear()
    {
        AudioManager.Inst.AllStop();

        SaveTime      = -AudioLeadIn;
        Playback      = double.MinValue;
        Distance      = double.MinValue;
        DistanceCache = 0d;
        bgmIndex      = 0;
        bpmIndex      = 0;
        IsStart       = false;

        OnClear?.Invoke();
    }

    public void GameStart()
    {
        OnGameStart?.Invoke();
        breakPoint            ??= new CancellationTokenSource();
        timeTask                = Task.Run( () => { TimeUpdate( TargetFrame, breakPoint.Token ); } );
        AudioManager.Inst.Pause = false;
        IsStart                 = true;
    }

    public async Task Release()
    {
        StopAllCoroutines();
        await ThreadCancel();
        OnRelease?.Invoke();
        IsLoaded  = false;
    }

    public IEnumerator GameOver()
    {
        IsStart   = false;
        Task task = ThreadCancel();
        yield return new WaitUntil( () => task.IsCompleted );

        float speed = 1f;
        while ( speed > 0f )
        {
            speed -= ( 1f / 3f ) * Time.deltaTime;
            float decrease = ( 1f - speed ) * GameSetting.CurrentPitch * .3f;
            AudioManager.Inst.Pitch = GameSetting.CurrentPitch - decrease;

            Playback += speed * ( 1000f * Time.deltaTime );
            Distance  = DistanceCache + ( Playback - Timings[bpmIndex].time );
            
            yield return null;
        }

        OnGameOver?.Invoke();
    }

    /// <returns> FALSE : Playback is higher than the last note time. </returns>
    public async void Pause( bool _isPause )
    {
        if ( _isPause )
        {
            IsStart = false;
            await ThreadCancel();

            SaveTime = Playback;
            AudioManager.Inst.Pause  = true;
            OnPause?.Invoke( true );
        }
        else
        {
            StartCoroutine( Continue() );
        }
    }

    private IEnumerator Continue()
    {
        CurrentScene.IsInputLock = true;
        double recoil = Playback - WaitPauseTime;
        while ( Playback > recoil )
        {
            Playback -= Time.deltaTime * 1200f;
            Distance = DistanceCache + ( Playback - Timings[bpmIndex].time );

            yield return null;
        }

        while ( Playback < SaveTime )
        {
            Playback += Time.deltaTime * 1200f;
            Distance = DistanceCache + ( Playback - Timings[bpmIndex].time );

            yield return null;
        }

        OnPause?.Invoke( false );
        breakPoint             ??= new CancellationTokenSource();
        timeTask                 = Task.Run( () => { TimeUpdate( TargetFrame, breakPoint.Token ); } );
        AudioManager.Inst.Pause  = false;
        IsStart                  = true;
        CurrentScene.IsInputLock = false;
    }
    #endregion

    public void UpdateSong( int _index )
    {
        if ( _index >= Songs.Count )
             throw new Exception( "out of range" );

        CurrentIndex = _index;
        CurrentSong  = Songs[_index];
    }

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