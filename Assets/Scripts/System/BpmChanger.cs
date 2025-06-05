using System.Collections.ObjectModel;
using TMPro;
using UnityEngine;

public class BpmChanger : MonoBehaviour
{
    private InGame scene;
    private int curIndex;

    [Header("BPM Changer")]

    [Header("Time")]
    private const double DelayTime = ( 1d / 60d ) * 1000d;
    private Timing curTiming;
    private bool isStart;
    private double time;

    public TextMeshProUGUI text;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnSystemInitialize += Initialize;
        scene.OnReLoad           += Initialize;
    }

    private void Initialize()
    {
        curIndex = 0;
        curTiming = NowPlaying.CurrentChart.timings[curIndex];
        time = curTiming.time;
        text.text = $"{( int )curTiming.bpm}";
        isStart = true;
    }

    private void LateUpdate()
    {
        if ( isStart && curIndex < NowPlaying.CurrentChart.timings.Count &&
             time < NowPlaying.Playback )
        {
            text.text = $"{( int )curTiming.bpm}";

            if ( ++curIndex < NowPlaying.CurrentChart.timings.Count )
            {
                curTiming = NowPlaying.CurrentChart.timings[curIndex];
                time = curIndex + 1 < NowPlaying.CurrentChart.timings.Count && Global.Math.Abs( NowPlaying.CurrentChart.timings[curIndex + 1].time - curTiming.time ) > DelayTime ?
                       curTiming.time + DelayTime : curTiming.time;
            }
        }
    }
}
