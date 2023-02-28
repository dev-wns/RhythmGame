using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class BpmChanger : MonoBehaviour
{
    private InGame scene;
    private ReadOnlyCollection<Timing> timings;
    private int curIndex;
    private Timing curTiming;
    
    [Header("Sprite")]
    public int sortingOrder;

    [Header("BPM Changer")]
    public List<Sprite> sprites = new List<Sprite>();
    private List<SpriteRenderer> images = new List<SpriteRenderer>();
    private CustomHorizontalLayoutGroup layoutGroup;
    private int prevNum, curNum;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>(); 
        scene.OnSystemInitialize += Initialize;
        scene.OnGameStart += StartProcess;
        scene.OnReLoad += ReLoad;

        layoutGroup = GetComponent<CustomHorizontalLayoutGroup>();

        images.AddRange( GetComponentsInChildren<SpriteRenderer>( true ) );
        images.Reverse();

        for ( int i = 0; i < images.Count; i++ )
              images[i].sortingOrder = sortingOrder;
    }

    private void ReLoad()
    {
        StopAllCoroutines();
        prevNum = curNum = 0;
        curIndex = 0;
        curTiming = new Timing();

        images[0].gameObject.SetActive( true );
        images[0].sprite = sprites[0];
        for ( int i = 1; i < images.Count; i++ )
        {
            images[i].gameObject.SetActive( false );
        }
        layoutGroup.SetLayoutHorizontal();
    }

    private void Initialize( Chart _chart )
    {
        timings = _chart.timings;
    }

    private void StartProcess() => StartCoroutine( Process() );

    private IEnumerator Process()
    {
        float minSPF = 1f / 60f;
        WaitUntil waitChangedTimeUntil = new WaitUntil( () => curTiming.time <= NowPlaying.Playback );
        while ( curIndex < timings.Count )
        {
            curTiming = timings[curIndex];
            var bpm = Math.Round( curTiming.bpm );
            yield return waitChangedTimeUntil;

            double calcCurBpm = bpm;
            curNum = Global.Math.Log10( bpm ) + 1;
            for ( int i = 0; i < images.Count; i++ )
            {
                if ( i < curNum )
                {
                    if ( !images[i].gameObject.activeSelf )
                         images[i].gameObject.SetActive( true );

                    images[i].sprite = sprites[( int )calcCurBpm % 10];
                    calcCurBpm *= .1f;
                }
                else
                {
                    if ( images[i].gameObject.activeSelf )
                         images[i].gameObject.SetActive( false );
                }
            }

            if ( prevNum != curNum )
                layoutGroup.SetLayoutHorizontal();

            if ( curIndex + 1 < timings.Count && curTiming.bpm > 1000d &&
                 timings[curIndex + 1].time - curTiming.time < minSPF )
                 yield return YieldCache.WaitForSeconds( minSPF );
                 
            prevNum = curNum;
            curIndex++;
        }
    }
}
