using System.Collections;
using UnityEngine;
using TMPro;

public class BpmChanger : MonoBehaviour
{
    [SerializeField] InGame scene;
    private const double DelayTime = ( 1d / 60d ) * 1000d;
    public TextMeshProUGUI text;

    private void Awake()
    {
        //NowPlaying.OnInitialize += Clear;
        scene.OnGameStart  += GameStart;
        scene.OnClear      += Clear;

        text.SetText( "{0}", 10000000 ); // capacity
        text.SetText( "{0}", 0 );
    }

    private void OnDestroy()
    {
        //NowPlaying.OnInitialize -= Clear;
        scene.OnGameStart  -= GameStart;
        scene.OnClear      -= Clear;
    }

    private void Clear()
    {
        StopAllCoroutines();
        text.SetText( "{0}", Mathf.RoundToInt( ( float )( DataStorage.Inst.Timings[0].bpm * GameSetting.SoundPitch ) ) );
    }

    private void GameStart() => StartCoroutine( UpdateBPM() );

    private IEnumerator UpdateBPM()
    {
        var    timings  = DataStorage.Inst.Timings;
        int    bpmIndex = 0;
        double bpmTime  = timings[bpmIndex].time;
        while ( bpmIndex < timings.Count )
        {
            yield return new WaitUntil( () => bpmTime < NowPlaying.Playback );

            Timing current = timings[bpmIndex];
            text.SetText( "{0}", Mathf.RoundToInt( ( float )( current.bpm * GameSetting.SoundPitch ) ) );

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
