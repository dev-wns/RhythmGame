using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

#pragma warning disable CS0162

public class NowPlaying : Singleton<NowPlaying>
{
    public static Scene CurrentScene;

    [Header( "Song" )]
    public static ReadOnlyCollection<Song> OriginSongs { get; private set; } // 원본 음악 리스트
    public static ReadOnlyCollection<Song> Songs       { get; private set; } // 검색 등으로 변환된 리스트
    public static Song CurrentSong { get; private set; }
    public static int CurrentIndex { get; private set; }
    public static int KeyCount     { get; private set; }
    public static int TotalJudge   { get; private set; }
    public static int TotalNote    { get; private set; }
    public static int TotalSlider  { get; private set; }
    public static double MainBPM   { get; private set; }

    [Header( "Time" )]
    public  static double Playback { get; private set; } // ms 기준
    public  static double Distance { get; private set; }
    private        double distanceCache;
    private        double saveTime;
    private        int    bpmIndex;

    private static readonly double AudioLeadIn   = 3000d;
    private static readonly double WaitPauseTime = 2000d;

    [Header( "Thread" )]
    private Task                    timeTask;
    private CancellationTokenSource timeCts;

    // Windows Api
    [DllImport( "Kernel32.dll" )]
    private static extern bool QueryPerformanceCounter( out long performanceCount );
    [DllImport( "Kernel32.dll" )]
    private static extern bool QueryPerformanceFrequency( out long frequency );

    //
    public static bool IsStart  { get; private set; }
    public static bool IsLoaded { get; private set; }

    // Load Event
    public static event Action<Song>  OnParsing;    // 약식 데이터 파싱 ( 프리스타일 곡 목록 등에 사용 )
    public static event Action        OnInitialize; // Main  Thread
    public static event Func<UniTask> OnLoad;       // Main  Thread
    public static event Action        OnLoadAsync;  // Other Thread
    public static event Action        OnLoadEnd;
    public static event Action<double/* Playback */> OnUpdateInThread;

    // Game Event
    public static event Action OnGameStart;
    public static event Action OnGameOver;
    public static event Action OnRelease;
    public static event Action OnClear;          // 변수만 초기화하고 재사용할 때
    public static event Action<bool> OnPause;

    #region Unity Event Function
    protected override void Awake()
    {
        base.Awake();

        LoadSongs();
               
        if ( Songs.Count > 0 )
             UpdateSong( 0 );
    }

    private async void OnApplicationQuit()
    {
        await Release();
    }
    #endregion

    #region Load
    /// <summary> 음악의 기본 정보 파싱 ( 플레이하기 전 프리스타일 등에서 표시될 최소 정보 ) </summary>
    public bool LoadSongs()
    {
        // StreamingAsset\\Songs 안의 모든 파일 순회하며 파싱
        List<Song> newSongs = new List<Song>();
        string[] files = Global.Path.GetFilesInSubDirectories( Global.Path.SoundDirectory, "*.osu" );
        for ( int i = 0; i < files.Length; i++ )
        {
            using ( FileParser parser = new FileParser() )
            {
                if ( parser.TryParse( files[i], out Song newSong ) )
                     newSongs.Add( newSong );

                OnParsing?.Invoke( newSong );
            }
        }

        newSongs.Sort( ( _left, _right ) => _left.title.CompareTo( _right.title ) );
        for ( int i = 0; i < newSongs.Count; i++ )
        {
            Song song   = newSongs[i];
            song.index  = i;
            newSongs[i] = song;
        }
        OriginSongs = new ReadOnlyCollection<Song>( newSongs );
        Songs = OriginSongs;

        // 파일 수정하고 싶을 때 사용
        //for ( int i = 0; i < OriginSongs.Count; i++ )
        //{
        //    using ( FileParser parser = new FileParser() )
        //        parser.ReWrite( OriginSongs[i] );
        //}

        return true;
    }

