using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class BpmChanger : MonoBehaviour
{
    private InGame scene;
    private List<Timing> timings = new List<Timing>();
    private int currentIndex;
    private float changeTime;
    private float currentBpm;

    public event Action<float/* bpm */> OnBpmChange;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>(); 
        scene.OnSystemInitialize += Initialize;
        scene.OnGameStart += StartProcess;
    }

    private void Initialize( in Chart _chart )
    {
        timings = _chart.timings.ToList();
    }

    private void StartProcess() => StartCoroutine( Process() );

    private IEnumerator Process()
    {
        if ( timings.Count > 0 )
             changeTime = timings[currentIndex].time;

        while ( currentIndex < timings.Count )
        {
            if ( changeTime <= NowPlaying.Playback )
            {
                currentBpm = timings[currentIndex].bpm;
                OnBpmChange?.Invoke( currentBpm );

                if ( ++currentIndex < timings.Count )
                    changeTime = timings[currentIndex].time;
            }

            yield return null;
        }
    }
}
