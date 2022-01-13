using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteRenderer : MonoBehaviour
{
    private NoteSystem system;
    public SpriteRenderer head, body, tail;
    private Transform headTf, bodyTf, tailTf;

    public float Time { get; private set; }
    public float CalcTime { get; private set; }
    public float SliderTime { get; private set; }
    public float CalcSliderTime { get; private set; }
    public bool IsSlider { get; private set; }
    public bool isHolding { get; set; }

    private float column { get; set; }
    private float newTime;
    private float weight;

    private void Awake()
    {
        weight = GameSetting.Weight;
        headTf = head.transform;
        bodyTf = body.transform;
        tailTf = tail.transform;
        tail.enabled = false;
    }

    public void SetInfo( int _lane, NoteSystem _system, in Note _data )
    {
        system    = _system;
        newTime   = _data.calcTime;
        isHolding = false;

        Time           = _data.time;
        CalcTime       = _data.calcTime;
        SliderTime     = _data.sliderTime;
        CalcSliderTime = _data.calcSliderTime;
        IsSlider       = _data.isSlider;

        column = GameSetting.NoteStartPos + ( _lane * GameSetting.NoteWidth ) + ( ( _lane + 1 ) * GameSetting.NoteBlank );

        if ( IsSlider )
        {
            system.CurrentScene.OnScrollChanged += ScaleUpdate;
            head.enabled = true;
            body.enabled = true;
        }
        else
        {
            head.enabled = true;
            body.enabled = false;
        }

        ScaleUpdate();
        if ( _lane == 1 || _lane == 4 ) head.color = new Color( 0.2078432f, 0.7843138f, 1f, 1f );
        else                            head.color = Color.white;
    }

    private void ScaleUpdate()
    {
        weight = GameSetting.Weight;

        head.transform.localScale = new Vector3( GameSetting.NoteWidth, GameSetting.NoteHeight, 1f );
        if ( IsSlider )
        {
            body.transform.localScale = new Vector3( GameSetting.NoteWidth * .8f, Mathf.Abs( ( CalcSliderTime - CalcTime ) * weight ), 1f );
            tail.transform.localScale = new Vector3( GameSetting.NoteWidth, GameSetting.NoteHeight, 1f );
        }
    }

    public void Despawn()
    {
        if ( IsSlider )
            system.CurrentScene.OnScrollChanged -= ScaleUpdate;

        head.enabled = body.enabled = false;
        system.Despawn( this );
    }

    private void LateUpdate()
    {

        if ( isHolding )
        {
            if ( head.transform.position.y <= GameSetting.JudgePos )
                 newTime = NowPlaying.PlaybackChanged;

            Vector3 startPos = new Vector3( column, GameSetting.JudgePos + ( ( newTime  - NowPlaying.PlaybackChanged ) * weight ), 2f );
            headTf.position = bodyTf.position = startPos;

            float sliderLength    = ( CalcSliderTime - CalcTime ) * weight;
            float sliderLengthAbs = sliderLength >= 0 ? sliderLength : -sliderLength;
            tailTf.position       = new Vector3( column, GameSetting.JudgePos + ( ( CalcTime - NowPlaying.PlaybackChanged ) * weight ) + sliderLengthAbs, 2f ); ;

            float bodyScale = tailTf.position.y - headTf.position.y;
            if ( bodyScale <= 0f ) bodyScale = 0f;
            bodyTf.localScale = new Vector3( GameSetting.NoteWidth * .8f, bodyScale, 1f );
        }
        else
        {
            Vector3 startPos = new Vector3( column, GameSetting.JudgePos + ( ( newTime  - NowPlaying.PlaybackChanged ) * weight ), 2f );
            headTf.position  = startPos;

            if ( IsSlider )
            {
                bodyTf.position = startPos;

                float sliderLength = ( CalcSliderTime - CalcTime ) * weight;
                float sliderLengthAbs = sliderLength >= 0 ? sliderLength : -sliderLength;
                tailTf.position = new Vector3( column, GameSetting.JudgePos + ( ( CalcTime - NowPlaying.PlaybackChanged ) * weight ) + sliderLengthAbs, 2f );
            }
        }
    }
}
