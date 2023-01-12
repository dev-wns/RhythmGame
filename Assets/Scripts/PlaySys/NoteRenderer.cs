using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteRenderer : MonoBehaviour, IObjectPool<NoteRenderer>
{
    public ObjectPool<NoteRenderer> pool { get; set; }

    public SpriteRenderer head, body, tail;
    private Transform headTf, bodyTf, tailTf;
    private Note note;
    public int ID { get; private set; }
    public float HeadPos => transform.position.y;
    public float TailPos => transform.position.y + BodyLength;
    public double Time => note.time;
    public double CalcTime => note.calcTime;
    public double SliderTime => note.sliderTime;
    public double CalcSliderTime => note.calcSliderTime;
    public bool IsSlider => note.isSlider;
    public bool ShouldResizeSlider { get; set; }
    public float BodyLength { get; private set; }
    public KeySound Sound => note.keySound;

    private float column;
    private static readonly Color NoteFailColor = new Color( .25f, .25f, .25f, 1f );
    private double newTime;

    private void Awake()
    {
        headTf = head.transform;
        headTf.localScale = new Vector2( GameSetting.NoteWidth, GameSetting.NoteHeight );

        bodyTf = body.transform;
        bodyTf.transform.position = new Vector2( 0, GameSetting.NoteHeight * .5f );

        tailTf = tail.transform;
        tailTf.localScale = new Vector2( GameSetting.NoteWidth, GameSetting.NoteHeight );
    }

    public void SetInfo( int _lane, in Note _note, int _id )
    {
        //system  = _system;
        ID = _id;
        ShouldResizeSlider = false;
        note      = _note;

        column = GameSetting.NoteStartPos + ( _lane * GameSetting.NoteWidth ) + ( ( _lane + 1 ) * GameSetting.NoteBlank );
        newTime = note.calcTime;

        body.enabled = tail.enabled = IsSlider;
        head.color   = tail.color   = Color.white;
        body.color   = GameSetting.IsNoteBodyGray ? Color.gray : Color.white;
    }

    public void SetBodyFail() => head.color = body.color = tail.color = NoteFailColor;

    public void Despawn()
    {
        ShouldResizeSlider = false;
        pool.Despawn( this );
    }

    private void LateUpdate()
    {
       // 롱노트 판정선에 붙기
        if ( IsSlider )
        {
            if ( ShouldResizeSlider && Time < NowPlaying.Playback )
                 newTime = NowPlaying.PlaybackInBPM;

            BodyLength = ( float )( ( CalcSliderTime - newTime ) * GameSetting.Weight ) - GameSetting.NoteHeight;
            bodyTf.localScale = BodyLength < 0 ? new Vector2( GameSetting.NoteBodyWidth, 0f ) :
                                                 new Vector2( GameSetting.NoteBodyWidth, BodyLength );

            tailTf.localPosition = BodyLength < 0 ? Vector2.zero : new Vector2( 0f, BodyLength );
        }

        transform.position = new Vector2( column, GameSetting.JudgePos + ( float )( ( newTime - NowPlaying.PlaybackInBPM ) * GameSetting.Weight ) );
    }
}
