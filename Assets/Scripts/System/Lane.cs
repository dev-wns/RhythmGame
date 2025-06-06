using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lane : MonoBehaviour
{
    private Judgement judge;

    [Header( "Input Key" )]
    public int     Key  { get; private set; } // Lane Index
    public int     VKey { get; private set; } // Virtual KeyCode
    public KeyCode UKey { get; private set; } // Unity KeyCode

    [Header( "Note" )]
    public NoteRenderer note1 /* Lane 0,2,3,5 */, note2 /* Lane 1,4 */, noteMedian;
    private ObjectPool<NoteRenderer> notePool;
    private Queue<NoteRenderer>      notes            = new ();
    private Queue<NoteRenderer>      sliderMissQueue  = new ();
    private Queue<NoteRenderer>      sliderEarlyQueue = new ();
    private Queue<InputData>         dataQueue        = new ();

    private List<Note>   noteDatas = new ();
    private Note         spawnData;
    private NoteRenderer curNote;
    private int          dataIndex;

    [Header( "Lane Effect" )]
    private readonly float LaneEffectOffset = 1f / .15f;
    public  SpriteRenderer laneRenderer;
    private Color          laneColor;
    private float          laneAlpha;

    [Header( "Hit Effect" )]
    public SpriteRenderer  hitRenderer;
    public List<Sprite>    spritesN = new ();
    public List<Sprite>    spritesL = new ();
    private KeyState       inputState;
    private NoteType       noteType;
    private float          offsetN;
    private float          offsetL;
    private int            hitEffectIndex;
    private float          hitEffectTimer;


    private void Awake()
    {
        NowPlaying.OnPreUpdate  += PreUpdate;
        NowPlaying.OnPostUpdate += PostUpdate;

        //InGame scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        //scene.OnGameStart += GameStart;
        //scene.OnGameOver  += GameOver;
        //scene.OnReLoad    += ReLoad;
        //scene.OnPause     += Pause;

        // InputSys = GetComponent<InputSystem>();
        //judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
    }

    private void Update()
    {
        // Input Lock으로 인한 간섭이 없어야함
        UpdateHitEffect();
    }

    private void OnDestroy()
    {
        NowPlaying.OnPreUpdate  -= PreUpdate;
        NowPlaying.OnPostUpdate -= PostUpdate;
    }

    #region Initialize
    public void AddData( in InputData _data ) => dataQueue.Enqueue( _data );

    public void Initialize( int _lane, List<Note> _datas )
    {
        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        noteDatas = _datas;
        spawnData = noteDatas.Count > 0 ? noteDatas[dataIndex] : new Note();

        Key  = _lane;
        UKey = InputManager.Keys[( GameKeyCount )NowPlaying.KeyCount][Key];
        VKey = InputManager.GetVirtualKey( UKey );

        NoteRenderer note = note1;
        if (      NowPlaying.KeyCount == 4 ) note = _lane == 1 || _lane == 2 ? note2 : note1;
        else if ( NowPlaying.KeyCount == 6 ) note = _lane == 1 || _lane == 4 ? note2 : note1;
        else if ( NowPlaying.KeyCount == 7 ) note = _lane == 1 || _lane == 5 ? note2 : _lane == 3 ? noteMedian : note1;
        notePool ??= new ObjectPool<NoteRenderer>( note, 5 );

        // 인덱스에 맞는 위치에 레인 배치
        transform.position = new Vector3( GameSetting.NoteStartPos + ( GameSetting.NoteWidth * Key ) + ( GameSetting.NoteBlank * Key ) + GameSetting.NoteBlank, GameSetting.JudgePos, 0f );
        if ( GameSetting.HasFlag( VisualFlag.LaneEffect ) )
        {
            laneRenderer.transform.position   = new Vector3( transform.position.x, GameSetting.JudgePos, transform.position.z );
            laneRenderer.transform.localScale = new Vector3( GameSetting.NoteWidth, 250f, 1f );
            
            // 레인 색상 선택
            if      ( NowPlaying.KeyCount == 4 ) laneColor = Key == 1 || Key == 2 ? Color.blue   : Color.red;
            else if ( NowPlaying.KeyCount == 6 ) laneColor = Key == 1 || Key == 4 ? Color.blue   : Color.red;
            else if ( NowPlaying.KeyCount == 7 ) laneColor = Key == 1 || Key == 5 ? Color.blue   :
                                                             Key == 3             ? Color.yellow : Color.red;
        }

        // 타격 이펙트 세팅
        if ( GameSetting.HasFlag( VisualFlag.HitEffect ) )
        {
            offsetN = .16f / spritesN.Count;
            offsetL = .16f / spritesL.Count;
            hitRenderer.transform.localScale = new Vector2( GameSetting.NoteWidth * 2, GameSetting.NoteWidth * 2 );
            hitRenderer.color = Color.clear;
        }
    }
    #endregion

    #region Update
    private void PreUpdate()
    {
        UpdateLaneEffect();
        SpawnNote();
    }

    private void PostUpdate()
    {
        CheckHitData();
        CheckSliderDespawn();
    }

    private void CheckHitData()
    {
        if ( curNote == null && notes.Count > 0 )
             curNote = notes.Dequeue();

        // 판정 처리 ( Virtual Key 사용 )
        if ( curNote == null || !dataQueue.TryDequeue( out InputData data ) )
             return;

        switch ( data.keyState )
        {
            case KeyState.Down:
            {
                if ( data.noteState == NoteState.Hit )
                {
                    HitEffect( curNote.IsSlider ? NoteType.Slider : NoteType.Default, KeyState.Down );
                    judge.ResultUpdate( data.diff, NoteType.Default );

                    if ( !curNote.IsSlider )
                          SelectNextNote();
                }
            } break;

            case KeyState.Up:
            {
                if ( data.noteState == NoteState.Hit )
                {
                    HitEffect( NoteType.Slider, KeyState.Up );
                    sliderEarlyQueue.Enqueue( curNote );

                    judge.ResultUpdate( data.diff, NoteType.Slider );
                    SelectNextNote( false );
                }
                else if ( data.noteState == NoteState.Miss )
                {
                    HitEffect( NoteType.Slider, KeyState.Up );
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
                    HitEffect( NoteType.Slider, KeyState.Up );

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

    private void SpawnNote()
    {
        if ( dataIndex >= noteDatas.Count )
             return;

        if ( spawnData.noteDistance <= NowPlaying.Distance + GameSetting.MinDistance )
        {
            NoteRenderer note = notePool.Spawn();
            note.SetInfo( Key, in spawnData );
            notes.Enqueue( note );

            if ( ++dataIndex < noteDatas.Count )
                 spawnData = noteDatas[dataIndex];
        }
    }

    private void UpdateLaneEffect()
    {
        if ( GameSetting.HasFlag( VisualFlag.LaneEffect ) )
             return;

        float increment = LaneEffectOffset * Time.deltaTime;
        laneAlpha = Input.GetKey( UKey ) ? Global.Math.Clamp( laneAlpha + increment, 0f, 1f ) :
                                           Global.Math.Clamp( laneAlpha - increment, 0f, 1f );

        if ( Global.Math.Abs( laneAlpha - laneRenderer.color.a ) > float.Epsilon )
             laneRenderer.color = new Color( laneColor.r, laneColor.g, laneColor.b, laneAlpha );
    }

    private void UpdateHitEffect()
    {
        hitEffectTimer += Time.deltaTime;

        if ( noteType == NoteType.Default &&
             hitEffectTimer > hitEffectIndex * offsetN )
        {
            if ( hitEffectIndex < spritesN.Count )
            {
                hitRenderer.sprite = spritesN[hitEffectIndex];
                hitEffectIndex += 1;
            }
            else
            {
                hitRenderer.color = Color.clear;
            }
        }
        else if ( noteType == NoteType.Slider &&
                  hitEffectTimer > hitEffectIndex * offsetL )
        {
            if ( hitEffectIndex < spritesL.Count )
            {
                hitRenderer.sprite = spritesL[hitEffectIndex];
                hitEffectIndex += 1;

                if ( hitEffectIndex >= spritesL.Count )
                {
                    if ( inputState == KeyState.Down )
                    {
                        hitEffectTimer = 0f;
                        hitEffectIndex = 0;
                    }
                    else
                        hitRenderer.color = Color.clear;
                }
            }
        }
    }

    public void HitEffect( NoteType _noteType, KeyState _inputState )
    {
        noteType   = _noteType;
        inputState = _inputState;
        if ( _inputState == KeyState.Down )
        {
            hitEffectTimer = 0f;
            hitEffectIndex = 0;
            hitRenderer.color = Color.white;
        }
    }

    public void CheckSliderDespawn()
    {
        // 일찍 처리된 롱노트 판정선에 닿을 때 디스폰
        if ( sliderEarlyQueue.Count > 0 )
        {
            NoteRenderer slider = sliderEarlyQueue.Peek();
            if ( slider.SliderTime < NowPlaying.Playback )
            {
                slider.Despawn();
                sliderEarlyQueue.Dequeue();
            }
        }

        // 미스처리된 롱노트 화면 밖에서 디스폰 
        if ( sliderMissQueue.Count > 0 )
        {
            NoteRenderer slider = sliderMissQueue.Peek();
            if ( slider.TailPos < -( ( Global.Screen.Height * .5f ) + 100f ) )
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
    #endregion

    private void ReLoad()
    {
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

        DataStorage.Inst.Clear();
        dataIndex = 0;
        spawnData = new Note();
        curNote   = null;
    }

    private void GameOver()
    {
        laneRenderer.color = new Color( laneColor.r, laneColor.g, laneColor.b, laneAlpha = 0f );
    }

    private void Pause( bool _isPause )
    {
        laneRenderer.color = new Color( laneColor.r, laneColor.g, laneColor.b, laneAlpha = 0f );
        if ( !_isPause || curNote == null || !curNote.IsSlider )
             return;

        if ( GameSetting.HasFlag( GameMode.AutoPlay ) )
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
