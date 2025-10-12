using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public  Queue<HitData>      dataQueue        = new ();
    private NoteRenderer curNote;
    private int          spawnIndex;

    [Header( "Lane Effect" )]
    private readonly float LaneOffset = 1f / .1f;
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

    [Header( "Input Effect" )]
    public SpriteRenderer inputRenderer;

    private void Awake()
    {
        ClearEffect();

        NowPlaying.OnClear     += Clear;
        NowPlaying.OnGameOver  += GameOver;
        NowPlaying.OnPause     += Pause;
    }

    private void OnDestroy()
    {
        NowPlaying.OnClear     -= Clear;
        NowPlaying.OnGameOver  -= GameOver;
        NowPlaying.OnPause     -= Pause;
    }


    private void Update()
    {
        // 노트 스폰 후 데이터 체크
        if ( NowPlaying.IsStart )
        {
            SpawnNotes();
            CheckHitData();

            if      ( Input.GetKeyDown( UKey ) ) inputRenderer.color = Color.gray;
            else if ( Input.GetKeyUp(   UKey ) ) inputRenderer.color = Color.clear;

            if ( GameSetting.HasFlag( VisualFlag.LaneEffect ) )
            {
                float increment = LaneOffset * Time.deltaTime;
                laneAlpha = Input.GetKey( UKey ) ? Global.Math.Clamp( laneAlpha + increment, 0f, 1f ) :
                                                   Global.Math.Clamp( laneAlpha - increment, 0f, 1f );

                if ( Global.Math.Abs( laneAlpha - laneRenderer.color.a ) > float.Epsilon )
                     laneRenderer.color = new Color( laneColor.r, laneColor.g, laneColor.b, laneAlpha );
            }

            UpdateHitEffect();
        }

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
            if ( missSlider.TailPos < -( ( Global.Screen.Height * .5f ) + 100f ) )
            {
                missSlider.Despawn();
                sliderMissQueue.Dequeue();
            }
        }
    }

    #region Initialize
    public void AddData( in HitData _data ) => dataQueue.Enqueue( _data );

    public void Initialize( int _lane )
    {
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
    //private void CheckHitData()
    //{
    //    if ( curNote == null && notes.Count > 0 )
    //         curNote = notes.Dequeue();

    //    // 판정 처리 ( Virtual Key 사용 )
    //    if ( curNote == null || !dataQueue.TryDequeue( out HitData data ) )
    //         return;

    //    keyState = data.keyState;
    //    if ( data.hitResult >= 0 )
    //    {
    //        HitEffect( curNote.IsSlider );

    //        if ( !curNote.IsKeyDown )
    //        {
    //            if ( curNote.IsSlider ) curNote.IsKeyDown = true;
    //            else                    SelectNextNote();
    //        }
    //        else
    //        {
    //            sliderEarlyQueue.Enqueue( curNote );
    //            SelectNextNote( false );
    //        }
    //    }
    //    else
    //    {
    //        HitEffect();
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
    //    }
    //}

    private void CheckHitData()
    {
        if ( curNote == null && notes.Count > 0 )
             curNote = notes.Dequeue();

        // 판정 처리 ( Virtual Key 사용 )
        if ( curNote == null || !dataQueue.TryDequeue( out HitData data )  )
             return;


        if ( !( Global.Math.Abs( curNote.data.time    - data.note.time    ) <= double.Epsilon &&
                Global.Math.Abs( curNote.data.endTime - data.note.endTime ) <= double.Epsilon ) )
        {
            Debug.LogError( $"{data.keyState}  {curNote.data.time}  {data.note.time}  {data.time}" );
        }

        keyState = data.keyState;
        if ( data.hitResult >= 0 ) // Hit
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
        else // Miss
        {
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
    }

    private void SpawnNotes()
    {
        if ( spawnIndex < NowPlaying.Notes[Key].Count )
        {
            Note current = NowPlaying.Notes[Key][spawnIndex];
            if ( current.distance <= NowPlaying.Distance + GameSetting.MinDistance )
            {
                NoteRenderer note = notePool.Spawn();
                note.SetInfo( Key, in current );
                notes.Enqueue( note );

                spawnIndex++;
            }
        }
    }

    private void UpdateHitEffect()
    {
        if ( !GameSetting.HasFlag( VisualFlag.HitEffect ) )
             return;

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
        if ( !GameSetting.HasFlag( VisualFlag.HitEffect ) )
             return;

        isHitLoop = _isLoop;
        if ( keyState == KeyState.Down )
        {
            hitTimer = 0f;
            hitIndex = 0;
            hitRenderer.color = Color.white;
        }
    }

    private void SelectNextNote( bool _isDespawn = true )
    {
        if ( _isDespawn )
        {
            curNote.gameObject.SetActive( false );
            curNote.Despawn();
        }

        isHitLoop = false;
        curNote   = null;
    }
    #endregion

    private void Clear()
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

        dataQueue.Clear();
        notes.Clear();
        notePool.AllDespawn();

        spawnIndex = 0;
        isHitLoop  = false;
        curNote    = null;
    }

    private void GameOver()
    {
        isHitLoop     = false;
        ClearEffect();

        if ( curNote != null )
             curNote.IsKeyDown = false;
    }

    private void ClearEffect()
    {
        inputRenderer.color = Color.clear;
        hitRenderer.color   = Color.clear;
        laneRenderer.color  = new Color( laneColor.r, laneColor.g, laneColor.b, 0f );
    }

    private void Pause( bool _isPause )
    {
        ClearEffect();
        //laneRenderer.color = new Color( laneColor.r, laneColor.g, laneColor.b, laneAlpha = 0f );
        //if ( !_isPause || curNote == null || !curNote.IsSlider )
        //     return;

        //if ( GameSetting.HasFlag( GameMode.AutoPlay ) )
        //{
        //    //judge.ResultUpdate( HitResult.Perfect, NoteType.Slider );
        //    SelectNextNote();
        //}
        //else
        //{
        //    curNote.SetSliderFail();
        //    //judge.ResultUpdate( HitResult.Miss, NoteType.Slider );
        //    sliderMissQueue.Enqueue( curNote );
        //    SelectNextNote( false );
        //}
    }
}
