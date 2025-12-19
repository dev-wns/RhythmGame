using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasureSystem : MonoBehaviour
{
    public ObjectPool<MeasureRenderer> pool;
    public MeasureRenderer mPrefab;
    private List<double/* Distance */> measures = new List<double>();
    private static readonly int Beat = 4;

    private void Awake()
    {
        if ( GameSetting.HasFlag( VisualFlag.ShowMeasure ) )
        {
            NowPlaying.OnLoadAsync += CreateMeasure;
            NowPlaying.OnGameStart += GameStart;
            NowPlaying.OnClear     += Clear;
        }

        pool = new ObjectPool<MeasureRenderer>( mPrefab, 10 );
    }

    private void OnDestroy()
    {
        if ( GameSetting.HasFlag( VisualFlag.ShowMeasure ) )
        {
            NowPlaying.OnLoadAsync -= CreateMeasure;
            NowPlaying.OnGameStart -= GameStart;
            NowPlaying.OnClear     -= Clear;
        }
    }

    private void Clear()
    {
        StopAllCoroutines();
        pool.AllDespawn();
    }

    private void GameStart() => StartCoroutine( SpawnMeasure() );

    private IEnumerator SpawnMeasure()
    {
        int    curIndex = 0;
        double distance = measures[curIndex];
        while ( curIndex < measures.Count )
        {
            yield return new WaitUntil( () => distance <= NowPlaying.Distance + GameSetting.MinDistance );
            MeasureRenderer measure = pool.Spawn();
            measure.SetInfo( distance );

            if ( ++curIndex < measures.Count )
                 distance = measures[curIndex];
        }
    }

    private void CreateMeasure()
    {
        var    timings       = DataStorage.Timings;
        var    totalTime     = NowPlaying.CurrentSong.totalTime;
        //double startNoteTime = DataStorage.Notes[0].time;
        for ( int i = 0; i < timings.Count; i++ )
        {
            // 마디선 계산은 상속된 타이밍만
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

            // 1마디( 4박자 ) | 1박자( 60 / bpm ) | 1/4박자( ( 60d / bpm ) / 4 )
            // |-----|-----|-----|-----| 1마디( 4박자 )
            //    ↓  
            // |-----|-----|-----|-----| 1박자( 16분음표 )
            // 16분음표( 1/4박자 ), 32분음표( 1/8박자 ), 64분음표( 1/16박자 )으로 분할가능

            double mspb = ( 60000d / timings[i].bpm ) * Beat; // millisecond per beat
            while ( time < nextTime )
            {
                measures.Add( NowPlaying.Inst.GetDistance( time + GameSetting.ScreenOffset ) );
                time += mspb;
            }
        }
    }
}
