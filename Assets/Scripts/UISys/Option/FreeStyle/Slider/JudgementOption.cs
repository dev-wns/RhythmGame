using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgementOption : OptionSlider
{
    protected override void Awake()
    {
        base.Awake();

        curValue = GameSetting.JudgePos;
        UpdateValue( curValue );
    }

    public override void Process()
    {
        GameSetting.JudgePos = curValue;
    }
}
