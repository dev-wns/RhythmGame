using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// KeySound that plays unconditionally.
/// </summary>
public class KeySampleSystem : MonoBehaviour
{
    public static bool UseAllSamples { get; private set; }

    private InGame scene;
    private List<KeySound> samples = new List<KeySound>();
    private int curIndex;
    private double curTime;
    //private double offset;
    private bool isStart;

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
        //offset   = 0d;
        UseAllSamples = false;
    }

    private void GameStart()
    {
        UseAllSamples = false;

        //offset = NowPlaying.CurrentSong.audioPath == string.Empty || NowPlaying.CurrentSong.isOnlyKeySound ? 0d : ( GameSetting.SoundOffset - 50 ) * .001d;

        isStart = true;
        if ( samples.Count > 0 )
             curTime = samples[curIndex].time;
        // StartCoroutine( Process() );
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

    private void LateUpdate()
    {
        while ( isStart && curIndex < samples.Count &&
                samples[curIndex].time + ( GameSetting.SoundOffset * .001d ) < NowPlaying.Playback )
        {
            SoundManager.Inst.Play( samples[curIndex++] );

            if ( curIndex < samples.Count )
                 UseAllSamples = true;
            //while ( curIndex + 1 < samples.Count && 
            //        Global.Math.Abs( samples[curIndex + 1].time - samples[curIndex].time ) < double.Epsilon )
            //{
            //    SoundManager.Inst.Play( samples[++curIndex] );
            //}

            //++curIndex;
            //curTime = samples[++curIndex].time;
        }
    }

    //private IEnumerator Process()
    //{
    //    if ( samples.Count > 0 )
    //         curTime = samples[curIndex].time;

    //    WaitUntil waitNextSample = new WaitUntil( () => curTime + offset < NowPlaying.Playback );

    //    while ( curIndex < samples.Count )
    //    {
    //        yield return waitNextSample;
    //        // �� ������ �� ���� ���
    //        //SoundManager.Inst.Play( samples[curIndex].sound );
    //        //if ( ++curIndex < samples.Count )
    //        //     curTime = samples[curIndex].time;

    //        // ���� �ð� ���� ���
    //        while ( curIndex < samples.Count )
    //        {
    //            SoundManager.Inst.Play( samples[curIndex] );
    //            if ( Global.Math.Abs( curTime - samples[curIndex].time ) < double.Epsilon )
    //            {
    //                curIndex += 1;
    //            }
    //            else
    //            {
    //                curTime = samples[curIndex].time;
    //                break;
    //            }
    //        }
    //    }
    //}
}
