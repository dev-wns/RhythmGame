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
    // 60bpm은 분당 1/4박자 60개, 스크롤 속도가 1일때 한박자(1/4) 시간은 1초
    private List<float> measures = new List<float>();
    private int currentIndex;
    private float currentTime;

    private float playback, bpms;

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
            float bpms = ( 60f / timings[i].bpm ) * 1000f; // beat per milliseconds

            float time = timings[i].time;
            if ( i == 0 )
            {
                int maxBeat = Mathf.FloorToInt( ( _chart.notes[0].time + 3000f ) / bpms );
                time = _chart.notes[0].time - ( maxBeat * bpms );
            }
            else
            {
                for ( int j = noteIndex; j < _chart.notes.Count; noteIndex = ++j )
                {
                    if ( timings[i].time < _chart.notes[j].time )
                    {
                        //float cur = timings[i].time - _chart.notes[j].time;
                        //float prev = j - 1 >= 0 ?
                        //              timings[i].time - _chart.notes[j - 1].time :
                        //              timings[i].time - _chart.notes[j].time;

                        //float curAbs = cur >= 0 ? cur : -cur;
                        //float prevAbs = prev >= 0 ? prev : -prev;

                        //if ( curAbs != prevAbs )
                        //    Debug.Log( $"{curAbs} {prevAbs}" );

                        //time = curAbs > prevAbs ? _chart.notes[j - 1].time : _chart.notes[j].time;

                        time = _chart.notes[j].time;
                        break;
                    }
                }
            }

            float nextTime = 0f;
            if ( i + 1 == timings.Count ) nextTime = _chart.notes[_chart.notes.Count - 1].time + 3000f;
            else                          nextTime = timings[i + 1].time;

            float calcTime = NowPlaying.GetChangedTime( time );
            if ( measures.Count == 0 || measures[measures.Count - 1] < calcTime )
                 measures.Add( calcTime );

            int maxCount = Mathf.FloorToInt( ( nextTime - time ) / bpms );
            for ( int j = 1; j < maxCount + 1; j++ )
            {
                measures.Add( NowPlaying.GetChangedTime( ( time + ( j * bpms ) ) ) );
            }
        }

        for ( int k = 1; k < measures.Count; k++ )
        {
            if ( measures[k - 1] == measures[k] )
                Debug.Log( "duplicate" );
        }
    }

    private IEnumerator Process()
    {
        if ( measures.Count > 0 )
             currentTime = measures[currentIndex];

        while ( currentIndex < measures.Count )
        {
            if ( currentTime <= NowPlaying.PlaybackChanged + GameSetting.PreLoadTime )
            {
                MeasureRenderer measure = mPool.Spawn();
                measure.SetInfo( this, currentTime );

                if ( ++currentIndex < measures.Count )
                     currentTime = measures[currentIndex];
            }

            yield return null;
        }
    }
}
