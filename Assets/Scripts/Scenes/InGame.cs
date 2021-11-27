using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class InGame : Scene
{
    public MetaData data;

    public static float __time { get; private set; }
    private uint playbackTime = 0;             // ms
    public static float PlaybackChanged = 0f; // ms

    private int timingIdx = 0;

    private FMOD.Sound sound;
    private uint soundLength; // ms
    private bool isLoaded, isMusicStart;
    private float preLoad;

    // ui
    public TextMeshProUGUI timeText, bpmText, comboText, frameText;

    // create
    private int nIdx, mIdx; // note, measure create index

    public static ObjectPool<Note> nPool;
    public Note nPrefab;

    private List<float> mTimings = new List<float>(); // measure Timings
    public static ObjectPool<Measure> mPool;
    public Measure mPrefab;

    public InputSystem[] ISystems = new InputSystem[6];

    private float bpm, medianBpm, weight;
    float delta;

    protected override void Awake()
    {
        base.Awake();

        data = GameManager.SelectData;
        bpm = data.timings[0].bpm;

        nPool = new ObjectPool<Note>( nPrefab, 25 );
        mPool = new ObjectPool<Measure>( mPrefab, 5 );

        SoundLoad();
        FindBpmMedian();
        SetMeasureTiming();
        StartCoroutine( BpmChange() );
        StartCoroutine( NoteSystem() );
        StartCoroutine( MeasureSystem() );

        preLoad = ( medianBpm / 60f / 4 ) * 1000f;
        isLoaded = true;
    }

    private void Update()
    {
        if ( !isLoaded ) return;

        if ( !isMusicStart )
        {
            SoundManager.Inst.Play( sound );
            isMusicStart = true;
        }

        if ( !GlobalSetting.IsFixedScroll ) weight = 3f / 410f * GlobalSetting.ScrollSpeed;
        else                                weight = 3f / medianBpm * GlobalSetting.ScrollSpeed;

        //FMOD.Channel channel;
        //FMOD.RESULT res = SoundManager.Inst.channelGroup.getChannel( 0, out channel );
        //channel.getPosition( out playbackTime, FMOD.TIMEUNIT.MS );
        __time += Time.deltaTime * 1000f;
        PlaybackChanged = GetNoteTime( __time );
        //PlaybackChanged = GetNoteTime( playbackTime / 1000f );

        //if ( res != FMOD.RESULT.OK ) return;

        timeText.text = string.Format( "{0:F1}", __time / 1000f ) + " 초";
        comboText.text = string.Format( "{0}", GameManager.Combo );
        delta += ( Time.unscaledDeltaTime - delta ) * .1f;
        frameText.text = string.Format( "{0:F1}", 1f / delta );
    }

    private IEnumerator NoteSystem()
    {
        while ( true )
        {
            float timing = data.notes[nIdx].hitTiming;
            yield return new WaitUntil( () => timing <= __time + preLoad );

            float calcTiming = GetNoteTime( timing );
            for ( int i = 0; i < 6; i++ )
            {
                Notes note = data.notes[nIdx];
                Note LNStart = null;
                Note LNEnd = null;
                if ( note.type == 128 )
                {
                    LNStart = nPool.Spawn();
                    LNStart.SetNote( note.line, nIdx, weight, 2566, note.hitTiming, calcTiming, null );

                    LNEnd = nPool.Spawn();
                    LNEnd.SetNote( note.line, nIdx, weight, 3333, note.hitTiming, GetNoteTime( note.lengthLN ), null );
                }

                Note obj = nPool.Spawn();
                obj.SetNote( note.line, nIdx - 1, weight, note.type, note.hitTiming, calcTiming, LNEnd );
                ISystems[note.line].notes.Enqueue( obj );


                if ( nIdx < data.notes.Count - 1 ) nIdx++;
                if ( nIdx == data.notes.Count || timing != data.notes[nIdx].hitTiming ) break;
            }
        }
    }

    private IEnumerator MeasureSystem()
    {
        while ( true )
        {
            float time = GetNoteTime( mTimings[mIdx] );
            yield return new WaitUntil( () => time <= PlaybackChanged + preLoad );

            Measure measure = mPool.Spawn();
            measure.SetInfo( time, weight );

            if ( mIdx < mTimings.Count - 1 ) mIdx++;
        }
    }

    private IEnumerator BpmChange()
    {
        while ( true )
        {
            float changeTime = data.timings[timingIdx].changeTime;
            yield return new WaitUntil( () => __time >= changeTime );

            bpm = data.timings[timingIdx].bpm;
            bpmText.text = string.Format( "{0}", ( int )data.timings[timingIdx].bpm ) + " BPM";

            if ( timingIdx < data.timings.Count - 1 ) timingIdx++;
        }
    }

    private void FindBpmMedian()
    {
        List<float> bpmList = new List<float>();
        foreach ( var data in data.timings )
        {
            float bpm = data.bpm;
            if ( !bpmList.Contains( bpm ) )
            {
                bpmList.Add( bpm );
            }
        }

        bpmList.Sort();
        medianBpm = bpmList[Mathf.FloorToInt( bpmList.Count / 2f )];
    }

    private void SoundLoad()
    {
        sound = SoundManager.Inst.Load( data.audioPath );
        sound.getLength( out soundLength, FMOD.TIMEUNIT.MS );
    }

    private float GetNoteTime( float _time ) //BPM에 따른 노트 위치 계산
    {
        double newTime = _time;
        double prevBpm = 1;
        for ( int i = 0; i < data.timings.Count - 1; i++ )
        {
            double time = data.timings[i].changeTime;
            double listBpm = data.timings[i].bpm;
            double bpm;
            if ( time > _time ) break; //변속할 타이밍이 아니면 빠져나오기
            bpm = medianBpm / listBpm;
            newTime += ( double )( bpm - prevBpm ) * ( _time - time ); //거리계산
            prevBpm = bpm; //이전bpm값
        }
        return ( float )newTime; 
    }

    private void SetMeasureTiming()
    {
        for ( int i = 0; i < data.timings.Count; i++ )
        {
            float time;
            Timings timing = data.timings[i];
            float bpms = ( timing.bpm / 60f / 4f ) * 1000f; // beat per milliseconds

            if ( i + 1 == data.timings.Count ) time = soundLength;
            else                               time = data.timings[i + 1].changeTime;

            int a = Mathf.FloorToInt( ( time - timing.changeTime ) / bpms );
            mTimings.Add( timing.changeTime );

            for( int j = 0; j < a; j++ )
            {
                mTimings.Add( timing.changeTime + ( j * bpms ) );
            }
        }
    }
}
