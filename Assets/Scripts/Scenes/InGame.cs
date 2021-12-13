using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public struct Timings
{
    public float changeTime;
    public float bpm;

    public Timings( float _changeTime, float _bpm )
    {
        changeTime = _changeTime;
        bpm = _bpm;
    }
}

public struct Notes
{
    public int line;
    public float hitTiming;
    public int type;
    public int lengthLN;
    public Notes( int _x, float _hitTiming, int _type, int _lengthLN )
    {
        line = Mathf.FloorToInt( _x * 6f / 512f );
        hitTiming = _hitTiming;
        type = _type;
        lengthLN = _lengthLN;
    }
}

public class InGame : Scene
{
    // ui
    public TextMeshProUGUI timeText, bpmText, comboText, frameText, medianText;

    // Systems
    private NoteSystem[]  NSystems;
    private MeasureSystem MSystem;

    public static ObjectPool<Note> nPool;
    public Note nPrefab;

    public delegate void DelInitialized();
    public static event DelInitialized SystemsInitialized;

    float delta;

    // Bpm
    public static float BPM { get; private set; } // 현재 BPM
    public static float Weight // BPM 변화와 스크롤 속도를 고려한 오브젝트 속도 가중치
    {
        get
        {
            if ( !GlobalSetting.IsFixedScroll ) return 3f / BPM * GlobalSetting.ScrollSpeed;
            else return 3f / MedianBpm * GlobalSetting.ScrollSpeed;
        }
    }
    public static float MedianBpm;
    public delegate void BPMChangeDel();
    public static event BPMChangeDel BPMChangeEvent;

    // time ( millisecond )
    public static float Playback { get; private set; } // 노래 재생 시간
    public static float PlaybackChanged { get; private set; } // BPM 변화에 따른 노래 재생 시간
    private Timer timer = new Timer();

    // 60bpm은 분당 1/4박자 60개, 스크롤 속도가 1일때 한박자(1/4) 시간은 1초
    public static float PreLoadTime { get { return ( 150f / GlobalSetting.ScrollSpeed ) * 1000f; } } // 5박자 시간 ( 고정 스크롤 일때 )
    public static uint EndTime { get; private set; } // 노래 끝 시간 

    public static readonly float InitWaitTime = 1f;      // 시작 전 대기시간

    public static bool IsPlaying { get; private set; } = false;
    private int timingIdx;
    private Coroutine curCoroutine = null;

    class BPMS
    {
        public float bpm, time;
        public BPMS( float _bpm, float _time )
        { bpm = _bpm; time = _time; }
    }
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

    //public void Initialized( Song _data )
    //{
    //    if ( !ReferenceEquals( curCoroutine, null ) ) StopCoroutine( curCoroutine );

    //    List<BPMS> bpms = new List<BPMS>();
    //    for ( int i = 0; i < Data.timings.Count; i++ )
    //    {
    //        bpms.Add( new BPMS( Data.timings[i].bpm, Data.timings[i].changeTime ) );
    //    }

    //    List<MedianCac> medianCalc = new List<MedianCac>();
    //    for ( int i = 0; i < bpms.Count; i++ )
    //    {
    //        float t;
    //        double b;
    //        if ( i == 0 )
    //        {
    //            t = 0;
    //            b = bpms[0].bpm;
    //        }
    //        else
    //        {
    //            t = bpms[i - 1].time;
    //            b = bpms[i - 1].bpm;
    //        }
    //        bool find = false;
    //        for ( int j = 0; j < medianCalc.Count; j++ )
    //        {
    //            if ( Mathf.Abs( ( float )( b - medianCalc[j].bpm ) ) < 0.1f )
    //            {
    //                find = true;
    //                medianCalc[j].time += bpms[i].time - t;
    //            }
    //        }
    //        if ( !find ) medianCalc.Add( new MedianCac( bpms[i].time - t, ( float )b ) );
    //    }

    //    for ( int i = 0; i < medianCalc.Count; i++ )
    //        if ( medianCalc[i].bpm <= 30f ) medianCalc.RemoveAt( i ); //너무 적은 수치일시 적용방지

    //    medianCalc.Sort( delegate ( MedianCac A, MedianCac B )
    //    {

    //        if ( A.time >= B.time ) return -1;
    //        else return 1;
    //    }
    //    );
    //    MedianBpm = 1 / ( ( float )medianCalc[0].bpm / 60000f );

    //    InitializedVariables();
    //}

    //private void InitializedVariables()
    //{
    //    Playback = 0f; PlaybackChanged = 0f;
    //    timingIdx = 0; EndTime = 0; BPM = 0f;
    //    IsPlaying = false;
    //}

    //public void Play( bool _isSimpleMode = false )
    //{
    //    curCoroutine = StartCoroutine( PlayMusic( _isSimpleMode ) );
    //}

