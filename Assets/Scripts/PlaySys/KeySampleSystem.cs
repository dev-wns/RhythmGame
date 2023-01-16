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
    private double offset;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnGameStart += GameStart;
        scene.OnReLoad += ReLoad;
    }

    private void ReLoad()
    {
        StopAllCoroutines();
        curIndex = 0;
        curTime  = 0d;
        offset   = 0d;
    }

    private void GameStart()
    {
        offset = NowPlaying.CurrentSong.isOnlyKeySound && GameSetting.CurrentGameMode.HasFlag( GameMode.AutoPlay ) ? 0d : GameSetting.SoundOffset * .001d;

        StartCoroutine( Process() );
    }

    public void SortSamples()
    {
        samples.Sort( delegate ( KeySound _A, KeySound _B )
        {
            if ( _A.time > _B.time )      return 1;
            else if ( _A.time < _B.time ) return -1;
            else                          return 0;

        } );
    }

    public void AddSample( in KeySound _sample )
    {
        samples.Add( _sample );
    }

    private IEnumerator FirstSync()
    {
        double time = samples[0].time + offset;
        yield return new WaitUntil( () => time < NowPlaying.Playback );

        NowPlaying.Inst.SoundSynchronized( time );
    }

    private IEnumerator Process()
    {
        if ( samples.Count > 0 )
             curTime = samples[curIndex].time;

        WaitUntil waitNextSample = new WaitUntil( () => curTime + offset < NowPlaying.Playback );
        StartCoroutine( FirstSync() );

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
                if ( Global.Math.Abs( curTime - samples[curIndex].time ) < double.Epsilon )
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
