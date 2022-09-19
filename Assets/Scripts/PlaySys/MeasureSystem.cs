using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MeasureSystem : MonoBehaviour
{
    private InGame scene;
    public BpmChanger bpmChanger;

    public ObjectPool<MeasureRenderer> mPool;
    public MeasureRenderer mPrefab;
    private List<double /* JudgeLine hit time */> measures = new List<double>();
    private int curIndex;
    private double curTime;
    private double loadTime;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        if ( ( GameSetting.CurrentVisualFlag & GameVisualFlag.ShowMeasure ) != 0 )
        {
            scene.OnSystemInitialize += Initialize;
            scene.OnReLoad += ReLoad;
            scene.OnGameStart += () => StartCoroutine( Process() );
            scene.OnScrollChanged += ScrollUpdate;
        }

        mPool = new ObjectPool<MeasureRenderer>( mPrefab, 5 );

        ScrollUpdate();
    }

    private void ReLoad()
    {
        StopAllCoroutines();
        curIndex = 0;
        curTime = 0d;
    }

    private void OnDestroy() => scene.OnScrollChanged -= ScrollUpdate;

    private void ScrollUpdate() => loadTime = GameSetting.PreLoadTime;

    public void Despawn( MeasureRenderer _obj ) => mPool.Despawn( _obj );

    private void Initialize( in Chart _chart )
    {
        // 0초 ~ 첫 노트시간까지 마디선 생성
        {
            double spb   = ( 60d / NowPlaying.Inst.CurrentSong.medianBpm ) * 4;
            double time  = _chart.notes[0].time;
            int maxCount = ( int )( time / spb );
            for ( int j = maxCount; j > 0; j-- )
                measures.Add( NowPlaying.Inst.GetChangedTime( time - ( j * spb ) ) );
        }

        // 첫 노트 ~ 음악 끝시간까지 마디선 생성
        {
            double spb   = ( 60d / NowPlaying.Inst.CurrentSong.medianBpm ) * 4; // 4박에 1개 생성 ( 60BPM일때 4초마다 1개 생성 )
            double time  = _chart.notes[0].time;
            int maxCount = ( int )( ( ( double )( NowPlaying.Inst.CurrentSong.totalTime * 0.001d ) - time ) / spb );
            for ( int j = 0; j < maxCount; j++ )
            {
                measures.Add( NowPlaying.Inst.GetChangedTime( time + ( j * spb ) ) );
            }
        }

        //// 0초 ~ 첫 노트시간까지 마디선 생성
        //var totalTime = NowPlaying.Inst.CurrentSong.totalTime;
        //var timings   = _chart.timings;
        //{
        //    double spb  = ( 60d / timings[0].bpm ) * 4;
        //    double time = _chart.notes[0].time;

        //    int maxCount = ( int )( time / spb );
        //    for ( int j = maxCount; j > 0; j-- )
        //        measures.Add( NowPlaying.Inst.GetChangedTime( time - ( j * spb ) ) );
        //}
        //// 첫 노트 ~ 음악 끝시간까지 마디선 생성
        //for ( int i = 0; i < timings.Count; i++ )
        //{
        //    if ( timings[i].bpm < 1 )
        //        continue;

        //    double spb      = ( 60d / timings[i].bpm ) * 4; // 4박에 1개 생성 ( 60BPM일때 4초마다 1개 생성 )
        //    double nextTime = ( i + 1 == timings.Count ) ? ( double )( totalTime * 0.001d ) : timings[i + 1].time;
        //    double time     = ( i == 0 ) ? _chart.notes[0].time : timings[i].time;

        //    int maxCount = ( int )( ( nextTime - time ) / spb );
        //    if ( maxCount == 0 ) measures.Add( NowPlaying.Inst.GetChangedTime( time ) );
        //    else
        //    {
        //        for ( int j = 0; j < maxCount; j++ )
        //        {
        //            measures.Add( NowPlaying.Inst.GetChangedTime( ( time + ( j * spb ) ) ) );
        //        }
        //    }
        //}
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
            measure.SetInfo( this, curTime );

            if ( ++curIndex < measures.Count )
                 curTime = measures[curIndex];
        }
    }
}
