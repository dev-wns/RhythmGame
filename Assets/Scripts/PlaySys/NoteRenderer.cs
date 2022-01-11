using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteRenderer : MonoBehaviour
{
    private NoteSystem system;

    public GameObject head, body, tail;

    public float Time { get; private set; }
    public float CalcTime { get; private set; }
    public float SliderTime { get; private set; }
    public float CalcSliderTime { get; private set; }
    public bool IsSlider { get; private set; }

    public bool isHolding;
    public float column;
    private SpriteRenderer rdr;
    private float newTime;

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

        if ( IsSlider )
        {
            system.CurrentScene.OnScrollChanged += ScaleUpdate;
            body.SetActive( true );
            tail.SetActive( true );
        }
        else
        {
            body.SetActive( false );
            tail.SetActive( false );
        }

        ScaleUpdate();
        if ( _lane == 1 || _lane == 4 ) head.GetComponent<SpriteRenderer>().color = new Color( 0.2078432f, 0.7843138f, 1f, 1f );
        else                            head.GetComponent<SpriteRenderer>().color = Color.white;
    }

    public void SetColor( Color _color ) => rdr.color = _color;

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

    private void Awake()
    {
        rdr = GetComponent<SpriteRenderer>();
    }

    public void Despawn()
    {
        if ( IsSlider )
            system.CurrentScene.OnScrollChanged -= ScaleUpdate;

        system?.Despawn( this );
    }

    private void LateUpdate()
    {
        var moveTime = GlobalSetting.JudgeLine + ( ( newTime - NowPlaying.PlaybackChanged ) * GameSetting.Weight );
        head.transform.position = new Vector3( column, moveTime, 2f );
        if ( IsSlider )
        {
            body.transform.position = new Vector3( column, moveTime, 2f );
            tail.transform.position = new Vector3( column, Mathf.Abs( ( CalcSliderTime - CalcTime ) * GameSetting.Weight ) + moveTime, 2f );
        }

        //if ( isHolding )
        //{
        //    float startDiff = ( CalcTime       - NowPlaying.PlaybackChanged ) * GameSetting.Weight;
        //    float endDiff   = ( CalcSliderTime - NowPlaying.PlaybackChanged ) * GameSetting.Weight;
        //    float currentScale = Mathf.Abs( endDiff - startDiff ) - Mathf.Abs( startDiff );
        //    if ( endDiff > 0 && transform.localScale.y > 0 )
        //         transform.localScale = new Vector3( GlobalSetting.NoteWidth, currentScale, 1f );
        //    else
        //         transform.localScale = new Vector3( GlobalSetting.NoteWidth, 0f, 1f );

        //    transform.position = new Vector3( column, GlobalSetting.JudgeLine, 2f );
        //    newTime = NowPlaying.PlaybackChanged;
        //}
        //else
        //{
        //    transform.position = new Vector3( column, GlobalSetting.JudgeLine + ( ( newTime - NowPlaying.PlaybackChanged ) * GameSetting.Weight ), 2f );
        //}
    }
}
