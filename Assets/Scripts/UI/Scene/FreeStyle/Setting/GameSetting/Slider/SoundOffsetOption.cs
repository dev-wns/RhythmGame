using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundOffsetOption : OptionSlider
{
    protected override void Awake()
    {
        base.Awake();

        curValue = GameSetting.SoundOffset;
        UpdateValue( curValue );
    }

    public void InputProcess( float _value )
    {
        GameSetting.SoundOffset = ( int )_value;
        UpdateText( _value );
    }

    public override void Process()
    {
        GameSetting.SoundOffset = curValue;
    }
}