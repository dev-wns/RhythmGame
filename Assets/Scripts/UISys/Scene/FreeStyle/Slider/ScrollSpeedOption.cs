using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollSpeedOption : OptionSlider
{
    protected override void Awake()
    {
        base.Awake();

        curValue = ( float )GameSetting.ScrollSpeed;
        UpdateValue( curValue );
    }

    public override void Process()
    {
        GameSetting.ScrollSpeed = curValue;
    }
}
