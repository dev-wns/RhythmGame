using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public abstract class OptionSlider : OptionBase
{
    [Header( "Slider" )]
    public Slider slider;
    public TextMeshProUGUI valueText;

    public int increaseValue = 10;
    public int minValue, maxValue;
    public int curValue;

    public string prePos, postPos;

    [Header( "Fast Scroll Time" )]
    private static float PressUpdateTime = .05f;
    private static float PressWaitTime   = .5f;
    private float time;
    private bool  isPress = false;

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

    public override void InputProcess()
    {
        InputAction( KeyCode.LeftArrow,  LeftArrow );
        InputAction( KeyCode.RightArrow, RightArrow );
    }

    private void InputAction( KeyCode _keyCode, Action _action )
    {
        if ( Input.GetKeyDown( _keyCode ) )
        {
            _action?.Invoke();
        }
        else if ( Input.GetKey( _keyCode ) )
        {
            time += Time.deltaTime;
            if ( time >= PressWaitTime )
                isPress = true;

            if ( isPress && time >= PressUpdateTime )
            {
                time = 0f;
                _action?.Invoke();
            }
        }
        else if ( Input.GetKeyUp( _keyCode ) )
        {
            time = 0f;
            isPress = false;
        }
    }

    private void LeftArrow()
    {
        curValue -= increaseValue;
        if ( curValue < minValue )
        {
            curValue = minValue;
            return;
        }

        Process();
        UpdateValue( curValue );
        SoundManager.Inst.Play( SoundSfxType.Slider );
    }

    private void RightArrow()
    {
        curValue += increaseValue;
        if ( curValue > maxValue )
        {
            curValue = maxValue;
            return;
        }

        Process();
        UpdateValue( curValue );
        SoundManager.Inst.Play( SoundSfxType.Slider );
    }

    protected void UpdateValue( float _value )
    {
        if ( valueText == null ) return;

        if ( slider.wholeNumbers ) valueText.text = string.Format( "{0}{1}{2}",     prePos, Mathf.RoundToInt( _value ), postPos );
        else                       valueText.text = string.Format( "{0}{1:0.#}{2}", prePos, curValue,                   postPos );

        slider.value = curValue;
    }

    protected void UpdateText( float _value )
    {
        if ( valueText == null ) return;

        if ( slider.wholeNumbers ) valueText.text = string.Format( "{0}{1}{2}", prePos, Mathf.RoundToInt( _value ), postPos );
        else valueText.text = string.Format( "{0}{1:0.#}{2}", prePos, curValue, postPos );
    }
}
