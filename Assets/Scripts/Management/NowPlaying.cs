using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using UnityEditor;
using UnityEngine;

#pragma warning disable CS0162

public class NowPlaying : Singleton<NowPlaying>
{
    public static Scene CurrentScene;

    private readonly Timer     SysTimer   = new ();
    private readonly GameTimer LogicTimer = new ();

    public Song      CurrentSong   { get; private set; } // 게임 모드 등으로 인해 바뀐 음악 정보
    public Record    CurrentRecord { get; private set; } = new ();

    public  static readonly int AudioLeadIn   = -3000;
    private static readonly int WaitPauseTime = 2000;

    [Header( "Time" )]
    public  static int    Playback  { get; private set; } // ms 기준
    public  static double Distance  { get; private set; }

    private TimingMap timingMap = new ();
    //private int       saveMs;

    //private long   frequency;
    //private long   startTick;
    //private double distanceCache;
    //private int    bpmIndex;

    [DllImport( "Assistant" )] private static extern void SetTimerResolution( bool isHighQuality );
    
    public static bool IsStart  { get; private set; }
    public static event Action<double/* Playback */> OnUpdateInThread;

    protected override void Awake()
    {
        base.Awake();

        SystemSetting.Inst.ReadUserData();
        GameSetting.Inst.ReadUserData();
        InputManager.Inst.ReadUserData();

        AudioManager.Inst.Initialize();

        Judgement judgement = Judgement.Inst;
        Browser   browser   = Browser.Inst;

        SetTimerResolution( true );
    }

    private void OnDestroy()
    {
        SetTimerResolution( false );
        
        IsStart = false;
    }

    private void Update()
    {
        if ( !IsStart ) return;

        // AudioLeadIn +
        Playback = ( int )Math.Round( LogicTimer.Milliseconds * GameSetting.SoundPitch );
        //Distance = timingMap.TimeToDistance( Playback );
    }

    /// <summary> 선택된 음악을 바탕으로 초기화 </summary>
    public void Initialize( in Song _song )
    {
        timingMap.Initialize( in _song );
        CurrentSong = GameSetting.Inst.ApplyGameMode( in _song );

        Clear();
    }

    //private void TimeUpdate( long _targetFrame, CancellationToken _token )
    //{
    //    GetPerformanceCounter( out startTick );

    //    long logicInterval = Frequency / _targetFrame;
    //    long logicNextTick = startTick + logicInterval;

    //    //var  timings     = DataStorage.Inst.Timings;
    //    SpinWait spinner = new SpinWait();

    //    Debug.Log( $"Time Thread Start( {_targetFrame} Frame )" );
    //    while ( !_token.IsCancellationRequested )
    //    {
    //        GetPerformanceCounter( out long now );
    //        Playback = TickToPlayback( now );
    //        if ( now >= logicNextTick )
    //        {
    //            do
    //            {
    //                if ( _token.IsCancellationRequested ) break;
    //                logicNextTick += logicInterval;

    //            } while ( now >= logicNextTick );

    //            // OnUpdateInThread?.Invoke( Playback );
    //            LogicFps?.Update( now );
    //            spinner.Reset();

    //            //// 이동된 거리 계산
    //            //for ( int i = bpmIndex; i < timings.Count; i++ )
    //            //{
    //            //    double time = timings[i].time;
    //            //    double bpm  = timings[i].bpm / CurrentSong.mainBPM;

    //            //    // 지나간 타이밍에 대한 거리
    //            //    if ( i + 1 < timings.Count && timings[i + 1].time < Playback )
    //            //    {
    //            //        bpmIndex += 1;
    //            //        distanceCache += bpm * ( timings[i + 1].time - time );
    //            //        break;
    //            //    }

    //            //    // 이전 타이밍까지의 거리( 캐싱 ) + 현재 타이밍에 대한 거리
    //            //    Distance = distanceCache + ( bpm * ( Playback - time ) );
    //            //    break;
    //            //}
    //        }
    //        else spinner.SpinOnce();
            
    //        SpinCount = spinner.Count;
    //        LoopFps?.Update( now );
    //    }

    //    Debug.Log( "Time Thread End" );
    //}
   
    #region Playing
    /// <summary> 기본 변수 초기화 </summary>
    public void Clear()
    {
        AudioManager.Inst.AllStop();

        //saveMs        = -AudioLeadIn;
        Playback      = int.MinValue;
        Distance      = double.MinValue;
        //distanceCache = 0d;
        //startTick     = 0;
        //bpmIndex      = 0;
        IsStart       = false;

        Judgement.Inst.Clear();

        //OnClear?.Invoke();
    }

