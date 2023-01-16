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

    public override void Process()
    {
        GameSetting.BGAOpacity = curValue;
    }
}
