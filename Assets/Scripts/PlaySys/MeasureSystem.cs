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
    private int currentIndex;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();

        mPool = new ObjectPool<MeasureRenderer>( mPrefab, 5 );

        scene.OnGameStart += () => StartCoroutine( Process() );
    }

    public void AddTime( float _time ) => measures.Add( _time );

    private IEnumerator Process()
    {
        while ( currentIndex < measures.Count - 1 )
        {
            float curTime = measures[currentIndex];
            yield return new WaitUntil( () => curTime <= NowPlaying.PlaybackChanged + GameSetting.PreLoadTime );

            MeasureRenderer measure = mPool.Spawn();
            measure.Initialized( curTime );
            currentIndex++;
        }
    }
}
