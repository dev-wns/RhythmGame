using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteRenderer : MonoBehaviour
{
    private InGame game;
    private NoteSystem system;

    public SpriteRenderer head, body, tail;
    private Transform headTf, bodyTf, tailTf;

    public Sprite skinNormal, skinHead, skinBody, skinTail;
    private static readonly float BodyScaleOffset    = 256f / 128f; // PixelPerUnit  / TextureHeight
    private static readonly float BodyPositionOffset = 128f / 256f; // TextureHeight / PixelPerUnit
    private Note note;

    public double Time => note.time;
    public double CalcTime => note.calcTime;
    public double SliderTime => note.sliderTime;
    public double CalcSliderTime => note.calcSliderTime;
    public bool IsSlider => note.isSlider;
    public bool IsPressed { get; set; }
    public KeySound Sound => note.keySound;

    private float column;

    //private static readonly Color MiddleColor   = new Color( 0.2078432f, 0.7843138f, 1f, 1f );
    //private static readonly Color BodyColor     = new Color( .4f, .4f, .4f, 1f );
    private static readonly Color NoteFailColor = new Color( .25f, .25f, .25f, 1f );

    private double weight;
    private double newTime;

    private void Awake()
    {
        game = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        game.OnScrollChanged += ScrollUpdate;

        headTf = head.transform;
        headTf.localScale = new Vector2( GameSetting.NoteWidth, GameSetting.NoteHeight );

        bodyTf = body.transform;

        tailTf = tail.transform;
        tailTf.localScale = new Vector2( GameSetting.NoteWidth, GameSetting.NoteHeight );
    }

    private void OnDestroy()
    {
        game.OnScrollChanged -= ScrollUpdate;
    }

    public void SetInfo( int _lane, NoteSystem _system, in Note _note )
    {
        system  = _system;
        note    = _note;

        column = GameSetting.NoteStartPos + ( _lane * GameSetting.NoteWidth ) + ( ( _lane + 1 ) * GameSetting.NoteBlank );
        newTime = note.calcTime;

        if ( IsSlider )
        {
            body.enabled = tail.enabled = true;
            head.sprite = skinHead;
        }
        else
        {
            body.enabled = tail.enabled = false;
            head.sprite = skinNormal;
        }

        //body.enabled = tail.enabled = IsSlider ? true : false;
        ScrollUpdate();
        head.color = body.color = tail.color = Color.white;
    }

    public void SetBodyFail() => head.color = body.color = tail.color = NoteFailColor;

    private void ScrollUpdate()
    {
        weight = GameSetting.Weight;
        if ( !IsSlider ) return;

        double bodyLength = ( CalcSliderTime - CalcTime ) * weight;
        bodyTf.localPosition = new Vector2( 0f, GameSetting.NoteHeight * BodyPositionOffset );
        
        var bodyScale = ( float )( ( bodyLength * BodyScaleOffset ) - ( GameSetting.NoteHeight * 2f ) );
        bodyTf.localScale = bodyScale < 0 ? new Vector2( GameSetting.NoteWidth, 0f ) :
                                            new Vector2( GameSetting.NoteWidth, bodyScale );

        var tailPos = ( float )bodyLength - ( GameSetting.NoteHeight * BodyPositionOffset );
        tailTf.localPosition = tailPos < GameSetting.NoteHeight * .5f ? new Vector2( 0f, GameSetting.NoteHeight * .5f ) :
                                                                        new Vector2( 0f, tailPos );
    }

    public void Despawn()
    {
        IsPressed = false;
        system.Despawn( this );
    }

    private void LateUpdate()
    {
       // 롱노트 판정선에 붙기
        Vector2 headPos;
        if ( IsPressed )
        {
            if ( transform.position.y <= GameSetting.JudgePos )
                 newTime = NowPlaying.PlaybackChanged;

            headPos = new Vector2( column, GameSetting.JudgePos + ( float )( ( newTime - NowPlaying.PlaybackChanged ) * weight ) );

            double bodyLength = ( CalcSliderTime - newTime ) * weight;
            bodyTf.localPosition = new Vector2( 0f, GameSetting.NoteHeight * BodyPositionOffset );

            var bodyScale = ( float )( ( bodyLength * BodyScaleOffset ) - ( GameSetting.NoteHeight * 2f ) );
            bodyTf.localScale = bodyScale < 0 ? new Vector2( GameSetting.NoteWidth, 0f ) :
                                                new Vector2( GameSetting.NoteWidth, bodyScale );

            var tailPos = ( float )bodyLength - ( GameSetting.NoteHeight * BodyPositionOffset );
            tailTf.localPosition = tailPos < GameSetting.NoteHeight * .5f ? new Vector2( 0f, GameSetting.NoteHeight * .5f ) :
                                                                            new Vector2( 0f, tailPos );
        }
        else
        {
            headPos = new Vector2( column, GameSetting.JudgePos + ( float )( ( ( newTime - NowPlaying.PlaybackChanged ) * weight ) ) );
        }

        transform.localPosition = headPos;
    }
}
