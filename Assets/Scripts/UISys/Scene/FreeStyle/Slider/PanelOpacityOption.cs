using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelOpacityOption : OptionSlider
{
    protected override void Awake()
    {
        base.Awake();

        curValue = GameSetting.PanelOpacity * 100f;
        UpdateValue( curValue );
    }

    public override void Process()
    {
        GameSetting.PanelOpacity = curValue * .01f;
    }
}
