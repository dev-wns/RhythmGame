using System.Collections.ObjectModel;
using TMPro;
using UnityEngine;

public class BpmChanger : MonoBehaviour
{
    private InGame scene;
    private int curIndex;
    private ReadOnlyCollection<Timing> timings;
    [Header("BPM Changer")]

    [Header("Time")]
    private const double DelayTime = ( 1d / 60d ) * 1000d;
    private Timing curTiming;
    private bool isStart;
    private double time;

    public TextMeshProUGUI text;

    private void Awake()
    {
        NowPlaying.OnPreInit += Initialize;
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnReLoad              += Initialize;
    }

    private void OnDestroy()
    {
        NowPlaying.OnPreInit -= Initialize;
    }

    private void Initialize()
    {
        timings   = DataStorage.Timings;
        curIndex  = 0;
        curTiming = timings[curIndex];
        time      = curTiming.time;
        text.text = $"{( int )curTiming.bpm}";
        isStart   = true;
    }

    private void LateUpdate()
    {
        if ( isStart && curIndex < timings.Count &&
             time < NowPlaying.Playback )
        {
            text.text = $"{( int )curTiming.bpm}";

            if ( ++curIndex < timings.Count )
            {
                curTiming = timings[curIndex];
                time = curIndex + 1 < timings.Count && Global.Math.Abs( timings[curIndex + 1].time - curTiming.time ) > DelayTime ?
                       curTiming.time + DelayTime : curTiming.time;
            }
        }
    }
}
