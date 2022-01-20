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
    private int currentIndex;
    // private float changeTime;
    // private float currentBpm;
    private Timing curTiming;

    public event Action<float/* bpm */> OnBpmChange;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>(); 
        scene.OnSystemInitialize += Initialize;
        scene.OnGameStart += StartProcess;
    }

    private void Initialize( in Chart _chart )
    {
        timings = _chart.timings;
    }

    private void StartProcess() => StartCoroutine( Process() );

    private IEnumerator Process()
    {
        WaitUntil waitChangedTimeUntil = new WaitUntil( () => curTiming.time <= NowPlaying.Playback );
        while ( currentIndex < timings.Count )
        {
            curTiming = timings[currentIndex];
            yield return waitChangedTimeUntil;
            
            Debug.Log( $"Current Bpm : {curTiming.bpm}" );
            OnBpmChange?.Invoke( curTiming.bpm );
            currentIndex++;
        }
    }
}
