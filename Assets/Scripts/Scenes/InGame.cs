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
    float length;

    public static float PreLoadTime { get { return ( 1250f / Weight ); } }
    // 60bpm은 분당 1/4박자 60개, 스크롤 속도가 1일때 한박자(1/4) 시간은 1초
    public static float Weight { get { return ( 60f / GameManager.Inst.MedianBpm ) * GlobalSetting.ScrollSpeed; } }

    // time ( millisecond )
    public static float Playback { get; private set; } // 노래 재생 시간
    public static float PlaybackChanged { get; private set; } // BPM 변화에 따른 노래 재생 시간

    public float GetChangedTime( float _time ) // BPM 변화에 따른 시간 계산
    {
        double newTime = _time;
        double prevBpm = 0d;
        for ( int i = 0; i < chart.timings.Count; i++ )
        {
            double time = chart.timings[i].time;
            double bpm = chart.timings[i].bpm;

            if ( time > _time ) break;
            newTime += ( bpm - prevBpm ) * ( _time - time );
            prevBpm = bpm;
        }
        return ( float )newTime;
    }

    protected override void Awake()
    {
        base.Awake();
        Playback = PlaybackChanged = 0f;
        ChangeAction( SceneAction.InGame );

        // Parse
        using ( FileParser parser = new FileParser() )
        {
            parser.TryParse( GameManager.Inst.CurrentSong.filePath, out chart );
        }

        GameManager.Combo = 0;
        GameManager.Kool  = 0;
        GameManager.Cool  = 0;
        GameManager.Good  = 0;
    }

    private void Start()
    {
        // Notes
        var notes = chart.notes;
        for ( int i = 0; i < notes.Count; i++ )
        {
            var time = notes[i].time + GlobalSetting.SoundOffset;
            var calcTime = GetChangedTime( notes[i].time + GlobalSetting.SoundOffset );

            var note = new Note();
            note.line = notes[i].line;
            note.time = time;
            note.calcTime = calcTime;
            note.sliderTime = notes[i].sliderTime;
            note.calcSliderTime = notes[i].calcSliderTime;
            note.isSlider = notes[i].isSlider;

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
            measureSystem.AddTime( GetChangedTime( timing.time ) );

            for ( int j = 1; j < maxCount + 1; j++ )
            {
                measureSystem.AddTime( GetChangedTime( timing.time + (j * bpms) ) );
            }
        }

        SoundManager.Inst.LoadBgm( GameManager.Inst.CurrentSong.audioPath );
        SoundManager.Inst.PlayBgm( true );
        OnGameStart();
        length = chart.notes[chart.notes.Count - 1].time;

        StartCoroutine( WaitBeginningTime() );
        StartCoroutine( BpmChnager() );
    }

    private IEnumerator WaitBeginningTime()
    {
        yield return YieldCache.WaitForSeconds( 3f );
        isStart = true;
        SoundManager.Inst.PauseBgm( false );
    }


    private int timingIdx;
    private IEnumerator BpmChnager()
    {
        while ( timingIdx < chart.timings.Count )
        {
            yield return new WaitUntil( () => Playback > chart.timings[timingIdx].time );
            bpmText.text = $"{Mathf.RoundToInt(chart.timings[timingIdx++].bpm)} BPM";
        }
    }

    protected override void Update()
    {
        base.Update();

        if ( !isStart ) return;
        
        Playback += Time.deltaTime * 1000f;
        PlaybackChanged = GetChangedTime( Playback );

        timeText.text = string.Format( "{0:F1} 초", Playback * 0.001f );
        delta += ( Time.unscaledDeltaTime - delta ) * .1f;
        frameText.text = string.Format( "{0:F1}", 1f / delta );

        comboText.text = string.Format( "{0}", GameManager.Combo );
        koolText.text = $"{ GameManager.Kool}";
        coolText.text = $"{ GameManager.Cool}";
        goodText.text = $"{ GameManager.Good}";
        //medianText.text = string.Format( "{0:F1}", MedianBpm ); 

        if ( Playback > length )
        {
            Debug.Log( $"Kool {GameManager.Kool}  Cool {GameManager.Cool}  Good {GameManager.Good}" );
        }
    }

    public override void KeyBind()
    {
        Bind( SceneAction.InGame, KeyCode.Escape, () => SceneChanger.Inst.LoadScene( SCENE_TYPE.FREESTYLE ) );

        //Bind( SceneAction.InGame, KeyCode.Alpha1, () => GlobalSetting.ScrollSpeed -= 1 );
        //Bind( SceneAction.InGame, KeyCode.Alpha1, () => OnScrollChanged?.Invoke() );

        //Bind( SceneAction.InGame, KeyCode.Alpha2, () => GlobalSetting.ScrollSpeed += 1 );
        //Bind( SceneAction.InGame, KeyCode.Alpha2, () => OnScrollChanged?.Invoke() );
    }
}
