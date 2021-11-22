using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class InGame : Scene
{
    public MetaData data;

    private uint PlaybackTime = 0;             // ms
    public static float PlaybackChanged = 0f; // second

    private int noteIdx = 0;
    private int timingIdx = 0;

    private FMOD.Sound sound;
    private uint soundLength; // ms
    private bool isLoaded;

    public TextMeshProUGUI timeText;

    private ObjectPool<Note> nPool;
    public Note nPrefab;

    private List<float> mTimings = new List<float>(); // measure Timings
    private ObjectPool<MeasureLine> mPool;
    public MeasureLine mPrefab;

    protected override void Awake()
    {
        base.Awake();

        nPool = new ObjectPool<Note>( nPrefab );
        mPool = new ObjectPool<MeasureLine>( mPrefab );

        data = GameManager.SelectData;

        GlobalSetting.BPM = data.timings[0].bpm;
        StartCoroutine( BpmChange() );
        SoundLoad();
        FindBpmWeight();
        SetMeasureTiming();

        for ( int idx = 0; idx < data.notes.Count - 1; idx++ )
        {
            if ( data.notes.Count - 1 == noteIdx ) return;

            MetaData.Notes note = data.notes[idx];

            Note obj = nPool.Spawn();
            Note LNStart = null;
            Note LNEnd = null;

            if ( note.type == 128 )
            {
                LNStart = nPool.Spawn();
                LNStart.SetNote( note.x, 2566, GetNoteTime( note.hitTiming / 1000f ), null );

                LNEnd = nPool.Spawn();
                LNEnd.SetNote( note.x, 3333, GetNoteTime( note.lengthLN / 1000f ), null );
            }

            obj.SetNote( note.x, note.type, GetNoteTime( note.hitTiming / 1000f ), LNEnd );

            noteIdx++;
        }

        for ( int i = 0; i < mTimings.Count - 1; i++ )
        {
            MeasureLine measure = mPool.Spawn();
            measure.SetInfo( GetNoteTime( mTimings[i] / 1000f ) );
        }

        SoundManager.Inst.Play( sound );
        isLoaded = true;
    }

    private void Update()
    {
        if ( !isLoaded ) return;

        FMOD.Channel channel;
        FMOD.RESULT res = SoundManager.Inst.channelGroup.getChannel( 0, out channel );
        channel.getPosition( out PlaybackTime, FMOD.TIMEUNIT.MS );
        PlaybackChanged = GetNoteTime( PlaybackTime / 1000f );

        if ( res != FMOD.RESULT.OK ) return;

        timeText.text = PlaybackTime.ToString();
    }

    private IEnumerator BpmChange()
    {
        float changeTime = data.timings[timingIdx].changeTime;
        yield return new WaitUntil(()=> PlaybackTime >= changeTime );

        GlobalSetting.BPM = data.timings[timingIdx].bpm;
        if ( timingIdx < data.timings.Count - 1 ) timingIdx++;

        StartCoroutine( BpmChange() );
    }

    private void FindBpmWeight()
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
        GlobalSetting.BPMWeight = bpmList[Mathf.FloorToInt( bpmList.Count / 2f )];
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
            double time = data.timings[i].changeTime / 1000d;
            double listBpm = data.timings[i].bpm;
            double bpm;
            if ( time > _time ) break; //변속할 타이밍이 아니면 빠져나오기
            bpm = GlobalSetting.BPMWeight / listBpm;
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
            MetaData.Timings timing = data.timings[i];

            if ( i + 1 == data.timings.Count ) time = soundLength;
            else                               time = data.timings[i + 1].changeTime;

            int a = Mathf.FloorToInt( ( time - timing.changeTime ) / ( timing.bpm * 4f ) );
            mTimings.Add( timing.changeTime );

            for( int j = 0; j < a; j++ )
            {
                mTimings.Add( timing.changeTime + ( j * timing.bpm * 4f ) );
            }
        }
    }
}
