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

    public List<NoteSystem>  noteSystems  = new List<NoteSystem>();
    public MeasureSystem measureSystem;
    private Chart chart;
    float delta;

    private struct CalcNote
    {
        public Note? note;
        public float noteTime;
        public float sliderTime;
    }

    private void Start()
    {
        chart = NowPlaying.Inst.CurrentChart;
        Random.InitState( ( int )System.DateTime.Now.Ticks );
        
        CreateNotes();
        CreateMeasures();

        for ( int i = 0; i < 6; i++ )
              noteSystems[i].lane = i;

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

        timeText.text = string.Format( "{0:F1} 초", NowPlaying.Playback * 0.001f );
        delta += ( Time.unscaledDeltaTime - delta ) * .1f;
        frameText.text = string.Format( "{0:F1}", 1f / delta );

        //comboText.text = string.Format( "{0}", GameManager.Combo );
    }

    private void CreateNotes()
    {
        var notes = chart.notes;
        CalcNote[] column = new CalcNote[6];

        for ( int i = 0; i < notes.Count; i++ )
        {
            bool hasNoSliderMod = GameSetting.CurrentGameMod.HasFlag( GameMod.NoSlider );
            
            switch ( GameSetting.CurrentRandom )
            {
                case GameRandom.None:
                case GameRandom.Mirror:
                case GameRandom.Random:
                case GameRandom.Half_Random:
                {
                    Note newNote;
                    if ( hasNoSliderMod ) newNote = new Note( notes[i].line, notes[i].time, notes[i].calcTime, 0, 0, false );
                    else                  newNote = notes[i];
                    noteSystems[notes[i].line].AddNote( newNote );
                } break;

                case GameRandom.Max_Random:
                {
                    int count = -1;
                    // 타격시간이 같은 노트 저장
                    for ( int j = 0; j < 6; j++ )
                    {
                        if ( i + j < notes.Count && notes[i].time == notes[i + j].time )
                        {
                            column[notes[i + j].line].note     = notes[i + j];
                            column[notes[i + j].line].noteTime = notes[i + j].time;

                            if ( !hasNoSliderMod && notes[i + j].isSlider )
                            {
                                column[notes[i + j].line].sliderTime = notes[i + j].sliderTime;
                            }

                            count++;
                        }
                        else break;
                    }
                    i += count;

                    // 일반노트만 있을 때 스왑
                    for ( int j = 0; j < 6; j++ )
                    {
                        var rand = Random.Range( 0, 5 );

                        bool isOverlab = false;
                        for ( int k = 0; k < 6; k++ )
                        {
                            if ( column[k].sliderTime >= column[rand].noteTime )
                            {
                                isOverlab = true;
                                break;
                            }
                        }

                        if ( !isOverlab )
                        {
                            var tmp = column[j];
                            column[j] = column[rand];
                            column[rand] = tmp;
                        }
                    }

                    // 노트 추가
                    for ( int j = 0; j < 6; j++ )
                    {
                        if ( column[j].note.HasValue )
                        {
                            Note newNote;
                            Note note = column[j].note.Value;
                            if ( hasNoSliderMod ) newNote = new Note( note.line, note.time, note.calcTime, 0, 0, false );
                            else                  newNote = note;
                            noteSystems[j].AddNote( newNote );
                        }

                        column[j].note = null;
                    }
                } break;
            }
        }

        // 라인별 스왑일 때
        switch ( GameSetting.CurrentRandom )
        {
            case GameRandom.Mirror:
            noteSystems.Reverse();
            break;

            case GameRandom.Random:
            {
                for ( int i = 0; i < 6; i++ )
                {
                    var rand = Random.Range( 0, 5 );

                    var tmp = noteSystems[i];
                    noteSystems[i] = noteSystems[rand];
                    noteSystems[rand] = tmp;
                }
            } break;

            case GameRandom.Half_Random:
            {
                for ( int i = 0; i < 6; i++ )
                {
                    var rand = Random.Range( 0, 2 );

                    var tmp = noteSystems[i];
                    noteSystems[i] = noteSystems[rand];
                    noteSystems[rand] = tmp;
                }

                for ( int i = 2; i < 6; i++ )
                {
                    var rand = Random.Range( 2, 5 );

                    var tmp = noteSystems[i];
                    noteSystems[i] = noteSystems[rand];
                    noteSystems[rand] = tmp;
                }
            } break;
        }
    }

    private void CreateMeasures()
    {
        var timings = chart.timings;
        for ( int i = 0; i < timings.Count; i++ )
        {
            float time;
            Timing timing = timings[i];

            if ( timing.bpm < 10 ) continue;
            float bpms = ( timing.bpm / 60f ) * 1000f / 4; // beat per milliseconds

            if ( i + 1 == timings.Count ) time = chart.notes[chart.notes.Count - 1].time;
            else                          time = timings[i + 1].time;

            int maxCount = Mathf.FloorToInt( ( time - timing.time ) / bpms );
            measureSystem.AddTime( NowPlaying.GetChangedTime( timing.time ) );

            for ( int j = 1; j < maxCount + 1; j++ )
            {
                measureSystem.AddTime( NowPlaying.GetChangedTime( timing.time + ( j * bpms ) ) );
            }
        }
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
