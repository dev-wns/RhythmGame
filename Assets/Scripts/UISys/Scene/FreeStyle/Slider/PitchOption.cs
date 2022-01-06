using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PitchOption : OptionSlider
{
    protected override void Awake()
    {
        base.Awake();

        curValue = GameSetting.SoundPitch;
        UpdateValue( curValue );
    }

    public override void Process()
    {
        GameSetting.SoundPitch = curValue;
    }
}
