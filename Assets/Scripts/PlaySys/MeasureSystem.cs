using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MeasureSystem : MonoBehaviour
{
    private InGame scene;

    public ObjectPool<MeasureRenderer> mPool;
    public MeasureRenderer mPrefab;
    // 60bpm은 분당 1/4박자 60개, 스크롤 속도가 1일때 한박자(1/4) 시간은 1초
    public List<float> measures = new List<float>();
    private int curIdx;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        mPool = new ObjectPool<MeasureRenderer>( mPrefab, 5 );

        scene.SystemInitialized += Initialized;
        scene.StartGame += () => StartCoroutine( Process() );
    }

    private void Initialized( Chart _chart )
    {
        var timings = _chart.timings;
        for ( int i = 0; i < timings.Count; i++ )
        {
            float time;
            Timing timing = timings[i];

            if ( timing.bpm < 60 || timing.bpm > 999 ) continue;
            float bpm = ( timing.bpm / 60f ) * 1000f; // beat per milliseconds

            if ( i + 1 == timings.Count ) time = _chart.notes[_chart.notes.Count - 1].time;
            else time = timings[i + 1].time;

            int a = Mathf.FloorToInt( ( time - timing.time ) / bpm );
            measures.Add( InGame.GetChangedTime( timing.time, _chart ) );

            for ( int j = 0; j < a; j++ )
            {
                measures.Add( InGame.GetChangedTime( timing.time + ( j * bpm ), _chart ) );
            }
        }
    }

    private IEnumerator Process()
    {
        while ( curIdx < measures.Count - 1 )
        {
            float curTime = measures[curIdx];
            yield return new WaitUntil( () => curTime <= InGame.PlaybackChanged + InGame.PreLoadTime );

            MeasureRenderer measure = mPool.Spawn();
            measure.Initialized( curTime );
            curIdx++;
        }
    }
}
