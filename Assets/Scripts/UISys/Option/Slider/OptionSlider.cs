using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public abstract class OptionSlider : OptionBindArrowBase
{
    public Slider slider;
    public TextMeshProUGUI valueText;

    public float increaseValue = 10f;
    public float minValue, maxValue;
    public float curValue;

    public string prePos, postPos;

    protected override void Awake()
    {
        base.Awake();

        type = OptionType.Slider;

        if ( slider )
        {
            slider.minValue = minValue;
            slider.maxValue = maxValue;
        }
    }

    public override void LeftArrow()
    {
        curValue -= increaseValue;
        if ( curValue < minValue ) 
             curValue = minValue;

        Process();
        UpdateValue( curValue );
        SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.DECREASE );
    }

    public override void RightArrow()
    {
        curValue += increaseValue;
        if ( curValue > maxValue )
             curValue = maxValue;

        Process();
        UpdateValue( curValue );
        SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.INCREASE );
    }

    protected void UpdateValue( float _value )
    {
        if ( valueText == null ) return;

        if ( slider.wholeNumbers )
            valueText.text = string.Format( "{0}{1}{2}", prePos, Mathf.RoundToInt( _value ), postPos );
        else
            valueText.text = string.Format( "{0}{1:0.#}{2}", prePos, curValue, postPos );
        slider.value = curValue;
    }
}