    public async UniTask Release()
    {
        //await StopTimeTask();
        //if ( EnableRawInput( false ) ) Debug.LogWarning( "Assistant.dll Release Successed" );
        //else                           Debug.LogWarning( "Assistant.dll Release Failed" );

        IsStart   = false;
        //IsLoaded  = false;

        //OnRelease?.Invoke();
    }

    public void GameStart()
    {
        //OnGameStart?.Invoke();

        //if ( EnableRawInput( true ) ) Debug.LogWarning( "Assistant.dll Initialize Successed" );
        //else                          Debug.LogWarning( "Assistant.dll Initialize Failed" );
        // timeCts ??= new CancellationTokenSource();
        // timeTask = UniTask.RunOnThreadPool( () => { TimeUpdate( SystemSetting.InputTargetFrame, timeCts.Token ); } );
        AudioManager.Inst.Pause = false;
        IsStart                 = true;
    }

    public async UniTask GameOver()
    {
        IsStart   = false;
        CurrentScene.IsInputLock = true;

        //await StopTimeTask();
        
        float speed = 1f;
        while ( speed > 0f )
        {
            speed -= ( 1f / 3f ) * Time.deltaTime;
            float decrease = ( 1f - speed ) * GameSetting.SoundPitch * .3f;
            AudioManager.Inst.Pitch = GameSetting.SoundPitch - decrease;

            Playback += ( int )Math.Round( ( speed * 1000f ) * Time.deltaTime );
            //Distance  = distanceCache + ( Playback - DataStorage.Inst.Timings[bpmIndex].time );

            await UniTask.Yield( PlayerLoopTiming.Update );
        }

        //OnGameOver?.Invoke();
        CurrentScene.IsInputLock = false;
    }

    public void Pause()
    {
        IsStart = false;

        AudioManager.Inst.Pause = true;
        LogicTimer.Pause();
    }

    public void Resume()
    {
        IsStart = true;

        AudioManager.Inst.Pause = false;
        LogicTimer.Resume();
    }

    /// <returns> FALSE : Playback is higher than the last note time. </returns>
    //public async UniTask Pause( bool _isPause )
    //{
    //    if ( _isPause )
    //    {
    //        IsStart = false;

    //        LogicTimer.Pause();
    //        AudioManager.Inst.Pause = true;

    //        //await StopTimeTask();
    //        //if ( EnableRawInput( false ) ) Debug.LogWarning( "Assistant.dll Release Successed" );
    //        //else                           Debug.LogWarning( "Assistant.dll Release Failed" );

    //        //saveMs = Playback;
    //        //AudioManager.Inst.Pause  = true;
    //        //OnPause?.Invoke( true );
    //    }
    //    else
    //    {
    //        await Continue();
    //    }
    //}

    //private async UniTask Continue()
    //{
    //    CurrentScene.IsInputLock = true;
    //    double recoil  = Playback - WaitPauseTime;
    //    var    timings = DataStorage.Inst.Timings;
    //    while ( Playback > recoil )
    //    {
    //        Playback -= ( int )Math.Round( Time.deltaTime * 1200f );
    //        //Distance = distanceCache + ( Playback - timings[bpmIndex].time );

    //        await UniTask.Yield( PlayerLoopTiming.Update );
    //    }

    //    while ( Playback < saveMs )
    //    {
    //        Playback += ( int ) Math.Round( Time.deltaTime * 1200f );
    //        //Distance = distanceCache + ( Playback - timings[bpmIndex].time );

    //        await UniTask.Yield( PlayerLoopTiming.Update );
    //    }

    //    //if ( EnableRawInput( true ) ) Debug.LogWarning( "Assistant.dll Initialize Successed" );
    //    //else                          Debug.LogWarning( "Assistant.dll Initialize Failed" );

    //    //OnPause?.Invoke( false );
    //    //timeCts ??= new CancellationTokenSource();
    //    //timeTask = UniTask.RunOnThreadPool( () => TimeUpdate( SystemSetting.InputTargetFrame, timeCts.Token ) );
    //    AudioManager.Inst.Pause  = false;
    //    IsStart                  = true;
    //    CurrentScene.IsInputLock = false;
    //}
    #endregion

    //public int TickToPlayback( long _tick )
    //{
    //    double second = ( double )( _tick - startTick ) / frequency; // double Type Divide
    //    return saveMs + ( int )Math.Round( second * 1000d );
    //}

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