    /// <summary> 선택된 음악을 바탕으로 초기화 </summary>
    public void Initialize()
    {
        MainBPM = CurrentSong.mainBPM * GameSetting.CurrentPitch;

        // 모드를 선택한 상태로 InGame 진입 후 계산
        bool isConvert  = GameSetting.CurrentGameMode.HasFlag( GameMode.ConvertKey ) &&  CurrentSong.keyCount == 7;
        KeyCount        = isConvert  ? 6 : CurrentSong.keyCount;
        TotalNote       = isConvert  ? CurrentSong.noteCount   - CurrentSong.delNoteCount   : CurrentSong.noteCount;
        TotalSlider     = isConvert  ? CurrentSong.sliderCount - CurrentSong.delSliderCount : CurrentSong.sliderCount;

        bool isNoSlider = GameSetting.CurrentGameMode.HasFlag( GameMode.NoSlider );
        TotalNote       = isNoSlider ? TotalNote + TotalSlider : TotalNote;
        TotalSlider     = isNoSlider ? 0 : TotalSlider;
        TotalJudge      = TotalNote + TotalSlider;

        // 선 파싱 ( 게임에 필요한 모든 정보 파싱 )
        if ( !DataStorage.Inst.LoadChart( CurrentSong ) )
        {
            // Go to FreeStyle
        }

        // 후 계산
        OnInitialize?.Invoke(); // 객체 생성 등의 초기화( 채보 선택 후 한번만 실행 )
    }

    /// <summary> 선택된 음악의 리소스 로딩 </summary>
    public async UniTask Load()
    {
        UniTask task = UniTask.RunOnThreadPool( () => { OnLoadAsync?.Invoke(); } );
        await OnLoad.Invoke();

        await task; // 대기
        OnLoadEnd?.Invoke();

        // 기본 변수 초기화
        Clear();
        IsLoaded = true;
    }
    #endregion

    #region Thread
    private void TimeUpdate( long _targetFrame, CancellationToken _token )
    {
        QueryPerformanceFrequency( out long frequency );
        QueryPerformanceCounter( out long start );

        long end         = 0;
        long startTime   = start;
        long targetTicks = frequency / _targetFrame; // 1 seconds = 10,000,000 ticks
        var  timings     = DataStorage.Timings;
        SpinWait spinner = new SpinWait();
        Debug.Log( $"Time Thread Start( {_targetFrame} Frame )" );
        while ( true )
        {
            _token.ThrowIfCancellationRequested();

            QueryPerformanceCounter( out end );
            if ( targetTicks <= ( end - start ) )
            {
                // 시간 갱신
                Playback = saveTime + ( ( double )( end - startTime ) / frequency * 1000d ); // ms
                // 이동된 거리 계산
                for ( int i = bpmIndex; i < timings.Count; i++ )
                {
                    double time = timings[i].time;
                    double bpm  = timings[i].bpm / MainBPM;

                    // 지나간 타이밍에 대한 거리
                    if ( i + 1 < timings.Count && timings[i + 1].time < Playback )
                    {
                        bpmIndex += 1;
                        distanceCache += bpm * ( timings[i + 1].time - time );
                        break;
                    }

                    // 이전 타이밍까지의 거리( 캐싱 ) + 현재 타이밍에 대한 거리
                    Distance = distanceCache + ( bpm * ( Playback - time ) );
                    break;
                }

                OnUpdateInThread?.Invoke( Playback );
                QueryPerformanceCounter( out start );
            }
            else
            {
                spinner.SpinOnce();
                spinner.Reset();
            }
        }
    }

    private async UniTask StopTimeTask()
    {
        if ( timeCts != null )
        {
            timeCts.Cancel();
            try
            {
                if ( timeTask != null )
                     await timeTask;
            }
            catch ( OperationCanceledException )
            {
                Debug.Log( "Time Thread Cancel" );
            }
            finally
            {
                timeCts.Dispose();
                timeCts  = null;
                timeTask = null;
            }
        }
    }
    #endregion

