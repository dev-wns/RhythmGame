using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MeasureSystem : MonoBehaviour
{
    private InGame scene;
    public ObjectPool<MeasureRenderer> mPool;
    public MeasureRenderer mPrefab;
    private List<double /* JudgeLine hit time */> measures = new List<double>();
    private int curIndex;
    private double curTime;
    private double loadTime;
    private static readonly int Beat = 4;

    private class MeasureTiming
    {
        public double time;
        public double bpm;
        public MeasureTiming( Timing _timing )
        {
            time  = _timing.time;
            bpm   = _timing.bpm;
        }
    }

    private void Awake()
    {
        var sceneObj = GameObject.FindGameObjectWithTag( "Scene" );
        if ( sceneObj.TryGetComponent( out scene ) )
        {
            if ( ( GameSetting.CurrentVisualFlag & GameVisualFlag.ShowMeasure ) != 0 )
            {
                scene.OnSystemInitialize += Initialize;
                scene.OnReLoad += ReLoad;
                scene.OnGameStart += () => StartCoroutine( Process() );
                scene.OnScrollChange += UpdateScrollSpeed;
            }
        }

        mPool = new ObjectPool<MeasureRenderer>( mPrefab, 5 );
    }

    private void Start()
    {
        UpdateScrollSpeed();
    }

    private void ReLoad()
    {
        StopAllCoroutines();
        curIndex = 0;
        curTime = 0d;
    }

    private void OnDestroy() => scene.OnScrollChange -= UpdateScrollSpeed;

    private void UpdateScrollSpeed() => loadTime = GameSetting.PreLoadTime;

    public void Despawn( MeasureRenderer _obj ) => mPool.Despawn( _obj );

    private void Initialize( Chart _chart )
    {
        var timings = _chart.uninheritedTimings;
        var totalTime = NowPlaying.Inst.CurrentSong.totalTime;
        for ( int i = 0; i < timings.Count; i++ )
        {
            double spb = ( 60d / timings[i].bpm ) * Beat; // 4박에 1개 생성 ( 60BPM일때 4초마다 1개 생성 )
            double time     = timings[i].time;
            double nextTime = ( i + 1 == timings.Count ) ? ( double )( totalTime * 0.001d ) + 10d : timings[i + 1].time;

            int maxCount =  Mathf.FloorToInt( ( float )( Global.Math.Abs( nextTime - time ) / spb ) );
            measures.Add( NowPlaying.Inst.GetChangedTime( time ) );
            for ( int j = 0; j < maxCount; j++ )
            {
                measures.Add( NowPlaying.Inst.GetChangedTime( ( time + ( ( j + 1 ) * spb ) ) ) );
            }
        }
    }

    private IEnumerator Process()
    {
        if ( measures.Count > 0 )
             curTime = measures[curIndex];
        
        WaitUntil waitNextMeasure = new WaitUntil( () => curTime <= NowPlaying.PlaybackChanged + loadTime );
        while ( curIndex < measures.Count )
        {
            yield return waitNextMeasure;

            MeasureRenderer measure = mPool.Spawn();
            measure.SetInfo( mPool, curTime );

            if ( ++curIndex < measures.Count )
                 curTime = measures[curIndex];
        }
    }
}
