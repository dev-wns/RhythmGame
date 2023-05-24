using System.Collections.Generic;
using UnityEngine;

public class MeasureSystem : MonoBehaviour
{
    private InGame scene;
    public ObjectPool<MeasureRenderer> pool;
    public MeasureRenderer mPrefab;
    private List<double/* ScaledTime */> measures = new List<double>();
    private int curIndex;
    private double curTime;
    private static readonly int Beat = 4;

    private bool shouldShowMeasure;

    private void Awake()
    {
        var sceneObj = GameObject.FindGameObjectWithTag( "Scene" );
        shouldShowMeasure = ( GameSetting.CurrentVisualFlag & GameVisualFlag.ShowMeasure ) != 0;
        if ( sceneObj.TryGetComponent( out scene ) )
        {
            if ( shouldShowMeasure )
            {
                scene.OnSystemInitialize += Initialize;
                scene.OnReLoad += ReLoad;
                NowPlaying.Inst.OnSpawnObjects += SpawnMeasures;
            }
        }

        pool = new ObjectPool<MeasureRenderer>( mPrefab, 10 );
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        if ( shouldShowMeasure )
             NowPlaying.Inst.OnSpawnObjects -= SpawnMeasures;
    }

    private void ReLoad()
    {
        curIndex = 0;
        curTime = measures[curIndex];

        pool.AllDespawn();
    }

    private void Initialize( Chart _chart )
    {
        var timings   = _chart.uninheritedTimings;
        var totalTime = NowPlaying.CurrentSong.totalTime * .001d;
        for ( int i = 0; i < timings.Count; i++ )
        {
            double time     = timings[i].time;
            double nextTime = ( i + 1 == timings.Count ) ? ( double )( totalTime + 60d ) : timings[i + 1].time;
            double spb      = ( 60d / timings[i].bpm ) * Beat; // 4박에 1개 생성 ( 60BPM일때 4초마다 1개 생성 )

            while ( time < nextTime )
            {
                measures.Add( NowPlaying.Inst.GetScaledPlayback( time ) );
                time += spb;
            }
        }

        if ( measures.Count > 0 )
             curTime = measures[curIndex];
    }

    private void SpawnMeasures( double _playback, double _scaledPlayback )
    {
        while ( curIndex < measures.Count && curTime <= _scaledPlayback + GameSetting.PreLoadTime )
        {
            MeasureRenderer measure = pool.Spawn();
            measure.SetInfo( curTime );

            if ( ++curIndex < measures.Count )
                 curTime = measures[curIndex];
        }
    }
}
