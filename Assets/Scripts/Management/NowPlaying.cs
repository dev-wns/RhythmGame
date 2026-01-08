using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Concurrent;
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

    public Song      CurrentSong   { get; private set; } // 게임 모드 등으로 인해 바뀐 음악 정보
    //public Results CurrentResult { get; private set; } = new ();
    public Record    CurrentRecord { get; private set; } = new ();

    [Header( "Time" )]
    public  static double Playback { get; private set; } // ms 기준
    public  static double Distance { get; private set; }
    private long   frequency;
    private long   startTick;
    private double distanceCache;
    private double saveTime;
    private int    bpmIndex;

    public  static readonly double AudioLeadIn   = 3000d;
    private static readonly double WaitPauseTime = 2000d;

    [Header( "Thread" )]
    private UniTask                 timeTask;
    private CancellationTokenSource timeCts;

    //// DLL
    [DllImport( "Assistant" )]
    public static extern bool EnableRawInput( bool isEnable );
    [DllImport( "Assistant" )]
    private static extern bool GetPerformanceCounter( out long performanceCount );
    [DllImport( "Assistant" )]
    private static extern bool GetPerformanceFrequency( out long frequency );
    [DllImport( "Assistant" )]
    private static extern void SetTimerResolution( bool isHighQuality );
    //
    public static bool IsStart  { get; private set; }
    //public static bool IsLoaded { get; private set; }

    // Load Event
    //public static event Action<Song>  OnParsing;    // 약식 데이터 파싱 ( 프리스타일 곡 목록 등에 사용 )
    //public static event Action        OnInitialize; // Main  Thread
    //public static event Func<UniTask> OnLoad;       // Main  Thread
    //public static event Action        OnLoadAsync;  // Other Thread
    //public static event Action        OnLoadEnd;
    //public static event Action<double/* Playback */> OnUpdate;
    public static event Action<double/* Playback */> OnUpdateInThread;

    // Game Event
    //public static event Action OnGameStart;
    //public static event Action OnGameOver;
    //public static event Action OnRelease;
    //public static event Action OnClear;          // 변수만 초기화하고 재사용할 때
    //public static event Action<bool> OnPause;

    #region Unity Event Function
    protected override void Awake()
    {
        base.Awake();

        //SetTimerResolution( true );
        GetPerformanceFrequency( out frequency );

        AudioManager.Inst.Initialize();
        GameSetting.Inst.Initialize();
        SystemSetting.Inst.Initialize();
        InputManager.Inst.Initialize();

        Judgement judgement = Judgement.Inst;
        Browser   browser   = Browser.Inst;
    }

    private async void OnDestroy()
    {
        //SetTimerResolution( false );
        await Release();
    }

    //private void Update()
    //{
    //    if ( IsLoaded && IsStart )
    //         OnUpdate?.Invoke( Playback );
    //}
    #endregion

    #region Load
    /// <summary> 선택된 음악을 바탕으로 초기화 </summary>
    public void Initialize( in Song _song )
    {
        Song song    = _song;
        song.mainBPM = _song.mainBPM * GameSetting.SoundPitch;

        // 게임 모드가 적용되어 바뀐 정보
        bool isConvert   = GameSetting.CurrentGameMode.HasFlag( GameMode.ConvertKey ) &&  _song.keyCount == 7;
        song.keyCount    = isConvert ? 6 : _song.keyCount;
        song.noteCount   = isConvert ? _song.noteCount   - _song.delNoteCount   : _song.noteCount;
        song.sliderCount = isConvert ? _song.sliderCount - _song.delSliderCount : _song.sliderCount;

        bool isNoSlider  = GameSetting.CurrentGameMode.HasFlag( GameMode.NoSlider );
        song.noteCount   = isNoSlider ? song.noteCount + song.sliderCount : song.noteCount;
        song.sliderCount = isNoSlider ? 0 : song.sliderCount;

        GameSetting.Inst.UpdatePosition( song.keyCount );
        CurrentSong = song;
        Clear();

        // // 선 파싱 ( 게임에 필요한 모든 정보 파싱 )
        // if ( !DataStorage.Inst.LoadChart( _song ) )
        // {
        //     // Go to FreeStyle
        // }
        // 
        // // 후 계산
        // OnInitialize?.Invoke(); // 객체 생성 등의 초기화( 채보 선택 후 한번만 실행 )
    }

    /// <summary> 선택된 음악의 리소스 로딩 </summary>
    //public async UniTask Load()
    //{
        //await UniTask.WhenAll( UniTask.RunOnThreadPool( () => OnLoadAsync?.Invoke() ),
        //                       UniTask.Create( async () =>
        //                       {
        //                           if ( OnLoad == null )
        //                               return;

        //                           foreach ( Func<UniTask> handler in OnLoad.GetInvocationList() )
        //                               await handler(); // 구독자 전부 대기
        //                       } ) );
        //OnLoadEnd?.Invoke();

        // 기본 변수 초기화
        //Clear();
        //IsLoaded = true;
    //}
    #endregion

    #region Thread
    private void TimeUpdate( long _targetFrame, CancellationToken _token )
    {
        GetPerformanceCounter( out startTick );

        long endTick         = 0;
        long targetTicks = frequency / _targetFrame; // 1 seconds = 10,000,000 ticks
        var  timings     = DataStorage.Inst.Timings;
        SpinWait spinner = new SpinWait();

        Debug.Log( $"Time Thread Start( {_targetFrame} Frame )" );
        while ( true )
        {
            _token.ThrowIfCancellationRequested();

            GetPerformanceCounter( out endTick );
            if ( targetTicks <= ( endTick - startTick ) )
            {
                // 시간 갱신
                Playback = TickToPlayback( endTick );// saveTime + ( ( double )( endTick - startTick ) / frequency * 1000d ); // ms
                // 이동된 거리 계산
                for ( int i = bpmIndex; i < timings.Count; i++ )
                {
                    double time = timings[i].time;
                    double bpm  = timings[i].bpm / CurrentSong.mainBPM;

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
            }

            spinner.SpinOnce();
            //spinner.Reset();
        }
    }

    private async UniTask StopTimeTask()
    {
        if ( timeCts != null )
        {
            timeCts?.Cancel();

            try { await timeTask; }
            catch ( OperationCanceledException )
            {
                Debug.Log( "Time Thread Cancel" );
            }
            finally
            {
                timeCts?.Dispose();
                timeCts = null;
            }
        }
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
        startTick     = 0;
        bpmIndex      = 0;
        IsStart       = false;

        Judgement.Inst.Clear();

        //OnClear?.Invoke();
    }

    public async UniTask Release()
    {
        await StopTimeTask();
        if ( EnableRawInput( false ) ) Debug.LogWarning( "Assistant.dll Release Successed" );
        else                           Debug.LogWarning( "Assistant.dll Release Failed" );

        IsStart   = false;
        //IsLoaded  = false;

        //OnRelease?.Invoke();
    }

    public void GameStart()
    {
        //OnGameStart?.Invoke();

        if ( EnableRawInput( true ) ) Debug.LogWarning( "Assistant.dll Initialize Successed" );
        else                          Debug.LogWarning( "Assistant.dll Initialize Failed" );
        timeCts ??= new CancellationTokenSource();
        timeTask = UniTask.RunOnThreadPool( () => { TimeUpdate( SystemSetting.InputTargetFrame, timeCts.Token ); } );
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
            float decrease = ( 1f - speed ) * GameSetting.SoundPitch * .3f;
            AudioManager.Inst.Pitch = GameSetting.SoundPitch - decrease;

            Playback += speed * ( 1000f * Time.deltaTime );
            Distance  = distanceCache + ( Playback - DataStorage.Inst.Timings[bpmIndex].time );

            await UniTask.Yield( PlayerLoopTiming.Update );
        }

        //OnGameOver?.Invoke();
        CurrentScene.IsInputLock = false;
    }

    /// <returns> FALSE : Playback is higher than the last note time. </returns>
    public async UniTask Pause( bool _isPause )
    {
        if ( _isPause )
        {
            IsStart = false;
            await StopTimeTask();
            if ( EnableRawInput( false ) ) Debug.LogWarning( "Assistant.dll Release Successed" );
            else                           Debug.LogWarning( "Assistant.dll Release Failed" );

            saveTime = Playback;
            AudioManager.Inst.Pause  = true;
            //OnPause?.Invoke( true );
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
        var    timings = DataStorage.Inst.Timings;
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

        if ( EnableRawInput( true ) ) Debug.LogWarning( "Assistant.dll Initialize Successed" );
        else                          Debug.LogWarning( "Assistant.dll Initialize Failed" );

        //OnPause?.Invoke( false );
        timeCts ??= new CancellationTokenSource();
        timeTask = UniTask.RunOnThreadPool( () => TimeUpdate( SystemSetting.InputTargetFrame, timeCts.Token ) );
        AudioManager.Inst.Pause  = false;
        IsStart                  = true;
        CurrentScene.IsInputLock = false;
    }
    #endregion

    public double TickToPlayback( long _tick )
    {
        return saveTime + ( ( double )( _tick - startTick ) / frequency * 1000d ); // ms
    }

    public double GetDistance( double _time )
    {
        double result  = 0d;
        var    timings = DataStorage.Inst.Timings;
        for ( int i = 0; i < timings.Count; i++ )
        {
            double time = timings[i].time;
            double bpm  = timings[i].bpm / CurrentSong.mainBPM;

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