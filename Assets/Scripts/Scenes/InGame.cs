using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InGame : Scene
{
    // ui
    public TextMeshProUGUI timeText, bpmText, comboText, frameText;

    public delegate void DelGameStart();
    public event DelGameStart OnGameStart;

    public delegate void DelSpeedChanged();
    public event DelSpeedChanged OnScrollChanged;

    public NoteSystem[] noteSystems;
    public MeasureSystem measureSystem;

    float delta;

    private void Start()
    {
        var chart = NowPlaying.Inst.CurrentChart;
        // Notes
        var notes = chart.notes;
        for ( int i = 0; i < notes.Count; i++ )
        {
            Note newNote = new Note( notes[i].line, notes[i].time, notes[i].calcTime,
                0, 0, false );
            
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
                measureSystem.AddTime( NowPlaying.GetChangedTime( timing.time + ( j * bpms ) ) );
            }
        }

        OnGameStart();
        StartCoroutine( BpmChnager() );
        NowPlaying.Inst.Play();
    }

    private int timingIdx;
    private IEnumerator BpmChnager()
    {
        var chart = NowPlaying.Inst.CurrentChart;
        while ( timingIdx < chart.timings.Count )
        {
            yield return new WaitUntil( () => NowPlaying.Playback > chart.timings[timingIdx].time );
            bpmText.text = $"{Mathf.RoundToInt(chart.timings[timingIdx++].bpm)} BPM";
        }
    }

    protected override void Update()
    {
        base.Update();

        timeText.text = string.Format( "{0:F1} ÃÊ", NowPlaying.Playback * 0.001f );
        delta += ( Time.unscaledDeltaTime - delta ) * .1f;
        frameText.text = string.Format( "{0:F1}", 1f / delta );

        //comboText.text = string.Format( "{0}", GameManager.Combo );
    }

    public override void KeyBind()
    {
        Bind( SceneAction.Main, KeyCode.Escape, () => SceneChanger.Inst.LoadScene( SceneType.FreeStyle ) );

        Bind( SceneAction.Main, KeyCode.Alpha1, () => GameSetting.ScrollSpeed -= 1 );
        Bind( SceneAction.Main, KeyCode.Alpha1, () => OnScrollChanged?.Invoke() );

        Bind( SceneAction.Main, KeyCode.Alpha2, () => GameSetting.ScrollSpeed += 1 );
        Bind( SceneAction.Main, KeyCode.Alpha2, () => OnScrollChanged?.Invoke() );
    }
}
