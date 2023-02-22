using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearAlignmentOption : OptionSlider
{
    protected override void Awake()
    {
        base.Awake();

        curValue = Mathf.RoundToInt( GameSetting.GearOffsetX );
        UpdateValue( curValue );
    }

    public override void Process()
    {
        GameSetting.GearOffsetX = curValue;
    }
}
