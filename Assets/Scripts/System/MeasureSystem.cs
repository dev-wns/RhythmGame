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
                NowPlaying.OnSpawnObjects += SpawnMeasures;
                scene.OnReLoad += ReLoad;
            }
        }

        pool = new ObjectPool<MeasureRenderer>( mPrefab, 10 );
    }

    private void OnDestroy()
    {
        NowPlaying.OnSpawnObjects -= SpawnMeasures;
        //StopAllCoroutines();
    }

    private void ReLoad()
    {
        curIndex = 0;
        curTime = measures[curIndex];
        pool.AllDespawn();
    }

    private void Initialize( Chart _chart )
    {
        var timings   = _chart.timings;
        var totalTime = NowPlaying.CurrentSong.totalTime * .001d;

        for ( int i = 0; i < timings.Count; i++ )
        {
            if ( timings[i].isUninherited == 0 )
                 continue;

            double time      = timings[i].time;
            double nextTime  = 0d;
            bool hasNextTime = false;
            for ( int j = i + 1; j < timings.Count; j++ )
            {
                if ( timings[j].isUninherited == 1 )
                {
                    hasNextTime = true;
                    nextTime = timings[j].time;
                    break;
                }
            }

            if ( !hasNextTime )
                 nextTime = ( double )( totalTime + 60d );

            double spb = ( 60d / timings[i].bpm ) * Beat; // 4�ڿ� 1�� ���� ( 60BPM�϶� 4�ʸ��� 1�� ���� )
            while ( time < nextTime )
            {
                measures.Add( NowPlaying.Inst.GetDistance( time ) );
                time += spb;
            }
        }

        if ( measures.Count > 0 )
             curTime = measures[curIndex];
    }

    private void SpawnMeasures( double _distance )
    {
        while ( curIndex < measures.Count && curTime <= _distance + GameSetting.MinDistance )
        {
            MeasureRenderer measure = pool.Spawn();
            measure.SetInfo( curTime );

            if ( ++curIndex < measures.Count )
                curTime = measures[curIndex];
        }
    }
}
