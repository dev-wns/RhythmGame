using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class InGame : Scene
{
    public MetaData data;

    public static uint originTime = 0;
    public static float PlayBackChanged = 0f;
    public static uint changeTime;
    public static double BPM = 0d;
    public static double GlobalSpeed = 0d;

    private int noteIdx = 0;
    public int timingIdx = 0;
    public double prevBpm = 1d;

    private FMOD.Sound sound;
    private bool isSoundPlay = false;
    private float MedianBPM = 0f;

    public TextMeshProUGUI timeText;

    private ObjectPool<Note> notePool;
    public Note notePrefab;
    

    //private uint GetNoteTime( uint _time )
    //{
    //    uint newTime = _time;
    //    double curBpm = 0f;
    //    for ( int idx = 0; idx <= data.timings.Count - 1; ++idx )
    //    {
    //        uint nextTime = data.timings[idx].changeTime;
    //        curBpm = data.timings[idx].beatLength;

    //        if ( nextTime > _time ) break;

    //        if ( curBpm > 0 )
    //        {
    //            uninheritedTiming = data.timings[idx].beatLength;
    //        }
    //        else
    //        {
    //            curBpm = uninheritedTiming * -( data.timings[idx].beatLength / 100f );
    //        }

    //        double speed = curBpm - uninheritedTiming;
    //        if ( speed <= 0 ) speed = 1f;

    //        newTime = ( uint )( _time + ( speed * ( _time - nextTime ) ) );

    //        timingIdx = idx;
    //    }
    //    //prevBpm = curBpm;
    //    return ( uint )newTime;
    //}

    private List<double> BPMList = new List<double>();

    List<MedianCac> medianlist = new List<MedianCac>();
    class MedianCac
    {
        public float time; public double bpm; public int key;
        public MedianCac( float time, double bpm )
        {
            this.time = time;
            this.bpm = bpm;
            key = Mathf.FloorToInt( ( float )bpm );
        }
    }

    protected override void Awake()
    {
        base.Awake();

        notePool = new ObjectPool<Note>( notePrefab );

        data = NowPlaying.Inst.data;

        float prevBPM = 0f;
        for ( int i = 0; i < data.timings.Count; i++ )
        {
            var bpm = Mathf.Abs( data.timings[i].beatLength );

            if ( data.timings[i].isUninherited )
            {
                prevBPM = bpm;
                if ( bpm > 999999 )
                {
                    bpm = data.timings[i-1].beatLength;
                }
            }
            else
            {
                bpm = Mathf.Abs( ( prevBPM * 100f / bpm ) );
            }

            data.timings[i] = new MetaData.Timings( data.timings[i].changeTime, bpm, data.timings[i].isUninherited );
        }

        for ( int i = 0; i < data.timings.Count; i++ )
        {
            float t;
            double b;
            if ( i == 0 )
            {
                t = 0;
                b = data.timings[0].bpm;
            }
            else
            {
                t = data.timings[i - 1].changeTime;
                b = data.timings[i - 1].bpm;
            }
            bool find = false;
            for ( int j = 0; j < medianlist.Count; j++ )
            {
                if ( Mathf.Abs( ( float )( b - medianlist[j].bpm ) ) < 0.1f )
                {
                    find = true;
                    medianlist[j].time += data.timings[i].changeTime - t;
                }
            }
            if ( !find ) medianlist.Add( new MedianCac( data.timings[i].changeTime - t, b ) );
        }

        for ( int i = 0; i < medianlist.Count; i++ )
            if ( medianlist[i].bpm <= 30f ) medianlist.RemoveAt( i ); //너무 적은 수치일시 적용방지

        medianlist.Sort( delegate ( MedianCac A, MedianCac B )
        {

            if ( A.time >= B.time ) return -1;
            else return 1;
        }
        );
        float medianBPM = ( float )medianlist[0].bpm;

        MedianBPM = medianBPM;

        for ( int idx = 0; idx < data.notes.Count - 1; idx++ )
        {
            if ( data.notes.Count - 1 == noteIdx ) return;

            MetaData.Notes note = data.notes[idx];

            Note obj = notePool.Spawn();// NotePool.Inst.Dequeue();
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
    }
    float GetNoteTime( double TIME ) //BPM에 따른 노트 위치 계산
    {
        double newTIME = TIME;
        double prevBPM = 1;
        for ( int i = 0; i < data.timings.Count - 1; i++ )
        {
            double _time = data.timings[i].changeTime / 1000d;
            double _listbpm = data.timings[i].bpm;
            double _bpm;
            if ( _time > TIME ) break; //변속할 타이밍이 아니면 빠져나오기
            _bpm = ( MedianBPM / _listbpm );
            newTIME += ( double )( _bpm - prevBPM ) * ( TIME - _time ); //거리계산
            prevBPM = _bpm; //이전bpm값
        }
        return ( float )newTIME; //최종값 리턴
    }

    private void Update()
    {
        if ( !isSoundPlay )
        {
            sound = SoundManager.Inst.Load( data.audioPath );
            SoundManager.Inst.Play( sound );
            isSoundPlay = true;
        }

        FMOD.Channel channel;
        FMOD.RESULT res = SoundManager.Inst.channelGroup.getChannel( 0, out channel );
        channel.getPosition( out originTime, FMOD.TIMEUNIT.MS );
        PlayBackChanged = GetNoteTime( originTime / 1000d );


        //for ( int i = 0; i < data.timings.Count - 1; i++ )
        //{
        //    if ( data.timings[i].changeTime < originTime )
        //    {
        //        BPM = 1d / data.timings[i].beatLength * 1000d * 60d;
        //    }
        //}

        //changeTime = GetNoteTime( originTime );

        //if ( BPMIdx <= data.timings.Count - 1 )
        //{
        //    uint nextTime = data.timings[BPMIdx].changeTime;
        //    if ( nextTime < originTime )
        //    {
        //        double beatLength = data.timings[BPMIdx].beatLength;

        //        double uninheritedTiming = 1f;
        //        if ( beatLength > 0 )
        //        {
        //            BPM = 1f / data.timings[BPMIdx].beatLength * 1000f * 60f;
        //        }
        //        else
        //        {
        //            uninheritedTiming = BPM * - ( data.timings[BPMIdx].beatLength / 100f );
        //        }

        //        GlobalSpeed = ScrollSpeed * uninheritedTiming;
        //        ++BPMIdx;
        //    }
        //}


        if ( res != FMOD.RESULT.OK ) return;


        //while ( data.notes[noteIdx].hitTiming <= originTime + 4000f )//fourbeats )


        timeText.text = originTime.ToString();
    }
}
