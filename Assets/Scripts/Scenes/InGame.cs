using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class InGame : Scene
{
    public MetaData data;

    public static uint PlaybackTime = 0;
    public static float PlaybackChanged = 0f;

    private int noteIdx = 0;
    public int timingIdx = 0;

    private FMOD.Sound sound;
    private bool isLoaded, isBpmWeight, isSoundLoad;

    public TextMeshProUGUI timeText;

    private ObjectPool<Note> notePool;
    public Note notePrefab;

    private IEnumerator BpmChange()
    {
        float changeTime = data.timings[timingIdx].changeTime;
        yield return new WaitUntil(()=> PlaybackTime >= changeTime );

        GlobalSetting.BPM = (float)data.timings[timingIdx].bpm;
        if ( timingIdx < data.timings.Count - 1 ) timingIdx++;

        StartCoroutine( BpmChange() );
    }

    private void FindBpmWeight()
    {
        List<float> bpmList = new List<float>();
        foreach ( var data in data.timings )
        {
            float bpm = ( float )data.bpm;
            if ( !bpmList.Contains( bpm ) )
            {
                bpmList.Add( bpm );
            }
        }

        bpmList.Sort();
        GlobalSetting.BPMWeight = bpmList[Mathf.FloorToInt( bpmList.Count / 2f )];
        isBpmWeight = true;
    }

    private void SoundLoad()
    {
        sound = SoundManager.Inst.Load( data.audioPath );
        isSoundLoad = true;
    }

    protected override void Awake()
    {
        base.Awake();

        notePool = new ObjectPool<Note>( notePrefab );

        data = GameManager.SelectData;

        StartCoroutine( BpmChange() );
        SoundLoad();
        FindBpmWeight();

        for ( int idx = 0; idx < data.notes.Count - 1; idx++ )
        {
            if ( data.notes.Count - 1 == noteIdx ) return;

            MetaData.Notes note = data.notes[idx];

            Note obj = notePool.Spawn();
            Note LNStart = null;
            Note LNEnd = null;

            if ( note.type == 128 )
            {
                LNStart = notePool.Spawn();
                LNStart.SetNote( note.x, 2566, GetNoteTime( note.hitTiming / 1000d ), null );

                LNEnd = notePool.Spawn();
                LNEnd.SetNote( note.x, 3333, GetNoteTime( note.lengthLN / 1000d ), null );
            }

            obj.SetNote( note.x, note.type, GetNoteTime( note.hitTiming / 1000d ), LNEnd );

            noteIdx++;
        }

        SoundManager.Inst.Play( sound );
        isLoaded = true;
    }

    private float GetNoteTime( double _time ) //BPM에 따른 노트 위치 계산
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
        return ( float )newTime; //최종값 리턴
    }

    private void Update()
    {
        if ( !isLoaded ) return;

        FMOD.Channel channel;
        FMOD.RESULT res = SoundManager.Inst.channelGroup.getChannel( 0, out channel );
        channel.getPosition( out PlaybackTime, FMOD.TIMEUNIT.MS );
        PlaybackChanged = GetNoteTime( PlaybackTime / 1000d );


        if ( res != FMOD.RESULT.OK ) return;

        timeText.text = PlaybackTime.ToString();
    }
}
