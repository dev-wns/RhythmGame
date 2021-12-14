using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteRenderer : MonoBehaviour
{
    private static readonly Vector3 InitScale = new Vector3( GlobalSetting.NoteWidth, GlobalSetting.NoteHeight, 1f );
    private NoteSystem NSystem;
    public float Time { get; private set; }
    public float CalcTime { get; private set; }
    public float SliderTime { get; private set; }
    public float CalcSliderTime { get; private set; }
    public bool IsSlider { get; private set; }

    public bool isHolding;
    public float column;
    private SpriteRenderer rdr;

    public void Initialized( Note _data )
    {
        Time           = _data.time;
        CalcTime       = _data.calcTime;
        SliderTime     = _data.sliderTime;
        CalcSliderTime = _data.calcSliderTime;
        IsSlider       = _data.isSlider;

        column = GlobalSetting.NoteStartPos + ( _data.line * GlobalSetting.NoteWidth ) + ( ( _data.line + 1 ) * GlobalSetting.NoteBlank );

        if ( _data.isSlider )
        {
            transform.localScale = new Vector3( GlobalSetting.NoteWidth, Mathf.Abs( ( CalcSliderTime * GlobalSetting.ScrollSpeed ) - ( GlobalSetting.ScrollSpeed ) ), 1f );
            IsSlider = true;
        }
        else
        {
            transform.localScale = InitScale;
            IsSlider = false;
        }

        if ( _data.line == 1 || _data.line == 4 ) rdr.color = Color.blue;
        else                                      rdr.color = Color.white;
    }
    private void OnScrollSpeedChange()
    {
        if ( IsSlider ) transform.localScale = new Vector3( GlobalSetting.NoteWidth, Mathf.Abs( ( CalcSliderTime - CalcTime ) * GlobalSetting.ScrollSpeed ), 1f );
    }

    public void Destroy() 
    {
        gameObject.SetActive( false );
        NSystem.nPool.Despawn( this ); 
    }

    private void Awake()
    {
        NSystem = GameObject.FindGameObjectWithTag( "Systems" ).GetComponent<NoteSystem>();
        rdr = GetComponent<SpriteRenderer>();
        transform.localScale = InitScale;
        GlobalSetting.OnScrollChanged += OnScrollSpeedChange;
    }

    private void LateUpdate()
    {
        if ( isHolding )
        {
            if ( transform.localScale.y > 0 )
            {
                transform.localScale = new Vector3( GlobalSetting.NoteWidth, ( CalcSliderTime - InGame.PlaybackChanged ) * GlobalSetting.ScrollSpeed , 1f );
            }

            transform.position = new Vector3( column, GlobalSetting.JudgeLine, 2f );
        }
        else
        {
            transform.position = new Vector3( column, GlobalSetting.JudgeLine + ( ( CalcTime - InGame.PlaybackChanged ) * GlobalSetting.ScrollSpeed ), 2f );
        }
    }
}
