using System.Collections.Generic;
using UnityEngine;

public class MeasureSystem : MonoBehaviour
{
    public ObjectPool<MeasureRenderer> pool;
    public MeasureRenderer mPrefab;
    private List<double/* Distance */> measures = new List<double>();
    private int curIndex;
    private double distance;
    private static readonly int Beat = 4;

    private void Awake()
    {
        InGame scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        if ( GameSetting.HasFlag( VisualFlag.ShowMeasure ) )
        {
            NowPlaying.OnPostInitAsync += CreateMeasure;
            NowPlaying.OnPreUpdate     += SpawnMeasure;
            scene.OnReLoad += OnReLoad;
        }

        pool = new ObjectPool<MeasureRenderer>( mPrefab, 10 );
    }

    private void OnDestroy()
    {
        if ( GameSetting.HasFlag( VisualFlag.ShowMeasure ) )
        {
            NowPlaying.OnPostInitAsync -= CreateMeasure;
            NowPlaying.OnPreUpdate     -= SpawnMeasure;
        }
    }

    private void CreateMeasure()
    {
        var timings          = DataStorage.Timings;
        var totalTime        = NowPlaying.CurrentSong.totalTime;
        double startNoteTime = DataStorage.Notes[0].time;
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
                 nextTime = ( double )( totalTime + 60000d );

            double spb = ( 60d / timings[i].bpm ) * Beat * 1000d; // 4박에 1개 생성 ( 60BPM일때 4초마다 1개 생성 )
            while ( time < nextTime )
            {
                measures.Add( NowPlaying.Inst.GetDistance( time ) );
                time += spb;
            }
        }

        if ( measures.Count > 0 )
             distance = measures[curIndex];
    }

    private void SpawnMeasure()
    {
        if ( curIndex >= measures.Count )
             return;

        if ( distance <= NowPlaying.Distance + GameSetting.MinDistance )
        {
            MeasureRenderer measure = pool.Spawn();
            measure.SetInfo( distance );

            if ( ++curIndex < measures.Count )
                 distance = measures[curIndex];
        }
    }

    private void OnReLoad()
    {
        curIndex = 0;
        distance = measures[curIndex];
        pool.AllDespawn();
    }

}
