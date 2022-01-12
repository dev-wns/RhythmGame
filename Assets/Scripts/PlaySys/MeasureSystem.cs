using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MeasureSystem : MonoBehaviour
{
    private InGame scene;

    public ObjectPool<MeasureRenderer> mPool;
    public MeasureRenderer mPrefab;
    // 60bpm은 분당 1/4박자 60개, 스크롤 속도가 1일때 한박자(1/4) 시간은 1초
    private List<float> measures = new List<float>();
    private int currentIndex;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();

        mPool = new ObjectPool<MeasureRenderer>( mPrefab, 5, false );

        scene.OnSystemInitialize += Initialize;
        scene.OnGameStart += () => StartCoroutine( Process() );
    }

    public void Despawn( MeasureRenderer _obj ) => mPool.Despawn( _obj );

    private void Initialize( Chart _chart )
    {
        var timings = _chart.timings;
        for ( int i = 0; i < timings.Count; i++ )
        {
            float time;
            Timing timing = timings[i];

            if ( timing.bpm < 10 ) continue;
            float bpms = ( timing.bpm / 60f ) * 1000f / 4; // beat per milliseconds

            if ( i + 1 == timings.Count ) time = _chart.notes[_chart.notes.Count - 1].time;
            else time = timings[i + 1].time;

            int maxCount = Mathf.FloorToInt( ( time - timing.time ) / bpms );
            measures.Add( NowPlaying.GetChangedTime( timing.time ) );

            for ( int j = 1; j < maxCount + 1; j++ )
            {
                measures.Add( NowPlaying.GetChangedTime( timing.time + ( j * bpms ) ) );
            }
        }
    }

    private IEnumerator Process()
    {
        while ( currentIndex < measures.Count - 1 )
        {
            float curTime = measures[currentIndex];
            yield return new WaitUntil( () => curTime <= NowPlaying.PlaybackChanged + GameSetting.PreLoadTime );

            MeasureRenderer measure = mPool.Spawn();
            measure.SetInfo( this, curTime );

            currentIndex++;
        }
    }
}
