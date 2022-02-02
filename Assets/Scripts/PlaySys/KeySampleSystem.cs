using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class KeySampleSystem : MonoBehaviour
{
    private InGame scene;
    private List<KeySample> samples = new List<KeySample>();
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
        for( int i = 0; i < _chart.samples.Count; i++ )
        {
            var newSample = _chart.samples[i];
            newSample.sound.key = SoundManager.Inst.GetSampleKey( newSample.sound.name );
            samples.Add( newSample );
        }
    }

    private IEnumerator Process()
    {
        if ( samples.Count > 0 )
            curTime = samples[curIndex].time;

        WaitUntil waitNextSample = new WaitUntil( () => curTime <= NowPlaying.Playback );
        while ( curIndex < samples.Count )
        {
            yield return waitNextSample;

            SoundManager.Inst.Play( samples[curIndex].sound );

            if ( ++curIndex < samples.Count )
                 curTime = samples[curIndex].time;
        }
    }
}
