using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public enum NoteType { None, Default, Slider }
public struct InputData
{
    public double    time;
    public double    diff;
    public KeyState  keyState;
    public NoteState noteState;

    public InputData( double _time, double _diff, KeyState _keyState = KeyState.None, NoteState _noteState = NoteState.None )
    {
        time = _time;
        diff = _diff;
        keyState = _keyState;
        noteState = _noteState;
    }
}
public enum NoteState { None, Hit, Miss, }

public class InputSystem : MonoBehaviour
{
    #region Objects
    private Lane      lane;
    private InGame    scene;
    private Judgement judge;
    #endregion

    #region Note
    private Queue<InputData> inputQueue = new ();
    public NoteRenderer note1 /* Lane 0,2,3,5 */, note2 /* Lane 1,4 */, noteMedian;
    private ObjectPool<NoteRenderer> notePool;
    private Queue<NoteRenderer>      notes            = new Queue<NoteRenderer>();
    private Queue<NoteRenderer>      sliderMissQueue  = new Queue<NoteRenderer>();
    private Queue<NoteRenderer>      sliderEarlyQueue = new Queue<NoteRenderer>();

    private List<Note>   noteDatas = new List<Note>();
    private NoteRenderer curNote;
    //private Note         curData;
    //private int          noteIndexs;
    #endregion

    public event Action<NoteType, KeyState> OnHitNote;
    public event Action<KeyState> OnInputEvent;
    public event Action OnStopEffect;

    private int vKey;
    private KeyCode uKey;
    //private KeySound curSound;
    private bool isAuto;


    #region Time
    private double inputStartTime;
    private double inputHoldTime;
    #endregion

    #region Auto
    private double target;
    #endregion

    private CancellationTokenSource cancelSource = new();
    [DllImport( "user32.dll" )]
    private static extern short GetAsyncKeyState( int _vKey );

    #region Unity Event Function
    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnGameStart += GameStart;
        scene.OnGameOver  += GameOver;
        scene.OnReLoad    += ReLoad;
        scene.OnPause     += Pause;

        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();

        lane = GetComponent<Lane>();
        lane.OnLaneInitialize += Initialize;

