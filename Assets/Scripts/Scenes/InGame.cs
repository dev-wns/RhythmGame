using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InGame : Scene
{
    // ui
    public TextMeshProUGUI timeText, bpmText, comboText, frameText, medianText;
    public TextMeshProUGUI koolText, coolText, goodText;

    public delegate void DelGameStart();
    public event DelGameStart OnGameStart;

    public delegate void DelSpeedChanged();
    public event DelSpeedChanged OnScrollChanged;

    public NoteSystem[] noteSystems;
    public MeasureSystem measureSystem;

    private bool isStart = false;
    private Chart chart;

    float delta;

    public static float PreLoadTime { get { return ( 1250f / Weight ); } }
    // 60bpm은 분당 1/4박자 60개, 스크롤 속도가 1일때 한박자(1/4) 시간은 1초
    public static float Weight { get { return ( 60f / GameManager.Inst.MedianBpm ) * GlobalSetting.ScrollSpeed; } }

    protected override void Awake()
    {
        base.Awake();
        ChangeAction( SceneAction.InGame );

        GameManager.Combo = 0;
        GameManager.Kool  = 0;
        GameManager.Cool  = 0;
        GameManager.Good  = 0;
    }

    private void Start()
    {
        chart = NowPlaying.CurrentChart;
        // Notes
        var notes = chart.notes;
        for ( int i = 0; i < notes.Count; i++ )
        {
            noteSystems[notes[i].line].AddNote( notes[i] );
        }

        // Measures
        var timings = chart.timings;
        for ( int i = 0; i < timings.Count; i++ )
        {
            float time;
            Timing timing = timings[i];

            if ( timing.bpm < 10 )
                continue;
            float bpms = ( timing.bpm / 60f ) * 1000f / 4; // beat per milliseconds

            if ( i + 1 == timings.Count )
                time = chart.notes[chart.notes.Count - 1].time;
            else
                time = timings[i + 1].time;

            int maxCount = Mathf.FloorToInt( ( time - timing.time ) / bpms );
            measureSystem.AddTime( NowPlaying.GetChangedTime( timing.time ) );

            for ( int j = 1; j < maxCount + 1; j++ )
            {
                measureSystem.AddTime( NowPlaying.GetChangedTime( timing.time + (j * bpms) ) );
            }
        }

        OnGameStart();
        StartCoroutine( BpmChnager() );
        NowPlaying.Inst.Play();
    }

    private int timingIdx;
    private IEnumerator BpmChnager()
    {
        while ( timingIdx < chart.timings.Count )
        {
            yield return new WaitUntil( () => NowPlaying.Playback > chart.timings[timingIdx].time );
            bpmText.text = $"{Mathf.RoundToInt(chart.timings[timingIdx++].bpm)} BPM";
        }
    }

    protected override void Update()
    {
        base.Update();

        if ( !isStart ) return;

        timeText.text = string.Format( "{0:F1} 초", NowPlaying.Playback * 0.001f );
        delta += ( Time.unscaledDeltaTime - delta ) * .1f;
        frameText.text = string.Format( "{0:F1}", 1f / delta );

        comboText.text = string.Format( "{0}", GameManager.Combo );
        koolText.text = $"{ GameManager.Kool}";
        coolText.text = $"{ GameManager.Cool}";
        goodText.text = $"{ GameManager.Good}";
        //medianText.text = string.Format( "{0:F1}", MedianBpm ); 
    }

    public override void KeyBind()
    {
        Bind( SceneAction.InGame, KeyCode.Escape, () => SceneChanger.Inst.LoadScene( SCENE_TYPE.FREESTYLE ) );

        Bind( SceneAction.InGame, KeyCode.Alpha1, () => GlobalSetting.ScrollSpeed -= 1 );
        Bind( SceneAction.InGame, KeyCode.Alpha1, () => OnScrollChanged?.Invoke() );

        Bind( SceneAction.InGame, KeyCode.Alpha2, () => GlobalSetting.ScrollSpeed += 1 );
        Bind( SceneAction.InGame, KeyCode.Alpha2, () => OnScrollChanged?.Invoke() );
    }
}
