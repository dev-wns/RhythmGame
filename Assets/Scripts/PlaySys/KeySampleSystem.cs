using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class KeySampleSystem : MonoBehaviour
{
    private InGame scene;
    private ReadOnlyCollection<KeySample> samples;
    private int curIndex;
    private double curTime;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnSystemInitialize += Initialize;
        scene.OnGameStart += () => StartCoroutine( Process() );
    }

    private void Initialize( in Chart _chart )
    {
        samples = _chart.samples;
    }

    private IEnumerator Process()
    {
        if ( samples.Count > 0 )
            curTime = samples[curIndex].time;

        WaitUntil waitNextSample = new WaitUntil( () => curTime <= NowPlaying.Playback );
        while ( curIndex < samples.Count )
        {
            yield return waitNextSample;

            SoundManager.Inst.PlayKeySound( 6, samples[curIndex].sound );
            Debug.Log( $"Sample Play {samples[curIndex].sound.name}" );

            if ( ++curIndex < samples.Count )
                 curTime = samples[curIndex].time;

        }
    }
}
