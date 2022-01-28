using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections.ObjectModel;
using TMPro;

public class BpmChanger : MonoBehaviour
{
    private InGame scene;
    private ReadOnlyCollection<Timing> timings;
    private int curIndex;
    private Timing curTiming;

    public event Action<double/* bpm */> OnBpmChange;

    public List<Sprite> sprites = new List<Sprite>();
    private List<SpriteRenderer> images = new List<SpriteRenderer>();
    private CustomHorizontalLayoutGroup layoutGroup;
    private int prevNum, curNum;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>(); 
        scene.OnSystemInitialize += Initialize;
        scene.OnGameStart += StartProcess;

        layoutGroup = GetComponent<CustomHorizontalLayoutGroup>();

        images.AddRange( GetComponentsInChildren<SpriteRenderer>( true ) );
        images.Reverse();
    }

    private void Initialize( in Chart _chart )
    {
        timings = _chart.timings;
    }

    private void StartProcess() => StartCoroutine( Process() );

    private IEnumerator Process()
    {
        WaitUntil waitChangedTimeUntil = new WaitUntil( () => curTiming.time <= NowPlaying.Playback );
        while ( curIndex < timings.Count )
        {
            curTiming = timings[curIndex];
            yield return waitChangedTimeUntil;
            
            OnBpmChange?.Invoke( curTiming.bpm );

            double calcCurBpm = curTiming.bpm;
            curNum = Globals.Log10( curTiming.bpm ) + 1;
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

            curIndex++;
        }
    }
}
