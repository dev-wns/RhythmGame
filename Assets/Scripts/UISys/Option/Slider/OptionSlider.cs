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
    }

    public override void RightArrow()
    {
        curValue += increaseValue;
        if ( curValue > maxValue )
             curValue = maxValue;

        Process();
        UpdateValue( curValue );
    }

    protected void UpdateValue( float _value )
    {
        if ( valueText == null ) return;

        if ( slider.wholeNumbers )
            valueText.text = Mathf.RoundToInt( _value ).ToString();
        else
            valueText.text = string.Format( "{0:0.#}", curValue );
        slider.value = curValue;
    }
}