    #region Search
    public int Search( string _keyword )
    {
        if ( OriginSongs.Count == 0 || _keyword.Replace( " ", string.Empty ) == string.Empty )
        {
            Songs = OriginSongs;
            UpdateSong( CurrentSong.index );

            return -1;
        }

        List<Song> newSongs = new List<Song>();
        string keyword = _keyword.Replace( " ", string.Empty );
        bool   isSV    = keyword.ToUpper().CompareTo( "SV" ) == 0;
        for ( int i = 0; i < OriginSongs.Count; i++ )
        {
            if ( isSV )
            {
                if ( OriginSongs[i].minBpm != OriginSongs[i].maxBpm )
                     newSongs.Add( OriginSongs[i] );
            }
            else
            {
                if ( OriginSongs[i].title.Replace(   " ", string.Empty ).Contains( keyword, StringComparison.OrdinalIgnoreCase ) ||
                     OriginSongs[i].version.Replace( " ", string.Empty ).Contains( keyword, StringComparison.OrdinalIgnoreCase ) ||
                     OriginSongs[i].artist.Replace(  " ", string.Empty ).Contains( keyword, StringComparison.OrdinalIgnoreCase ) ||
                     OriginSongs[i].source.Replace(  " ", string.Empty ).Contains( keyword, StringComparison.OrdinalIgnoreCase ) )
                     newSongs.Add( OriginSongs[i] );
            }
        }

        int prevIndex = 0;
        Songs = newSongs.Count == 0 ? OriginSongs : new ReadOnlyCollection<Song>( newSongs );
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
        for ( int i = 0; i < OriginSongs.Count; i++ )
        {
            if ( OriginSongs[i].title.Contains(   _song.title,   StringComparison.OrdinalIgnoreCase ) &&
                 OriginSongs[i].version.Contains( _song.version, StringComparison.OrdinalIgnoreCase ) )
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

        saveTime      = -AudioLeadIn;
        Playback      = double.MinValue;
        Distance      = double.MinValue;
        distanceCache = 0d;
        bpmIndex      = 0;
        IsStart       = false;

        OnClear?.Invoke();
    }

    public async UniTask Release()
    {
        await StopTimeTask();
        
        IsStart   = false;
        IsLoaded  = false;

        OnRelease?.Invoke();
    }

    public void GameStart()
    {
        OnGameStart?.Invoke();
        timeCts               ??= new CancellationTokenSource();
        timeTask                = Task.Run( () => { TimeUpdate( SystemSetting.InputTargetFrame, timeCts.Token ); } );
        AudioManager.Inst.Pause = false;
        IsStart                 = true;
    }

    public async UniTask GameOver()
    {
        IsStart   = false;
        CurrentScene.IsInputLock = true;

        await StopTimeTask();
        
        float speed = 1f;
        while ( speed > 0f )
        {
            speed -= ( 1f / 3f ) * Time.deltaTime;
            float decrease = ( 1f - speed ) * GameSetting.CurrentPitch * .3f;
            AudioManager.Inst.Pitch = GameSetting.CurrentPitch - decrease;

            Playback += speed * ( 1000f * Time.deltaTime );
            Distance  = distanceCache + ( Playback - DataStorage.Timings[bpmIndex].time );

            await UniTask.Yield( PlayerLoopTiming.Update );
        }

        OnGameOver?.Invoke();
        CurrentScene.IsInputLock = false;
    }

    /// <returns> FALSE : Playback is higher than the last note time. </returns>
    public async UniTask Pause( bool _isPause )
    {
        if ( _isPause )
        {
            IsStart = false;
            await StopTimeTask();

            saveTime = Playback;
            AudioManager.Inst.Pause  = true;
            OnPause?.Invoke( true );
        }
        else
        {
            await Continue();
        }
    }

    private async UniTask Continue()
    {
        CurrentScene.IsInputLock = true;
        double recoil  = Playback - WaitPauseTime;
        var    timings = DataStorage.Timings;
        while ( Playback > recoil )
        {
            Playback -= Time.deltaTime * 1200f;
            Distance = distanceCache + ( Playback - timings[bpmIndex].time );

            await UniTask.Yield( PlayerLoopTiming.Update );
        }

        while ( Playback < saveTime )
        {
            Playback += Time.deltaTime * 1200f;
            Distance = distanceCache + ( Playback - timings[bpmIndex].time );

            await UniTask.Yield( PlayerLoopTiming.Update );
        }

        OnPause?.Invoke( false );
        timeCts                ??= new CancellationTokenSource();
        timeTask                 = Task.Run( () => TimeUpdate( SystemSetting.InputTargetFrame, timeCts.Token ) );
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
        double result  = 0d;
        var    timings = DataStorage.Timings;
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