        isAuto = GameSetting.CurrentGameMode.HasFlag( GameMode.AutoPlay );
    }

    private void Start()
    {
        target = UnityEngine.Random.Range( -( float )Judgement.Bad, ( float )Judgement.Bad );
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        cancelSource?.Cancel();
    }

    private void OnApplicationQuit()
    {
        cancelSource?.Cancel();
    }
    #endregion

    #region Event
    public void Initialize( int _lane )
    {
        uKey = KeySetting.Inst.Keys[( GameKeyCount )NowPlaying.KeyCount][_lane];
        vKey = KeySetting.Inst.GetVirtualKey( uKey );

        NoteRenderer note = note1;
        if (      NowPlaying.KeyCount == 4 ) note = _lane == 1 || _lane == 2 ? note2 : note1;
        else if ( NowPlaying.KeyCount == 6 ) note = _lane == 1 || _lane == 4 ? note2 : note1;
        else if ( NowPlaying.KeyCount == 7 ) note = _lane == 1 || _lane == 5 ? note2 : _lane == 3 ? noteMedian : note1;
        notePool ??= new ObjectPool<NoteRenderer>( note, 5 );

        //if ( noteDatas.Count > 0 )
        //{
        //    curData  = noteDatas[noteIndex];
        //    curSound = noteDatas[noteIndex].keySound;
        //}
    }

    private async void GameStart()
    {
        StartCoroutine( SpawnNotes() );
        StartCoroutine( SliderMissCheck() );
        StartCoroutine( SliderEarlyCheck() );

        await Task.Run( () => Process( cancelSource.Token ) );
    }

    private IEnumerator SpawnNotes()
    {
        int index = 0;
        Note data = noteDatas.Count > 0 ? noteDatas[index] : new Note();
        WaitUntil waitSpawnTime = new WaitUntil( () => data.noteDistance <= NowPlaying.Distance + GameSetting.MinDistance );
        while( true )
        {
            if ( index > noteDatas.Count )
                 yield break;

            yield return waitSpawnTime;

            NoteRenderer note = notePool.Spawn();
            note.SetInfo( lane.Key, in data );
            notes.Enqueue( note );

            if ( ++index < noteDatas.Count )
                 data = noteDatas[index];
        }
    }

    private void Update()
    {
        if ( !NowPlaying.IsStart )
             return;


        // Lane Effect 출력용 ( Unity Input 사용 )
        if ( Input.GetKeyDown( uKey ) )    OnInputEvent?.Invoke( KeyState.Down );
        else if ( Input.GetKeyUp( uKey ) ) OnInputEvent?.Invoke( KeyState.Up );

        // Judgement
        //if ( isAuto ) AutoCheckNote();
        //else          CheckNote();
    }

    private void LateUpdate()
    {
        //if ( scene.IsGameInputLock )
        //    return;

        // 최하단 노트 선택 ( NoteSpawn 코루틴 사용으로 LateUpdate에서 갱신 )
        if ( curNote == null && notes.Count > 0 )
             curNote = notes.Dequeue();

        // 판정 처리 ( Virtual Key 사용 )
        if ( curNote == null || !inputQueue.TryDequeue( out InputData data ) )
             return;

        switch ( data.keyState )
        {
            case KeyState.Down:
            {
                if ( data.noteState == NoteState.Hit )
                {
                    OnHitNote?.Invoke( curNote.IsSlider ? NoteType.Slider : NoteType.Default, KeyState.Down );
                    judge.ResultUpdate( data.diff, NoteType.Default );

                    if ( !curNote.IsSlider )
                        SelectNextNote();
                }
            }
            break;

            case KeyState.Up:
            {
                if ( data.noteState == NoteState.Hit )
                {
                    sliderEarlyQueue.Enqueue( curNote );
                    OnHitNote?.Invoke( NoteType.Slider, KeyState.Up );

                    judge.ResultUpdate( data.diff, NoteType.Slider );
                    SelectNextNote( false );
                }
                else if ( data.noteState == NoteState.Miss )
                {
                    curNote.SetSliderFail();
                    sliderMissQueue.Enqueue( curNote );

                    judge.ResultUpdate( HitResult.Miss, NoteType.Slider );
                    SelectNextNote( false );
                }
            }
            break;

            case KeyState.None:
            {
                // 롱노트 끝점까지 Holding 한 경우 ( 자동처리 보정 )
                if ( data.noteState == NoteState.Hit )
                {
                    OnHitNote?.Invoke( NoteType.Slider, KeyState.Up );

                    judge.ResultUpdate( data.diff, NoteType.Slider );
                    SelectNextNote();
                }
                // 아무런 입력을 하지않아 Miss 처리된 경우
                else if ( data.noteState == NoteState.Miss )
                {
                    if ( !curNote.IsSlider )
                    {
                        judge.ResultUpdate( HitResult.Miss, NoteType.Default );
                        SelectNextNote();
                    }
                    else
                    {
                        curNote.SetSliderFail();
                        sliderMissQueue.Enqueue( curNote );

                        // 입력이 없어 처리된 판정이므로 시작과 끝 판정을 Miss 처리한다.
                        judge.ResultUpdate( HitResult.Miss, NoteType.Slider, 2 );
                        SelectNextNote( false );
                    }
                }
            }
            break;
        }
    }

    private async void Process( CancellationToken _token )
    {
        int index = 0;
        KeyState keyState = KeyState.None;
        bool isEntry = false; // 하나의 입력으로 하나의 노트만 처리하기위한 노트 진입점
        Debug.Log( " Input Process Start " );

        try
        {
            while ( !_token.IsCancellationRequested )
            {
                if ( index >= noteDatas.Count )
                     break;

                double startDiff = noteDatas[index].time       - NowPlaying.Playback;
                double endDiff   = noteDatas[index].sliderTime - NowPlaying.Playback;

                if ( !isEntry && Judgement.IsMiss( startDiff ) )
                {
                    isEntry = false;
                    index   = ++index;
                    inputQueue.Enqueue( new InputData( NowPlaying.Playback, startDiff, KeyState.None, NoteState.Miss ) );

                    continue;
                }

                KeyState previous = keyState;
                if ( ( GetAsyncKeyState( vKey ) & 0x8000 ) != 0 )
                {
                    keyState = previous == KeyState.None || previous == KeyState.Up ? KeyState.Down : KeyState.Hold;
                    if ( !isEntry && keyState == KeyState.Down )
                    {
                        AudioManager.Inst.Play( noteDatas[index].keySound );

                        if ( Judgement.CanBeHit( startDiff ) )
                        {
                            // 일반노트는 끝판정 처리를 무시한다.
                            isEntry = noteDatas[index].isSlider;
                            index   = noteDatas[index].isSlider ? index : ++index;

                            inputQueue.Enqueue( new InputData( NowPlaying.Playback, startDiff, KeyState.Down, NoteState.Hit ) );
                        }
                    }
                }
                else
                {
                    keyState = previous == KeyState.Down || previous == KeyState.Hold ? KeyState.Up : KeyState.None;

                    // 롱노트 끝 점에 도달하면 퍼펙트 판정
                    if ( isEntry && previous == KeyState.Hold && endDiff < 0d )
                    {
                        isEntry = false;
                        index   = ++index;
                        inputQueue.Enqueue( new InputData( NowPlaying.Playback, 0d, KeyState.None, NoteState.Hit ) );

                        continue;
                    }

                    if ( isEntry && keyState == KeyState.Up )
                    {
                        isEntry = false;
                        index   = ++index;
                        inputQueue.Enqueue( new InputData( NowPlaying.Playback, endDiff, KeyState.Up,
                                                           Judgement.CanBeHit( endDiff ) ? NoteState.Hit : NoteState.Miss ) );
                    }

                }
            }

            await Task.Delay( 1 ); // 1000Hz
        }
        catch( Exception _ex )
        {
            Debug.Log( _ex.Message );
        }
        Debug.Log( " Input Process End " );
    }

    //private void CheckNote()
    //{
    //    if ( curNote == null )
    //        return;

    //    double startDiff = curNote.Time       - NowPlaying.Playback;
    //    double endDiff   = curNote.SliderTime - NowPlaying.Playback;

    //    if ( !curNote.IsSlider )
    //    {
    //        if ( Input.GetKeyDown( key ) && Judgement.CanBeHit( startDiff ) )
    //        {
    //            OnHitNote?.Invoke( NoteType.Default, KeyState.Down );
    //            judge.ResultUpdate( startDiff, NoteType.Default );
    //            SelectNextNote();
    //            return;
    //        }

    //        if ( Judgement.IsMiss( startDiff ) )
    //        {
    //            judge.ResultUpdate( HitResult.Miss, NoteType.Default );
    //            SelectNextNote();
    //        }
    //    }
    //    else
    //    {
    //        if ( !curNote.IsHolding )
    //        {
    //            if ( Input.GetKeyDown( key ) && Judgement.CanBeHit( startDiff ) )
    //            {
    //                curNote.IsHolding = true;

    //                OnHitNote?.Invoke( NoteType.Slider, KeyState.Down );
    //                judge.ResultUpdate( startDiff, NoteType.Default );

    //                inputStartTime = NowPlaying.Playback;
    //                return;
    //            }

    //            if ( judge.IsMiss( startDiff, NoteType.Default ) )
    //            {
    //                curNote.SetSliderFail();
    //                judge.ResultUpdate( HitResult.Miss, NoteType.Slider, 2 );
    //                sliderMissQueue.Enqueue( curNote );
    //                SelectNextNote( false );
    //            }
    //        }
    //        else
    //        {
    //            if ( endDiff < 0d )
    //            {
    //                judge.ResultUpdate( 0d, NoteType.Slider );
    //                OnHitNote?.Invoke( NoteType.Slider, KeyState.Up );
    //                SelectNextNote();
    //                return;
    //            }

    //            inputHoldTime = NowPlaying.Playback - inputStartTime;
    //            if ( inputHoldTime > .1d )
    //            {
    //                judge.ResultUpdate( HitResult.None, NoteType.None );
    //                inputStartTime = NowPlaying.Playback - ( inputHoldTime - .1d );
    //            }

    //            if ( Input.GetKeyUp( key ) )
    //            {
    //                OnHitNote?.Invoke( NoteType.Slider, KeyState.Up );
    //                if ( Judgement.CanBeHit( endDiff ) )
    //                {
    //                    judge.ResultUpdate( endDiff, NoteType.Slider );
    //                    sliderEarlyQueue.Enqueue( curNote );
    //                    SelectNextNote( false );
    //                }
    //                else
    //                {
    //                    curNote.SetSliderFail();
    //                    judge.ResultUpdate( HitResult.Miss, NoteType.Slider );
    //                    sliderMissQueue.Enqueue( curNote );
    //                    SelectNextNote( false );
    //                }
    //            }
    //        }
    //    }
    //}

    private void ReLoad()
    {
        StopAllCoroutines();
        while ( sliderMissQueue.Count > 0 )
        {
            var slider = sliderMissQueue.Dequeue();
            slider.Despawn();
        }

        while ( sliderEarlyQueue.Count > 0 )
        {
            var slider = sliderEarlyQueue.Dequeue();
            slider.Despawn();
        }

        curNote?.Despawn();
        while ( notes.Count > 0 )
        {
            var note = notes.Dequeue();
            note.Despawn();
        }

        notePool.AllDespawn();

        GameManager.Inst.Clear();
        //noteIndex = 0;
        curNote = null;
        //curSound = new KeySound();
    }
    public void AddNote( in Note _note )
    {
        noteDatas.Add( _note );
    }
    private void GameOver()
    {
        OnStopEffect?.Invoke();
    }

    /// <summary> process the slider when pausing, it will be judged immediately. </summary>
    private void Pause( bool _isPause )
    {
        OnStopEffect?.Invoke();

        if ( !_isPause || curNote == null || !curNote.IsSlider )
            return;

        if ( isAuto )
        {
            judge.ResultUpdate( HitResult.Perfect, NoteType.Slider );
            SelectNextNote();
        }
        else
        {
            curNote.SetSliderFail();
            judge.ResultUpdate( HitResult.Miss, NoteType.Slider );
            sliderMissQueue.Enqueue( curNote );
            SelectNextNote( false );
        }
    }
    #endregion

    #region Note Process
    private IEnumerator SliderEarlyCheck()
    {
        var WaitEnqueue  = new WaitUntil( () => sliderEarlyQueue.Count > 0 );
        while ( true )
        {
            yield return WaitEnqueue;

            var slider = sliderEarlyQueue.Peek();
            if ( slider.SliderTime < NowPlaying.Playback )
            {
                slider.Despawn();
                sliderEarlyQueue.Dequeue();
            }
        }
    }

    private IEnumerator SliderMissCheck()
    {
        var WaitEnqueue  = new WaitUntil( () => sliderMissQueue.Count > 0 );
        while ( true )
        {
            yield return WaitEnqueue;

            var slider = sliderMissQueue.Peek();
            if ( slider.TailPos < -640f )
            {
                slider.Despawn();
                sliderMissQueue.Dequeue();
            }
        }
    }

    private void SelectNextNote( bool _isDespawn = true )
    {
        if ( _isDespawn )
        {
            curNote.gameObject.SetActive( false );
            curNote.Despawn();
        }

        curNote = null;
    }

    //private void AutoCheckNote()
    //{
    //    if ( curNote == null ) return;

    //    double startDiff = curNote.Time       - NowPlaying.Playback;
    //    double endDiff   = curNote.SliderTime - NowPlaying.Playback;

    //    if ( !curNote.IsSlider )
    //    {
    //        if ( startDiff <= ( GameSetting.IsAutoRandom ? target : 0d ) )
    //        {
    //            target = UnityEngine.Random.Range( -( float )Judgement.Bad, ( float )Judgement.Bad );

    //            OnInputEvent?.Invoke( KeyState.Down );
    //            OnInputEvent?.Invoke( KeyState.Up );

    //            OnHitNote?.Invoke( NoteType.Default, KeyState.Down );
    //            judge.ResultUpdate( GameSetting.IsAutoRandom ? target : 0d, NoteType.Default );
    //            AudioManager.Inst.Play( curSound );
    //            //if ( GameSetting.UseClapSound )
    //            //     AudioManager.Inst.Play( SFX.Clap );
    //            SelectNextNote();
    //        }
    //    }
    //    else
    //    {
    //        if ( !curNote.IsHolding )
    //        {
    //            if ( startDiff < 0d )
    //            {
    //                OnInputEvent?.Invoke( KeyState.Down );

    //                curNote.IsHolding = true;
    //                OnHitNote?.Invoke( NoteType.Slider, KeyState.Down );
    //                AudioManager.Inst.Play( curSound );
    //                //if ( GameSetting.UseClapSound )
    //                //    AudioManager.Inst.Play( SFX.Clap );

    //                judge.ResultUpdate( 0d, NoteType.Default );

    //                inputStartTime = NowPlaying.Playback;
    //            }
    //        }
    //        else
    //        {
    //            if ( endDiff < 0d )
    //            {
    //                OnInputEvent?.Invoke( KeyState.Up );

    //                OnHitNote?.Invoke( NoteType.Slider, KeyState.Up );
    //                judge.ResultUpdate( 0d, NoteType.Slider );
    //                SelectNextNote();
    //            }

    //            inputHoldTime = NowPlaying.Playback - inputStartTime;
    //            if ( inputHoldTime > .1f )
    //            {
    //                judge.ResultUpdate( HitResult.None, NoteType.None );
    //                inputStartTime = NowPlaying.Playback - ( inputHoldTime - .1f );
    //            }
    //        }
    //    }
    //}


    #endregion
}
