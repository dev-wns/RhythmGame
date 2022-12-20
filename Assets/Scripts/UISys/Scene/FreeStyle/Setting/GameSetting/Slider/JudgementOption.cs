using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgementOption : OptionSlider
{
    protected override void Awake()
    {
        base.Awake();
        
        //curValue = GameSetting.JudgePos + ( Screen.height * .5f );
        curValue = Global.Math.Round( GameSetting.JudgeOffset );
        
        UpdateValue( curValue );
    }

    public override void Process()
    {
        // GameSetting.JudgePos = curValue - ( Screen.height * .5f );
        GameSetting.JudgeOffset = curValue;
    }
}
