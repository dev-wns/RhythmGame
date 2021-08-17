using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileReader : MonoBehaviour
{
    #region Variables
    public static bool isPlaying, isVideoLoaded, isResultLoad, isLoaded;
    public static int noteCountLongNote;
    public static float judgeOffset = -3.15f;
    public static float playBackChanged, playBack;
    public static float bpm = 0;
    public static float startBpm = 0;
    public static double multiply;

    public GameObject noteObj, colObj, lnObj, endLN, result, gameover, initSetting, judgeObj, barObj, LoadCircle;

    public int progress = 0;

    private bool svEnd, noteEnd, rnoteEnd, sampleEnd, playFieldOn, musicOn;
    private int sampleIdx, noteIDX, rnoteIdx, barIdx, timeIdx, noteIdx, timingIdx, preLoad;
    private int timingCount, noteCount, lastNoteTiming;
    private int[] keys = { 0, 1, 2, 3, 4, 5 };
    private float offset, loadProgress;
    private float p, time, rTime, barTime;
    private float startTime;

    //private ScoreManager scoreMgr;
    //private RankSystem rankSys;
    private SoundHandler SoundSys;
    private GameObject world;
    private NowPlaying select;
    #endregion

    #region Notes, Sound, Queue, BPM Collections
    public Queue<GameObject> nQueue = new Queue<GameObject>(); // note queue
    public Queue<GameObject> bQueue = new Queue<GameObject>(); // bar queue

    [SerializeField] struct Timings
    {
        public float time;
        public double bpm;
        public Timings( float _time, double _bpm )
        {
            time = _time;
            bpm = _bpm;
        }
    }

    [SerializeField] struct Notes
    {
        public int column;
        public int time;
        public int keySoundIdx;
        public int lengthLN;
        public bool isLN;

        public Notes( int _column, int _time, bool _isLN, int _lengthLN, int _keySound )
        {
            column = _column;
            time = _time;
            isLN = _isLN;
            lengthLN = _lengthLN;
            keySoundIdx = _keySound;
        }
    }

    [SerializeField] struct Samples
    {
        public int time;
        public int idx;
        public Samples( int _time, int _idx )
        {
            time = _time;
            idx = _idx;
        }
    }

    [SerializeField] Notes[] noteList;                    // 일반노트
    [SerializeField] Timings[] timeList;                  // 타이밍
    [SerializeField] List<int> barList = new List<int>(); // 마디선
    [SerializeField] string[] keySounds;
    [SerializeField] List<string> loadedKeySounds = new List<string>();
    [SerializeField] List<Samples> sampleList = new List<Samples>();
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        barIdx = 0; // bar pooling index
        noteIdx = 0; // note pooling index
        timeIdx = 0; // timing pooling index
        rnoteIdx = 0; // note collider pooling index
        sampleIdx = 0;
        isResultLoad = false;
        isLoaded = false;
        musicOn = false;
        scoreMgr = GetComponent<ScoreManager>();

        if ( GlobalSettings.isRandom )
        {
            Random( keys );
        }

        for ( int i = 0; i < keys.Length; ++i )
        {
            if ( GlobalSettings.isMirror )
            {
                keys[ i ] = 6 - i;
            }
        }
    }

    private void Start()
    {
        LoadCircle.SetActive( true );
        playBack = 0;
        world = GameObject.FindWithTag( "World" );
        SoundSys = world.GetComponent<SoundHandler>();
        SoundSys.ReleaseKeySound();
        preLoad = ( int )( 1500.0f / GlobalSettings.scrollSpeed ); // note pooling offset

        playFieldOn = true;
        isPlaying = false;
        noteCountLongNote = 0;

        // get selected music infomation
        string filePath = NowPlaying.File;
        offset = NowPlaying.Offset;
        ReadFile( filePath ); // read start

        noteList = new Notes[ NowPlaying.NoteCounts + NowPlaying.LongNoteCounts ];
        timeList = new Timings[ NowPlaying.TimingCounts ];

        for ( int i = 0; i < 200; ++i )
        {
            SetPooling();
        }
        for ( int i = 0; i < 6; ++i )
        {
            BarPooling();
        }
    }

    private void Update()
    {
        judgeOffset = -3.15f + GlobalSettings.stagePosY;

        // create notes, timings
        if ( isLoaded )
        {
            if ( !musicOn )
            {
                if ( playBack > -100f + offset + GlobalSettings.globalOffset * 1000f )
                {
                    AudioStart();
                    musicOn = true;
                }
            }

            if ( !GlobalSettings.isFixedScroll )
            {
                multiply = 3f / 410f * GlobalSettings.scrollSpeed;
            }
            else
            {
                multiply = 3f / NowPlaying.Median * GlobalSettings.scrollSpeed;
            }

            p += Time.deltaTime * 1000f; // use deltatime
            playBack = p;
            playBackChanged = GetNoteTime( playBack );

            if ( noteEnd ) // game end
            {
                if ( !isResultLoad )
                {
                    int __t = lastNoteTiming + 4000;
                    if ( __t < playBack - 3700f && playFieldOn )
                    {
                        playFieldOn = false;
                        var gameObjects = GameObject.FindGameObjectsWithTag( "player" );
                        for ( var i = 0; i < gameObjects.Length; ++i )
                        {
                            gameObjects[ i ].GetComponent<ColumnSetting>().OnResult();
                        }
                    }

                    if ( __t < playBack )
                    {
                        if ( !ScoreManager.isFailed && !GlobalSettings.isAutoPlay )
                        {
                            scoreMgr.SaveScore();
                        }

                        isResultLoad = true;
                        StartCoroutine( ShowResult() );
                        world.GetComponent<GlobalSettings>().SaveSetting();
                    }
                }
            }

            if ( sampleList.Count != 0 )
            {
                SampleSystem();
            }
        }
    }

    private void SetPooling()
    {
        GameObject obj = Instantiate( colObj, new Vector2( 0f, 1000f ), Quaternion.identity );
        nQueue.Enqueue( obj );
        obj.SetActive( false );
    }

    private void BarPooling()
    {
        GameObject obj = Instantiate( barObj, new Vector2( 0f, 1000f ), Quaternion.identity );
        bQueue.Enqueue( obj );
        obj.SetActive( false );
    }
    #endregion

    #region Init Setting
    private void AudioStart()
    {
        if ( !NowPlaying.IsVirtual )
        {
            SoundSys.PlaySound();
            isPlaying = true;
        }
    }

    private IEnumerator InitSong() // init the data after the async shutdown
    {
        GetBarTime();
        yield return new WaitUntil( () => isVideoLoaded );
        Debug.Log( "Load Time : " + Time.timeSinceLevelLoad );
        yield return new WaitForSeconds( 1.2f );
        LoadCircle.GetComponent<LoadIcon>().Fade();
        yield return new WaitUntil( () => !Input.GetKey( KeyCode.LeftControl ) );
        startBpm = bpm = ( float )timeList[ 0 ].bpm; // ms consumed per beat
        StartCoroutine( BpmChange() );
        StartCoroutine( NoteSystem() );
        StartCoroutine( InputSystem() );
        StartCoroutine( mBarSystem() );

        playBackChanged = playBack = -2000f;
        p = playBack;
        startTime = Time.timeSinceLevelLoad * 1000f;
        isLoaded = true;

        yield return new WaitForSeconds( 1f );
        initSetting.GetComponent<PlayerInit>().HideChilds(); // turn off stage setting
    }

    private void Random<T> (T[] _array )
    {
        int random1, random2;

        T tmp;

        for ( int index = 0; index < _array.Length; ++index )
        {
            random1 = UnityEngine.Random.Range( 0, _array.Length );
            random2 = UnityEngine.Random.Range( 0, _array.Length );

            tmp = _array[ random1 ];
            _array[ random1 ] = _array[ random2 ];
            _array[ random2 ] = tmp;
        }
    }
    #endregion

    #region time calculate algorhythm
    private void GetBarTime()
    {
        for( int i = 0; i < timeList.Length; ++i )
        {
            float _t;
            if( i + 1 == timeList.Length )
            {
                _t = noteList[ noteList.Length - 1 ].time;
            }
            else
            {
                _t = timeList[ i + 1 ].time;
            }

            int a = Mathf.FloorToInt( ( float )( ( _t - timeList[ i ].time ) / ( 4 * timeList[ i ].bpm ) ) );
            barList.Add( ( int )timeList[ i ].time );

            for ( int j = 1; j < a; ++j )
            {
                barList.Add( Mathf.RoundToInt( ( float )( timeList[ i ].time + j * 4 * timeList[ i ].bpm ) ) );
            }
        }
    }

    private float GetNoteTime( double _time ) // BPM에 따른 노트 위치 계산
    {
        double newTime = _time;
        double prevBpm = 1;
        for( int i = 0; i < timeList.Length; ++i )
        {
            double __time = timeList[ i ].time;
            double __bpmList = timeList[ i ].bpm;
            double _bpm;
            if( __time > _time) // 변속할 타이밍이 아니면 빠져나오기
            {
                _bpm = ( NowPlaying.Median / __bpmList );
                newTime += ( _bpm - prevBpm ) * ( _time - __time ); // 거리 계산
                prevBpm = _bpm;
            }
        }
        return ( float )newTime;
    }
    #endregion

    #region Note Streaming System
    private void SampleSystem()
    {
        int sampleTime = sampleList[ sampleIdx ].time;
        if ( sampleTime <= playBack && !sampleEnd )
        {
            int tmp = sampleIdx;
            for( int i = 0; i < 6; ++i )
            {
                SoundSys.PlaySample( sampleList[ sampleIdx ].idx );
                
                if( sampleIdx < sampleList.Count - 1 )
                {
                    ++sampleIdx;
                }
                else
                {
                    sampleEnd = true;
                    break;
                }

                if ( sampleList[ tmp ].time != sampleList[ sampleIdx ].time )
                {
                    break;
                }
            }
        }
    }
}
