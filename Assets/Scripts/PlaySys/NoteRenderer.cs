using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteRenderer : MonoBehaviour
{
    private NoteSystem system;
    public SpriteRenderer head, body, tail;

    public float Time { get; private set; }
    public float CalcTime { get; private set; }
    public float SliderTime { get; private set; }
    public float CalcSliderTime { get; private set; }
    public bool IsSlider { get; private set; }
    public bool isHolding { get; set; }

    private float column { get; set; }
    private float newTime;

    private bool isActive = false;

    public void SetInfo( int _lane, NoteSystem _system, Note _data )
    {
        system    = _system;
        newTime   = _data.calcTime;
        isHolding = false;

        Time           = _data.time;
        CalcTime       = _data.calcTime;
        SliderTime     = _data.sliderTime;
        CalcSliderTime = _data.calcSliderTime;
        IsSlider       = _data.isSlider;

        column = GlobalSetting.NoteStartPos + ( _lane * GlobalSetting.NoteWidth ) + ( ( _lane + 1 ) * GlobalSetting.NoteBlank );

        tail.enabled = false;
        if ( IsSlider )
        {
            system.CurrentScene.OnScrollChanged += ScaleUpdate;
            head.enabled = true;
            body.enabled = true;
            //body.SetActive( true );
            //tail.SetActive( true );
        }
        else
        {
            head.enabled = true;
            body.enabled = false;
            //body.SetActive( false );
            //tail.SetActive( false );
        }

        ScaleUpdate();
        if ( _lane == 1 || _lane == 4 ) head.color = new Color( 0.2078432f, 0.7843138f, 1f, 1f );
        else                            head.color = Color.white;

        isActive = true;
    }

    //public void SetColor( Color _color ) => rdr.color = _color;

    private void ScaleUpdate()
    {
        head.transform.localScale = new Vector3( GlobalSetting.NoteWidth, GlobalSetting.NoteHeight, 1f );
        if ( IsSlider )
        {
            body.transform.localScale = new Vector3( GlobalSetting.NoteWidth * .8f, Mathf.Abs( ( CalcSliderTime - CalcTime ) * GameSetting.Weight ), 1f );
            tail.transform.localScale = new Vector3( GlobalSetting.NoteWidth, GlobalSetting.NoteHeight, 1f );
        }

        // if ( IsSlider ) transform.localScale = new Vector3( GlobalSetting.NoteWidth, Mathf.Abs( ( CalcSliderTime - CalcTime ) * GameSetting.Weight ), 1f );
        // else            transform.localScale = new Vector3( GlobalSetting.NoteWidth, GlobalSetting.NoteHeight, 1f );
    }

    public void Despawn()
    {
        if ( IsSlider )
            system.CurrentScene.OnScrollChanged -= ScaleUpdate;

        head.enabled = false;
        body.enabled = false;
        tail.enabled = false;
        isActive = false;
        system?.Despawn( this );
    }

    private void LateUpdate()
    {
        if ( !isActive ) return;
        
        if ( isHolding )
        {
            if ( head.transform.position.y <= GlobalSetting.JudgeLine )
                 newTime = NowPlaying.PlaybackChanged;

            float startPos = GlobalSetting.JudgeLine + ( ( newTime  - NowPlaying.PlaybackChanged ) * GameSetting.Weight );
            float endPos   = GlobalSetting.JudgeLine + ( ( CalcTime - NowPlaying.PlaybackChanged ) * GameSetting.Weight ) +
                                                          Mathf.Abs( ( CalcSliderTime - CalcTime ) * GameSetting.Weight );

            head.transform.position = new Vector3( column, startPos, 2f );
            body.transform.position = new Vector3( column, startPos, 2f );
            tail.transform.position = new Vector3( column, endPos, 2f );

            float bodyScale = tail.transform.position.y - head.transform.position.y;
            if ( bodyScale <= 0f ) bodyScale = 0f;

            body.transform.localScale = new Vector3( GlobalSetting.NoteWidth * .8f, bodyScale, 1f );
        }
        else
        {
            float startPos = GlobalSetting.JudgeLine + ( ( newTime  - NowPlaying.PlaybackChanged ) * GameSetting.Weight );
            float endPos   = GlobalSetting.JudgeLine + ( ( CalcTime - NowPlaying.PlaybackChanged ) * GameSetting.Weight ) +
                                                          Mathf.Abs( ( CalcSliderTime - CalcTime ) * GameSetting.Weight );
            head.transform.position = new Vector3( column, startPos, 2f );

            if ( IsSlider )
            {
                body.transform.position = new Vector3( column, startPos, 2f );
                tail.transform.position = new Vector3( column, endPos, 2f );
            }
        }
    }
}
