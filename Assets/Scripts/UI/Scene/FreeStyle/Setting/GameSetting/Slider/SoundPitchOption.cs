using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SoundPitchOption : OptionSlider
{
    public event Action<float/* Pitch */> OnPitchUpdate;
    public TextMeshProUGUI previewText;

    protected override void Awake()
    {
        base.Awake();

        curValue = Mathf.RoundToInt( GameSetting.CurrentPitch * 100f );
        UpdateValue( curValue );
    }

    public override void Process()
    {
        GameSetting.CurrentPitch = curValue;
        SoundManager.Inst.SetPitch( GameSetting.CurrentPitch, ChannelType.BGM );

        OnPitchUpdate?.Invoke( GameSetting.CurrentPitch );
        previewText.text  = $"x{GameSetting.CurrentPitch:F1}";
        previewText.color = GameSetting.CurrentPitch < 1 ? new Color( .5f, .5f, 1f ) :
                            GameSetting.CurrentPitch > 1 ? new Color( 1f, .5f, .5f ) : Color.white;
    }
}
