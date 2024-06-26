using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGAOpacityOption : OptionSlider
{
    protected override void Awake()
    {
        base.Awake();

        curValue = GameSetting.BGAOpacity;
        UpdateValue( curValue );
    }
    
    public void InputProcess( float _value )
    {
        GameSetting.BGAOpacity = ( int )_value;
        UpdateText( _value );
    }

    public override void Process()
    {
        GameSetting.BGAOpacity = curValue;
    }
}
