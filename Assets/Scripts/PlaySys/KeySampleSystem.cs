using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;


/// <summary>
/// KeySound that plays unconditionally.
/// </summary>
public class KeySampleSystem : MonoBehaviour
{
    private InGame scene;
    private List<KeySound> samples = new List<KeySound>();
    private int curIndex;
    private double curTime;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnGameStart += () => StartCoroutine( Process() );
        scene.OnReLoad += ReLoad;
    }

    private void ReLoad()
    {
        StopAllCoroutines();
        curIndex = 0;
        curTime = 0f;
    }

    public void AddSample( in KeySound _sample )
    {
        samples.Add( _sample );
    }

    private IEnumerator Process()
    {
        if ( samples.Count > 0 )
             curTime = samples[curIndex].time;

        WaitUntil waitNextSample = new WaitUntil( () => curTime <= NowPlaying.Playback );
        
        while ( curIndex < samples.Count )
        {
            yield return waitNextSample;

            // 한 프레임 한 샘플 재생
            //SoundManager.Inst.Play( samples[curIndex].sound );
            //if ( ++curIndex < samples.Count )
            //     curTime = samples[curIndex].time;

            // 같은 시간 동시 재생
            while ( curIndex < samples.Count )
            {
                if ( curTime == samples[curIndex].time )
                {
                    SoundManager.Inst.Play( samples[curIndex++] );
                }
                else
                {
                    curTime = samples[curIndex].time;
                    break;
                }
            }
        }
    }
}
