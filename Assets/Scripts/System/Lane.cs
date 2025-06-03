using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Lane : MonoBehaviour
{
    private Judgement judge;
    public HitEffectSystem hitEffect;

    public int     Key  { get; private set; } // Lane Index
    public int     VKey { get; private set; } // Virtual KeyCode
    public KeyCode UKey { get; private set; } // Unity KeyCode
    //public InputSystem InputSys { get; private set; }

    public event Action<int/*Lane Key*/> OnLaneInitialize;

    #region Note
    [Header( "Note" )]
    public NoteRenderer note1 /* Lane 0,2,3,5 */, note2 /* Lane 1,4 */, noteMedian;
    private ObjectPool<NoteRenderer> notePool;
    private Queue<NoteRenderer>      notes            = new Queue<NoteRenderer>();
    private Queue<NoteRenderer>      sliderMissQueue  = new Queue<NoteRenderer>();
    private Queue<NoteRenderer>      sliderEarlyQueue = new Queue<NoteRenderer>();
    private Queue<InputData>         inputQueue       = new ();

    private List<Note>   noteDatas = new List<Note>();
    private NoteRenderer curNote;

    public event Action<NoteType, KeyState> OnHitNote;
    public event Action OnStopEffect;
    #endregion

    #region Effect
    [Header( "Lane Effect" )]
    public SpriteRenderer sprite;
    private Color color;
    private float alpha;
    private readonly float LaneEffectOffset = 1f / .15f;

    [Header( "Hit Effect" )]
    private int adsf;

    #endregion

    private void Awake()
    {
        //InGame scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        //scene.OnGameStart += GameStart;
        //scene.OnGameOver  += GameOver;
        //scene.OnReLoad    += ReLoad;
        //scene.OnPause     += Pause;

        // InputSys = GetComponent<InputSystem>();
        //judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
    }
    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private void Update()
    {
        if ( !NowPlaying.IsStart && ( GameSetting.CurrentVisualFlag & VisualFlag.LaneEffect ) == 0 )
              return;

        float increment = LaneEffectOffset * Time.deltaTime;
        alpha = Input.GetKey( UKey ) ? Global.Math.Clamp( alpha + increment, 0f, 1f ) :
                                       Global.Math.Clamp( alpha - increment, 0f, 1f );

        if ( Global.Math.Abs( alpha - sprite.color.a ) > float.Epsilon )
             sprite.color = new Color( color.r, color.g, color.b, alpha );
    }
    
    private void LateUpdate()
    {
        return;

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
            } break;

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
            } break;

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
            } break;
        }
    }

    public void AddNote( in Note _note )
    {
        noteDatas.Add( _note );
    }

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

    public void Initialize( int _lane )
    {
        Key  = _lane;
        UKey = KeySetting.Inst.Keys[( GameKeyCount )NowPlaying.KeyCount][Key];
        VKey = KeySetting.Inst.GetVirtualKey( UKey );

        NoteRenderer note = note1;
        if (      NowPlaying.KeyCount == 4 ) note = _lane == 1 || _lane == 2 ? note2 : note1;
        else if ( NowPlaying.KeyCount == 6 ) note = _lane == 1 || _lane == 4 ? note2 : note1;
        else if ( NowPlaying.KeyCount == 7 ) note = _lane == 1 || _lane == 5 ? note2 : _lane == 3 ? noteMedian : note1;
        notePool ??= new ObjectPool<NoteRenderer>( note, 5 );
        // InputManager.Inst.Connect( this );

        OnLaneInitialize?.Invoke( Key );

        // 인덱스에 맞는 위치에 레인 배치
        transform.position = new Vector3( GameSetting.NoteStartPos + ( GameSetting.NoteWidth * Key ) + ( GameSetting.NoteBlank * Key ) + GameSetting.NoteBlank, GameSetting.JudgePos, 0f );
        if ( GameSetting.CurrentVisualFlag.HasFlag( VisualFlag.LaneEffect ) )
        {
            sprite.transform.position   = new Vector3( transform.position.x, GameSetting.JudgePos, transform.position.z );
            sprite.transform.localScale = new Vector3( GameSetting.NoteWidth, 250f, 1f );
            
            // 레인 색상 선택
            if      ( NowPlaying.KeyCount == 4 ) color = Key == 1 || Key == 2 ? Color.blue   : Color.red;
            else if ( NowPlaying.KeyCount == 6 ) color = Key == 1 || Key == 4 ? Color.blue   : Color.red;
            else if ( NowPlaying.KeyCount == 7 ) color = Key == 1 || Key == 5 ? Color.blue   :
                                                         Key == 3             ? Color.yellow : Color.red;
        }
        else
        {
            sprite.gameObject.SetActive( false );
        }
    }

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
        curNote = null;
    }

    private IEnumerator SpawnNotes()
    {
        int index = 0;
        Note data = noteDatas.Count > 0 ? noteDatas[index] : new Note();
        WaitUntil waitSpawnTime = new WaitUntil( () => data.noteDistance <= NowPlaying.Distance + GameSetting.MinDistance );
        while ( true )
        {
            if ( index > noteDatas.Count )
                yield break;

            yield return waitSpawnTime;

            NoteRenderer note = notePool.Spawn();
            note.SetInfo( Key, in data );
            notes.Enqueue( note );

            if ( ++index < noteDatas.Count )
                data = noteDatas[index];
        }
    }

    private void GameStart()
    {
        StartCoroutine( SpawnNotes() );
        StartCoroutine( SliderMissCheck() );
        StartCoroutine( SliderEarlyCheck() );
    }

    private void GameOver()
    {
        OnStopEffect?.Invoke();
        sprite.color = new Color( color.r, color.g, color.b, alpha = 0f );
    }

    private void Pause( bool _isPause )
    {
        OnStopEffect?.Invoke();
        sprite.color = new Color( color.r, color.g, color.b, alpha = 0f );
        if ( !_isPause || curNote == null || !curNote.IsSlider )
             return;

        if ( GameSetting.CurrentGameMode.HasFlag( GameMode.AutoPlay ) )
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
}
