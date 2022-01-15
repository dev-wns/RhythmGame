using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class BpmChanger : MonoBehaviour
{
    private InGame scene;
    public TextMeshProUGUI bpmText;
    private List<Timing> timings = new List<Timing>();
    private int timingIndex;
    private float currentBpm;

    public delegate void DelBpmChange( float _bpm );
    public event DelBpmChange OnBpmChange;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnSystemInitialize += Initialize;
        scene.OnGameStart        += StartProcess;
    }

    private void Initialize( in Chart _chart )
    {
        timings = _chart.timings.ToList();
    }

    private void StartProcess() => StartCoroutine( Process() );

    private IEnumerator Process()
    {
        while ( timingIndex < timings.Count )
        {
            float changeTime = timings[timingIndex].time;
            yield return new WaitUntil( () => NowPlaying.Playback >= changeTime );

            currentBpm = timings[timingIndex++].bpm;
            bpmText.text = $"{Mathf.RoundToInt( currentBpm )} BPM";
            OnBpmChange?.Invoke( currentBpm );
        }

        yield return null;
    }
}
