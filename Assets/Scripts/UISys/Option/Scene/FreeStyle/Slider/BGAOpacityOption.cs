using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGAOpacityOption : OptionSlider
{
    protected override void Awake()
    {
        base.Awake();

        curValue = GameSetting.BGAOpacity * 100f;
        UpdateValue( curValue );
    }

    public override void Process()
    {
        GameSetting.BGAOpacity = curValue * .01f;
    }
}
