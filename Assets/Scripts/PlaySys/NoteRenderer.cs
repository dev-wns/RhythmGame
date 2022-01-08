using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteRenderer : MonoBehaviour
{
    public float Time { get; private set; }
    public float CalcTime { get; private set; }
    public float SliderTime { get; private set; }
    public float CalcSliderTime { get; private set; }
    public bool IsSlider { get; private set; }

    public bool isHolding;
    public float column;
    private SpriteRenderer rdr;

    public void SetInfo( Note _data )
    {
        Time           = _data.time;
        CalcTime       = _data.calcTime;
        SliderTime     = _data.sliderTime;
        CalcSliderTime = _data.calcSliderTime;
        IsSlider       = _data.isSlider;
        isHolding = false;

        column = GlobalSetting.NoteStartPos + ( _data.line * GlobalSetting.NoteWidth ) + ( ( _data.line + 1 ) * GlobalSetting.NoteBlank );

        if ( _data.isSlider ) transform.localScale = new Vector3( GlobalSetting.NoteWidth, Mathf.Abs( ( CalcSliderTime - CalcTime ) * InGame.Weight ), 1f );
        else                  transform.localScale = new Vector3( GlobalSetting.NoteWidth, GlobalSetting.NoteHeight, 1f );

        if ( _data.line == 1 || _data.line == 4 ) SetColor( new Color( 0.2078432f, 0.7843138f, 1f, 1f ) );
        else                                      SetColor( Color.white );
    }

    public void SetColor( Color _color ) => rdr.color = _color;

    private void OnScrollSpeedChange()
    {
        if ( IsSlider ) transform.localScale = new Vector3( GlobalSetting.NoteWidth, Mathf.Abs( ( CalcSliderTime - CalcTime ) * InGame.Weight ), 1f );
    }

    private void Awake()
    {
        rdr = GetComponent<SpriteRenderer>();
        GlobalSetting.OnScrollChanged += OnScrollSpeedChange;
    }

    private void OnDestroy()
    {
        GlobalSetting.OnScrollChanged -= OnScrollSpeedChange;
    }

    private void LateUpdate()
    {
        if ( isHolding )
        {
            if ( transform.localScale.y > 0 ) transform.localScale = new Vector3( GlobalSetting.NoteWidth, ( CalcSliderTime - InGame.PlaybackChanged ) * InGame.Weight, 1f );
            //else                              transform.localScale = new Vector3( GlobalSetting.NoteWidth, 0f, 1f ); ;

            transform.position = new Vector3( column, GlobalSetting.JudgeLine, 2f );
        }
        else
        {
            transform.position = new Vector3( column, GlobalSetting.JudgeLine + ( ( CalcTime - InGame.PlaybackChanged ) * InGame.Weight ), 2f );
        }
    }

}
