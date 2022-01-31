using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class NoteRenderer : MonoBehaviour
{
    private InGame game;
    private NoteSystem system;
    
    public SpriteRenderer head,    body;
    private Transform     headTf,  bodyTf;

    public double Time { get; private set; }
    public double SliderTime { get; private set; }
    public double CalcTime { get; private set; }
    public double CalcSliderTime { get; private set; }
    public bool IsSlider { get; private set; }
    public bool IsPressed { get; set; }

    private float column;
    private double newTime;

    private static readonly Color MiddleColor   = new Color( 0.2078432f, 0.7843138f, 1f, 1f );
    private static readonly Color BodyColor     = new Color( .4f, .4f, .4f, 1f );
    private static readonly Color BodyFailColor = new Color( .15f, .15f, .15f, 1f );

    private double weight;

    private void Awake()
    {
        game = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        game.OnScrollChanged += ScrollUpdate;

        headTf = head.transform;
        headTf.localScale = new Vector2( GameSetting.NoteWidth, GameSetting.NoteHeight );

        bodyTf = body.transform;
    }

    private void OnDestroy()
    {
        game.OnScrollChanged -= ScrollUpdate;
    }


    public void SetInfo( int _lane, NoteSystem _system, in Note _data )
    {
        system    = _system;
        newTime   = _data.calcTime;
        IsPressed = false;

        Time           = _data.time;
        CalcTime       = _data.calcTime;
        SliderTime     = _data.sliderTime;
        CalcSliderTime = _data.calcSliderTime;
        IsSlider       = _data.isSlider;

        column = GameSetting.NoteStartPos + ( _lane * GameSetting.NoteWidth ) + ( ( _lane + 1 ) * GameSetting.NoteBlank );

        body.enabled = IsSlider ? true : false;
        ScrollUpdate();

        head.color = _lane == 1 || _lane == 4 ? MiddleColor : Color.white;
        body.color = BodyColor;
    }

    public void SetBodyFail() => body.color = BodyFailColor;

    private void ScrollUpdate()
    {
        weight = GameSetting.Weight;
        if ( !IsSlider ) return;

        double sliderLengthAbs = Globals.Abs( ( CalcSliderTime - CalcTime ) * GameSetting.Weight );
        bodyTf.localScale = new Vector2( GameSetting.NoteWidth * .8f, ( float )sliderLengthAbs );
    }

    public void Despawn()
    {
        //gameObject.SetActive( false );
        system.Despawn( this );
    }

    private void LateUpdate()
    {
        //double weight = GameSetting.Weight;
        Vector2 headPos;
        if ( IsPressed )
        {
            if ( transform.position.y <= GameSetting.JudgePos )
                 newTime = NowPlaying.PlaybackChanged;

            headPos         = new Vector2( column, GameSetting.JudgePos + ( float )( ( newTime        - NowPlaying.PlaybackChanged ) * weight ) );
            Vector2 tailPos = new Vector2( column, GameSetting.JudgePos + ( float )( ( CalcSliderTime - NowPlaying.PlaybackChanged ) * weight ) );

            double bodyDiff   = tailPos.y - headPos.y;
            bodyTf.localScale = new Vector2( GameSetting.NoteWidth * .8f, bodyDiff <= 0d ? 0f : ( float )bodyDiff );
        }
        else
        {
            headPos = new Vector2( column, GameSetting.JudgePos + ( float )( ( ( newTime - NowPlaying.PlaybackChanged ) * weight ) ) );
        }

        transform.position = headPos;
    }
}
