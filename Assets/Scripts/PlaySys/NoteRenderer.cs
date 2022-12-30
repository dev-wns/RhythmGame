using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteRenderer : MonoBehaviour
{
    private NoteSystem system;

    public SpriteRenderer head, body, tail;
    private Transform headTf, bodyTf, tailTf;
    private Note note;

    public double Time => note.time;
    public double CalcTime => note.calcTime;
    public double SliderTime => note.sliderTime;
    public double CalcSliderTime => note.calcSliderTime;
    public bool IsSlider => note.isSlider;
    public bool IsPressed { get; set; }
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

    public void SetInfo( int _lane, NoteSystem _system, in Note _note )
    {
        system  = _system;
        note    = _note;

        column = GameSetting.NoteStartPos + ( _lane * GameSetting.NoteWidth ) + ( ( _lane + 1 ) * GameSetting.NoteBlank );
        newTime = note.calcTime;

        body.enabled = tail.enabled = IsSlider;
        head.color   = tail.color   = Color.white;
        //body.color = Color.gray;
        body.color = Color.white;
    }

    public void SetBodyFail() => head.color = body.color = tail.color = NoteFailColor;

    public void Despawn()
    {
        IsPressed = false;
        system.Despawn( this );
    }

    private void LateUpdate()
    {
       // 롱노트 판정선에 붙기
        if ( IsSlider )
        {
            if ( IsPressed ) 
                 newTime = NowPlaying.PlaybackInBPM;

            double bodyLength = ( ( CalcSliderTime - newTime ) * GameSetting.Weight ) - GameSetting.NoteHeight;
            bodyTf.localScale = bodyLength < 0 ? new Vector2( GameSetting.NoteBodyWidth, 0f ) :
                                                 new Vector2( GameSetting.NoteBodyWidth, ( float )bodyLength );

            tailTf.localPosition = bodyLength < 0 ? Vector2.zero : new Vector2( 0f, ( float )( bodyLength ) );
        }

        transform.position = new Vector2( column, GameSetting.JudgePos + ( float )( ( newTime - NowPlaying.PlaybackInBPM ) * GameSetting.Weight ) );
    }
}
