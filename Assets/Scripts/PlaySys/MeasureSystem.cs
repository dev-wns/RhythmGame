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
    private int currentIndex;
    private double currentTime;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnSystemInitialize += Initialize;
        scene.OnGameStart += () => StartCoroutine( Process() );
        mPool = new ObjectPool<MeasureRenderer>( mPrefab, 5 );
    }

    public void Despawn( MeasureRenderer _obj ) => mPool.Despawn( _obj );

    private void Initialize( in Chart _chart )
    {
        int noteIndex = 0;
        var timings = _chart.timings;
        for ( int i = 0; i < timings.Count; i++ )
        {
            if ( timings[i].bpm < 10 ) continue;
            double bpms = ( 60d / timings[i].bpm ) * 1000d; // beat per milliseconds

            double time = timings[i].time;
            if ( i == 0 )
            {
                int maxBeat = Mathf.FloorToInt( ( float )( ( _chart.notes[0].time + 3000d ) / bpms ) );
                time = _chart.notes[0].time - ( bpms * maxBeat );
            }
            else
            {
                for ( int j = noteIndex; j < _chart.notes.Count; noteIndex = ++j )
                {
                    if ( timings[i].time < _chart.notes[j].time )
                    {
                        time = _chart.notes[j].time;
                        break;
                    }
                }
            }

            double nextTime = ( i + 1 == timings.Count ) ? _chart.notes[_chart.notes.Count - 1].time + 3000d : timings[i + 1].time;
            double calcTime = NowPlaying.Inst.GetChangedTime( time );
            if ( measures.Count == 0 || measures[measures.Count - 1] < calcTime )
                 measures.Add( calcTime );

            int maxCount = Mathf.FloorToInt( ( float )( ( nextTime - time ) / bpms ) );
            for ( int j = 1; j < maxCount + 1; j++ )
            {
                measures.Add( NowPlaying.Inst.GetChangedTime( ( time + ( j * bpms ) ) ) );
            }
        }
    }

    private IEnumerator Process()
    {
        if ( measures.Count > 0 )
             currentTime = measures[currentIndex];

        WaitUntil waitNextMeasure = new WaitUntil( () => currentTime <= NowPlaying.PlaybackChanged + GameSetting.PreLoadTime );

        while ( currentIndex < measures.Count )
        {
            yield return waitNextMeasure;

            MeasureRenderer measure = mPool.Spawn();
            measure.SetInfo( this, currentTime );

            if ( ++currentIndex < measures.Count )
                 currentTime = measures[currentIndex];
            
        }
    }
}
