using System.Collections.ObjectModel;
using TMPro;
using UnityEngine;

public class BpmChanger : MonoBehaviour
{
    private InGame scene;
    private ReadOnlyCollection<Timing> timings;
    private int curIndex;

    [Header("BPM Changer")]

    [Header("Time")]
    private const double DelayTime = 1d / 60d;
    private Timing curTiming;
    private bool isStart;
    private double time;

    public TextMeshProUGUI text;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnSystemInitialize += Initialize;
        scene.OnReLoad += OnReLoad;
    }

    private void OnReLoad()
    {
        StopAllCoroutines();
        curIndex = 0;
        time = 0d;
        Initialize();
    }

    private void Initialize( Chart _chart )
    {
        timings = _chart.timings;
        Initialize();
    }

    private void Initialize()
    {
        curIndex = 0;
        curTiming = timings[curIndex];
        time = curTiming.time;
        text.text = $"{( int )curTiming.bpm}";
        isStart = true;
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
