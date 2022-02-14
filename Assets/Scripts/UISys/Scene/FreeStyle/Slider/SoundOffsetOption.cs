using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundOffsetOption : OptionSlider
{
    public TMPro.TextMeshProUGUI settingText;

    protected override void Awake()
    {
        base.Awake();

        curValue = GameSetting.SoundOffset;
        UpdateValue( curValue );
    }

    public override void Process()
    {
        GameSetting.SoundOffset = ( int )curValue;
        settingText.text = $"{Globals.Round( GameSetting.SoundOffset )}";
    }
}