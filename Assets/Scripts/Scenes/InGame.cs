using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class InGame : Scene
{
    // ui
    public TextMeshProUGUI timeText, bpmText, comboText, frameText;

    // Systems
    private NoteSystem[]  NSystems;
    private MeasureSystem MSystem;

    public static ObjectPool<Note> nPool;
    public Note nPrefab;

    public delegate void DelInitialized();
    public static event DelInitialized SystemsInitialized;


    float delta;

    private void Initialized()
    {
        // Note
        var notes = NowPlaying.Data.notes;
        for ( int i = 0; i < notes.Count; i++ )
        {
            NoteData mydata = new NoteData( notes[i].hitTiming, NowPlaying.GetChangedTime( notes[i].hitTiming ), notes[i].lengthLN, notes[i].type, notes[i].line );
            NSystems[notes[i].line].datas.Enqueue( mydata );
        }

        // Measure
        var timings = NowPlaying.Data.timings;
        for ( int i = 0; i < timings.Count; i++ )
        {
            float time;
            Timings timing = timings[i];
            float bpm = ( timing.bpm / 60f / 4f ) * 1000f; // beat per milliseconds

            if ( i + 1 == timings.Count ) time = NowPlaying.EndTime;
            else time = timings[i + 1].changeTime;

            int a = Mathf.FloorToInt( ( time - timing.changeTime ) / bpm );
            MSystem.timings.Enqueue( NowPlaying.GetChangedTime( timing.changeTime ) );

            for ( int j = 0; j < a; j++ )
            {
                MSystem.timings.Enqueue( NowPlaying.GetChangedTime( timing.changeTime + ( j * bpm ) ) );
            }
        }

        SystemsInitialized();
    }


    protected override void Awake()
    {
        base.Awake();

        var system = GameObject.FindGameObjectWithTag( "Systems" );
        NSystems = system.GetComponentsInChildren<NoteSystem>();
        MSystem = system.GetComponent<MeasureSystem>();
        nPool = new ObjectPool<Note>( nPrefab, 25 );

        NowPlaying.BPMChangeEvent += () => { bpmText.text = NowPlaying.BPM.ToString(); };
    }

    private void Start()
    {
        Initialized();
        NowPlaying.Inst.Play();
    }

    private void Update()
    {
        timeText.text = string.Format( "{0:F1} √ ", NowPlaying.Playback / 1000f );
        comboText.text = string.Format( "{0}", GameManager.Combo );
        delta += ( Time.unscaledDeltaTime - delta ) * .1f;
        frameText.text = string.Format( "{0:F1}", 1f / delta );
    }

    
}
