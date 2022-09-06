using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPitchOption : OptionSlider
{
    public event Action<float/* Pitch */> OnPitchUpdate;

    protected override void Awake()
    {
        base.Awake();

        curValue = GameSetting.CurrentPitch * 100f;
        UpdateValue( curValue );
    }

    public override void Process()
    {
        GameSetting.CurrentPitch = ( int )curValue;
        SoundManager.Inst.Pitch = GameSetting.CurrentPitch;
        // curValue * .01f;
        OnPitchUpdate?.Invoke( GameSetting.CurrentPitch );
    }
}
