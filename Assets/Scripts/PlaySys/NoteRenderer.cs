using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteRenderer : MonoBehaviour
{
    private InGame scene;
    public NoteSystem system;

    public float Time { get; private set; }
    public float CalcTime { get; private set; }
    public float SliderTime { get; private set; }
    public float CalcSliderTime { get; private set; }
    public bool IsSlider { get; private set; }

    public bool isHolding;
    public float column;
    private SpriteRenderer rdr;
    private float newTime;

    public void SetInfo( Note _data )
    {
        newTime   = _data.calcTime;
        isHolding = false;

        Time           = _data.time;
        CalcTime       = _data.calcTime;
        SliderTime     = _data.sliderTime;
        CalcSliderTime = _data.calcSliderTime;
        IsSlider       = _data.isSlider;

        column = GlobalSetting.NoteStartPos + ( _data.line * GlobalSetting.NoteWidth ) + ( ( _data.line + 1 ) * GlobalSetting.NoteBlank );

        if ( IsSlider ) 
             scene.OnScrollChanged += ScaleUpdate;

        ScaleUpdate();
        if ( _data.line == 1 || _data.line == 4 ) SetColor( new Color( 0.2078432f, 0.7843138f, 1f, 1f ) );
        else                                      SetColor( Color.white );
    }

    public void SetColor( Color _color ) => rdr.color = _color;

    private void ScaleUpdate()
    {
        if ( IsSlider ) transform.localScale = new Vector3( GlobalSetting.NoteWidth, Mathf.Abs( ( CalcSliderTime - CalcTime ) * GameSetting.Weight ), 1f );
        else            transform.localScale = new Vector3( GlobalSetting.NoteWidth, GlobalSetting.NoteHeight, 1f );
    }

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        rdr   = GetComponent<SpriteRenderer>();
    }


    public void Despawn()
    {
        if ( IsSlider ) 
             scene.OnScrollChanged -= ScaleUpdate;

        system?.Despawn( this );
    }

    private void LateUpdate()
    {

        if ( isHolding )
        {
            float startDiff = ( CalcTime       - NowPlaying.PlaybackChanged ) * GameSetting.Weight;
            float endDiff   = ( CalcSliderTime - NowPlaying.PlaybackChanged ) * GameSetting.Weight;
            float currentScale = Mathf.Abs( endDiff - startDiff ) - Mathf.Abs( startDiff );
            if ( endDiff > 0 && transform.localScale.y > 0 )
                 transform.localScale = new Vector3( GlobalSetting.NoteWidth, currentScale, 1f );
            else
                 transform.localScale = new Vector3( GlobalSetting.NoteWidth, 0f, 1f );

            transform.position = new Vector3( column, GlobalSetting.JudgeLine, 2f );
            newTime = NowPlaying.PlaybackChanged;
        }
        else
        {
            transform.position = new Vector3( column, GlobalSetting.JudgeLine + ( ( newTime - NowPlaying.PlaybackChanged ) * GameSetting.Weight ), 2f );
        }
    }
}
