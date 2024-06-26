using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelOpacityOption : OptionSlider
{
    protected override void Awake()
    {
        base.Awake();

        curValue = GameSetting.PanelOpacity;
        UpdateValue( curValue );
    }

    public void InputProcess( float _value )
    {
        GameSetting.PanelOpacity = ( int )_value;
        UpdateText( _value );
    }

    public override void Process()
    {
        GameSetting.PanelOpacity = curValue;
    }
}
