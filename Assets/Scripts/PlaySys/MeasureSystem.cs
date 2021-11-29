using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MeasureSystem : MonoBehaviour
{
    public static ObjectPool<Measure> mPool;
    public Measure mPrefab;

    private float curTiming;
    public Queue<float> timings;

    private void Awake()
    {
        mPool = new ObjectPool<Measure>( mPrefab, 5 );
        timings = new Queue<float>();

        InGame.SystemsInitialized += Initialized;
    }

    private void Initialized()
    {
        if ( timings.Count == 0 )
        {
            Debug.Log( "Measure System Initialize Fail " );
            return;
        }

        curTiming = timings.Dequeue();
        StartCoroutine( Process() );
    }

    private IEnumerator Process()
    {
        yield return new WaitUntil( () => curTiming <= NowPlaying.PlaybackChanged + NowPlaying.PreLoadTime );
        
        Measure measure = mPool.Spawn();
        measure.Initialized( curTiming );

        if ( timings.Count > 0 )
        {
            curTiming = timings.Dequeue();
            StartCoroutine( Process() );
        }
    }
}
