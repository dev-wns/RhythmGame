using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgementOption : OptionSlider
{
    protected override void Awake()
    {
        base.Awake();
        
        curValue = GameSetting.JudgePos + ( Screen.height * .5f );
        UpdateValue( curValue );
    }

    public override void Process()
    {
        GameSetting.JudgePos = curValue - ( Screen.height * .5f );
    }
}