    //private IEnumerator PlayMusic( bool _isSimpleMode )
    //{
    //    if ( !_isSimpleMode )
    //    {
    //        StartCoroutine( BpmChange() );
    //        IsPlaying = true;
    //        // yield return new WaitUntil( () => Playback >= 0 );
    //        yield return YieldCache.WaitForSeconds( InitWaitTime );
    //    }
    //    else yield return null;

    //    SoundManager.Inst.LoadAndPlay( Data.audioPath );
    //    EndTime = SoundManager.Inst.Length;
    //    StartCoroutine( TimeUpdate() );
    //}

    //private IEnumerator BpmChange()
    //{
    //    BPM = Data.timings[0].bpm;
    //    BPMChangeEvent();

    //    while ( timingIdx < Data.timings.Count )
    //    {
    //        float changeTime = Data.timings[timingIdx].changeTime;
    //        yield return new WaitUntil( () => Playback >= changeTime );

    //        BPM = Data.timings[timingIdx].bpm;
    //        BPMChangeEvent();
    //        timingIdx++;
    //    }
    //    yield return null;
    //}

    //public static float GetChangedTime( float _time ) // BPM 변화에 따른 시간 계산
    //{
    //    double newTime = _time;
    //    double prevBpm = 0d;
    //    for ( int i = 0; i < Data.timings.Count; i++ )
    //    {
    //        double time = Data.timings[i].changeTime;
    //        double bpm = Data.timings[i].bpm;

    //        if ( time > _time ) break;
    //        bpm = MedianBpm / bpm;
    //        newTime += ( bpm - prevBpm ) * ( _time - time );
    //        prevBpm = bpm;
    //    }
    //    return ( float )newTime;
    //}

    //private IEnumerator TimeUpdate()
    //{
    //    Playback = 0;
    //    timer.Start();
    //    SoundManager.Inst.Position = 0;
    //    while ( Playback <= EndTime )
    //    {
    //        Playback = timer.elapsedMilliSeconds;
    //        Playback += Time.deltaTime * 1000f;
    //        PlaybackChanged = GetChangedTime( Playback );
    //        yield return null;
    //    }

    //    SoundManager.Inst.AllStop();
    //}


    private void Initialized()
    {
        //// Note
        //var notes = NowPlaying.Data.notes;
        //for ( int i = 0; i < notes.Count; i++ )
        //{
        //    float LNEndTime = 0f;
        //    if ( notes[i].type == 128 )
        //    {
        //        LNEndTime = NowPlaying.GetChangedTime( notes[i].lengthLN );
        //    }

        //    NoteData mydata = new NoteData( notes[i].hitTiming, NowPlaying.GetChangedTime( notes[i].hitTiming ), 
        //                                    notes[i].lengthLN, LNEndTime, 
        //                                    notes[i].type, notes[i].line );
        //    NSystems[notes[i].line].datas.Enqueue( mydata );
        //}

        //// Measure
        //var timings = NowPlaying.Data.timings;
        //for ( int i = 0; i < timings.Count; i++ )
        //{
        //    float time;
        //    Timings timing = timings[i];
            
        //    if ( timing.bpm < 60 || timing.bpm > 999 ) continue;
        //    float bpm = ( timing.bpm / 60f ) * 1000f; // beat per milliseconds

        //    if ( i + 1 == timings.Count ) time = NowPlaying.EndTime;
        //    else time = timings[i + 1].changeTime;

        //    int a = Mathf.FloorToInt( ( time - timing.changeTime ) / bpm );
        //    MSystem.timings.Enqueue( NowPlaying.GetChangedTime( timing.changeTime ) );

        //    for ( int j = 0; j < a; j++ )
        //    {
        //        MSystem.timings.Enqueue( NowPlaying.GetChangedTime( timing.changeTime + ( j * bpm ) ) );
        //    }
        //}
        
        SystemsInitialized();
    }


    protected override void Awake()
    {
        base.Awake();

        var system = GameObject.FindGameObjectWithTag( "Systems" );
        NSystems = system.GetComponentsInChildren<NoteSystem>();
        MSystem = system.GetComponent<MeasureSystem>();
        nPool = new ObjectPool<Note>( nPrefab, 25 );

        NowPlaying.BPMChangeEvent += () => { bpmText.text = string.Format( "{0} BPM", ( int )NowPlaying.BPM ); };

        //using ( Parser parser = new OsuParser( GlobalSoundInfo.CurrentSound.filePath ) )
        //{
            
        //}
    }

    private void Start()
    {
        Initialized();
        NowPlaying.Inst.Play();
    }

    private void Update()
    {
        timeText.text = string.Format( "{0:F1} 초", NowPlaying.Playback * 0.001f );
        //comboText.text = string.Format( "{0}", GameManager.Combo );
        delta += ( Time.unscaledDeltaTime - delta ) * .1f;
        frameText.text = string.Format( "{0:F1}", 1f / delta );
        medianText.text = string.Format( "{0:F1}", NowPlaying.MedianBpm ); 
    }
}
