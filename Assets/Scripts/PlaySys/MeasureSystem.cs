using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MeasureSystem : MonoBehaviour
{
    private InGame game;

    public ObjectPool<MeasureRenderer> mPool;
    public MeasureRenderer mPrefab;
    // 60bpm은 분당 1/4박자 60개, 스크롤 속도가 1일때 한박자(1/4) 시간은 1초
    public List<float> measures = new List<float>();
    private int currentIndex;

    private void Awake()
    {
        var scene = GameObject.FindGameObjectWithTag( "Scene" );
        game = scene.GetComponent<InGame>();

        mPool = new ObjectPool<MeasureRenderer>( mPrefab, 5 );

        game.OnGameStart += () => StartCoroutine( Process() );
    }

    public void AddTime( float _time ) => measures.Add( _time );

    private void Initialized( Chart _chart )
    {
        //var timings = _chart.timings;
        //for ( int i = 0; i < timings.Count; i++ )
        //{
        //    float time;
        //    Timing timing = timings[i];

        //    if ( timing.bpm < 10 ) continue;
        //    float bpms = ( timing.bpm / 60f ) * 1000f / 4; // beat per milliseconds

        //    if ( i + 1 == timings.Count ) time = _chart.notes[_chart.notes.Count - 1].time;
        //    else time = timings[i + 1].time;

        //    int a = Mathf.FloorToInt( ( time - timing.time ) / bpms );
        //    measures.Add( InGame.GetChangedTime( timing.time, _chart ) );

        //    for ( int j = 1; j < a + 1; j++ )
        //    {
        //        measures.Add( InGame.GetChangedTime( timing.time + ( j * bpms ), _chart ) );
        //    }
        //}
    }

    private IEnumerator Process()
    {
        while ( currentIndex < measures.Count - 1 )
        {
            float curTime = measures[currentIndex];
            yield return new WaitUntil( () => curTime <= InGame.PlaybackChanged + InGame.PreLoadTime );

            MeasureRenderer measure = mPool.Spawn();
            measure.Initialized( curTime );
            currentIndex++;
        }
    }
}
