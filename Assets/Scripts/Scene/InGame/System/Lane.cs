using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lane : MonoBehaviour
{
    [Header( "Input Key" )]
    public int     Key  { get; private set; } // Lane Index
    public int     VKey { get; private set; } // Virtual KeyCode
    public KeyCode UKey { get; private set; } // Unity KeyCode

    [Header( "Note" )]
    public NoteRenderer note1 /* Lane 0,2,3,5 */, note2 /* Lane 1,4 */, noteMedian;
    private ObjectPool<NoteRenderer> notePool;
    private Queue<NoteRenderer> notes            = new ();
    private Queue<NoteRenderer> sliderMissQueue  = new ();
    private Queue<NoteRenderer> sliderEarlyQueue = new ();
    private Queue<HitData>      dataQueue        = new ();

    private List<Note>   noteDatas = new ();
    private Note         spawnData;
    private NoteRenderer curNote;
    private int          spawnIndex;

    [Header( "Lane Effect" )]
    private readonly float LaneOffset = 1f / .15f;
    public  SpriteRenderer laneRenderer;
    private Color          laneColor;
    private float          laneAlpha;

    [Header( "Hit Effect" )]
    private KeyState       keyState;
    public SpriteRenderer  hitRenderer;
    public List<Sprite>    hitSprites = new ();
    private float          hitOffset;
    private int            hitIndex;
    private float          hitTimer;
    private bool           isHitLoop;


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

    private void Update()
    {
        // Input Lock 여부와 상관없이 재생
        UpdateHitEffect();

        if ( !NowPlaying.IsStart )
             return;

        UpdateLaneEffect();
        SpawnNote();
        CheckHitData();
        CheckSliderDespawn();
    }

    #region Initialize
    public void AddData( in HitData _data ) => dataQueue.Enqueue( _data );

    public void Initialize( int _lane, List<Note> _datas )
    {
        noteDatas  = _datas;
        spawnIndex = 0;
        spawnData  = noteDatas.Count > 0 ? noteDatas[spawnIndex] : new Note();

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
            hitOffset = .16f / hitSprites.Count;
            hitRenderer.transform.localScale = new Vector2( GameSetting.NoteWidth * 2, GameSetting.NoteWidth * 2 );
            hitRenderer.color = Color.clear;
        }
    }
    #endregion

    #region Update
    private void CheckHitData()
    {
        if ( curNote == null && notes.Count > 0 )
             curNote = notes.Dequeue();

        // 판정 처리 ( Virtual Key 사용 )
        if ( curNote == null || !dataQueue.TryDequeue( out HitData data ) )
             return;

        keyState = data.keyState;
        if ( data.hitResult >= 0 )
        {
            HitEffect( curNote.IsSlider );

            if ( !curNote.IsKeyDown )
            {
                if ( curNote.IsSlider ) curNote.IsKeyDown = true;
                else                    SelectNextNote();
            }
            else
            {
                sliderEarlyQueue.Enqueue( curNote );
                SelectNextNote( false );
            }
        }
        else
        {
            HitEffect();
            if ( !curNote.IsSlider )
            {
                SelectNextNote();
            }
            else
            {
                curNote.SetSliderFail();
                sliderMissQueue.Enqueue( curNote );
                SelectNextNote( false );
            }
        }

        //switch ( data.keyState )
        //{
        //    case KeyState.Down:
        //    {
        //        Debug.Log( $"Down : {data.hitResult}" );
        //        if ( data.hitResult >= 0 ) // Hit
        //        {

        //            if ( curNote.IsSlider ) curNote.IsKeyDown = true;
        //            else                    SelectNextNote();
        //        }
        //        else if ( data.hitResult < 0 ) // Miss
        //             SelectNextNote();

        //    } break;

        //    case KeyState.Up:
        //    {
        //        Debug.Log( $"Up : {data.hitResult}" );

        //        HitEffect( NoteType.Slider, KeyState.Up );
        //        if ( data.hitResult >= 0 ) // Hit
        //        {
        //            sliderEarlyQueue.Enqueue( curNote );
        //            SelectNextNote( false );
        //        }
        //        else if ( data.hitResult < 0 ) // Miss
        //        {
        //            curNote.SetSliderFail();
        //            sliderMissQueue.Enqueue( curNote );
        //            SelectNextNote( false );
        //        }
        //    } break;

        //    case KeyState.None:
        //    {
        //        //Debug.Log( $"None : {data.hitResult}" );

        //        HitEffect( NoteType.Slider, KeyState.Up );
        //        if ( !curNote.IsSlider )
        //        {
        //            SelectNextNote();
        //        }
        //        else
        //        {
        //            curNote.SetSliderFail();
        //            sliderMissQueue.Enqueue( curNote );
        //            SelectNextNote( false );
        //        }
        //    } break;
        //}

    }

    private void SpawnNote()
    {
        if ( spawnIndex >= noteDatas.Count )
             return;

        if ( spawnData.distance <= NowPlaying.Distance + GameSetting.MinDistance )
        {
            NoteRenderer note = notePool.Spawn();
            note.SetInfo( Key, in spawnData );
            notes.Enqueue( note );

            if ( ++spawnIndex < noteDatas.Count )
                   spawnData = noteDatas[spawnIndex];
        }
    }

    private void UpdateLaneEffect()
    {
        if ( !GameSetting.HasFlag( VisualFlag.LaneEffect ) )
             return;

        float increment = LaneOffset * Time.deltaTime;
        laneAlpha = Input.GetKey( UKey ) ? Global.Math.Clamp( laneAlpha + increment, 0f, 1f ) :
                                           Global.Math.Clamp( laneAlpha - increment, 0f, 1f );

        if ( Global.Math.Abs( laneAlpha - laneRenderer.color.a ) > float.Epsilon )
             laneRenderer.color = new Color( laneColor.r, laneColor.g, laneColor.b, laneAlpha );
    }

    private void UpdateHitEffect()
    {
        hitTimer += Time.deltaTime;

        if ( !isHitLoop )
        {
            if ( hitTimer > hitIndex * hitOffset )
            {
                if ( hitIndex < hitSprites.Count )
                     hitRenderer.sprite = hitSprites[hitIndex++];
                else
                     hitRenderer.color = Color.clear;
            }
        }
        else
        {
            if ( hitTimer > hitIndex * hitOffset )
            {
                if ( hitIndex < hitSprites.Count )
                {
                    hitRenderer.sprite = hitSprites[hitIndex++];
                }
                else
                {
                    hitTimer = 0f;
                    hitIndex = 0;
                }
            }
        }
    }

    public void HitEffect( bool _isLoop = false )
    {
        isHitLoop = _isLoop;

        if ( keyState == KeyState.Down )
        {
            hitTimer = 0f;
            hitIndex = 0;
            hitRenderer.color = Color.white;
        }

        if ( keyState == KeyState.Up )
             hitRenderer.color = Color.clear;
    }

    public void CheckSliderDespawn()
    {
        // 일찍 처리된 롱노트 판정선에 닿을 때 디스폰
        if ( sliderEarlyQueue.TryPeek( out NoteRenderer earlySlider ) )
        {
            if ( earlySlider.EndTime < NowPlaying.Playback )
            {
                earlySlider.Despawn();
                sliderEarlyQueue.Dequeue();
            }
        }

        // 미스처리된 롱노트 화면 밖에서 디스폰 
        if ( sliderMissQueue.TryPeek( out NoteRenderer missSlider ) )
        {
            //Debug.Log( $"MissSlider : {missSlider.TailPos} {-( ( Global.Screen.Height * .5f ) + 100f)}" );
            if ( missSlider.TailPos < -( ( Global.Screen.Height * .5f ) + 100f ) )
            {
                missSlider.Despawn();
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

        spawnIndex = 0;
        spawnData  = new Note();
        curNote    = null;
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
            //judge.ResultUpdate( HitResult.Perfect, NoteType.Slider );
            SelectNextNote();
        }
        else
        {
            curNote.SetSliderFail();
            //judge.ResultUpdate( HitResult.Miss, NoteType.Slider );
            sliderMissQueue.Enqueue( curNote );
            SelectNextNote( false );
        }
    }
}
