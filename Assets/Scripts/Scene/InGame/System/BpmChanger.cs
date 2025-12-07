using System.Collections;
using TMPro;
using UnityEngine;

public class BpmChanger : MonoBehaviour
{
    private const double DelayTime = ( 1d / 60d ) * 1000d;
    public TextMeshProUGUI text;

    private void Awake()
    {
        NowPlaying.OnInitialize += Clear;
        NowPlaying.OnGameStart  += GameStart;
        NowPlaying.OnClear      += Clear;
    }

    private void OnDestroy()
    {
        NowPlaying.OnInitialize -= Clear;
        NowPlaying.OnGameStart  -= GameStart;
        NowPlaying.OnClear      -= Clear;
    }

    private void Clear()
    {
        StopAllCoroutines();
        text.text = $"{Mathf.RoundToInt( ( float ) ( DataStorage.Timings[0].bpm * GameSetting.CurrentPitch ) )}";
    }

    private void GameStart() => StartCoroutine( UpdateBPM() );

    private IEnumerator UpdateBPM()
    {
        var    timings  = DataStorage.Timings;
        int    bpmIndex = 0;
        double bpmTime  = timings[bpmIndex].time;
        while ( bpmIndex < timings.Count )
        {
            yield return new WaitUntil( () => bpmTime < NowPlaying.Playback );

            Timing current = timings[bpmIndex];
            text.text = $"{Mathf.RoundToInt( ( float ) ( current.bpm * GameSetting.CurrentPitch ) )}";

            // ´ÙÀ½ BPM
            if ( ++bpmIndex < timings.Count )
            {
                bool needDelay = false;
                current = timings[bpmIndex];
                if ( bpmIndex + 1 < timings.Count )
                {
                    Timing next = timings[bpmIndex + 1];
                    if ( Global.Math.Abs( next.time - current.time ) > DelayTime )
                         needDelay = true;
                }

                bpmTime = needDelay ? current.time + DelayTime : current.time;
            }
        }
    }
}
