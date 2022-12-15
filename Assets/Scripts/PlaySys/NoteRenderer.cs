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
        bodyTf.transform.position = new Vector2( 0, GameSetting.NoteWidth * .5f );

        // tailTf = tail.transform;
        // tailTf.localScale = new Vector2( GameSetting.NoteWidth, GameSetting.NoteHeight );
        // tail.enabled = false;
        tail.gameObject.SetActive( false );

    }

    public void SetInfo( int _lane, NoteSystem _system, in Note _note )
    {
        system  = _system;
        note    = _note;

        column = GameSetting.NoteStartPos + ( _lane * GameSetting.NoteWidth ) + ( ( _lane + 1 ) * GameSetting.NoteBlank );
        newTime = note.calcTime;

        body.enabled = IsSlider;
        head.color = tail.color = Color.white;
        body.color = Color.gray;
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
                 newTime = NowPlaying.PlaybackChanged;

            double bodyLength = ( ( CalcSliderTime - newTime ) * GameSetting.Weight ) - ( GameSetting.NoteWidth * .5f );
            bodyTf.localScale = bodyLength < 0 ? new Vector2( GameSetting.NoteBodyWidth, 0f ) :
                                                 new Vector2( GameSetting.NoteBodyWidth, ( float )bodyLength );

            //var tailPos = ( float )bodyLength - ( GameSetting.NoteHeight * BodyPositionOffset );
            //tailTf.localPosition = tailPos < GameSetting.NoteHeight * BodyPositionOffset ? new Vector2( 0f, GameSetting.NoteHeight * BodyPositionOffset ) :
            //                                                                               new Vector2( 0f, tailPos );
        }

        transform.localPosition = new Vector2( column, GameSetting.JudgePos + ( float )( ( newTime - NowPlaying.PlaybackChanged ) * GameSetting.Weight ) );
    }
}
