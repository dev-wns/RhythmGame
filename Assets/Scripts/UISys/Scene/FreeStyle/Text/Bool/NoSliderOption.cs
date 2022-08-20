using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoSliderOption : OptionText
{
    public TMPro.TextMeshProUGUI settingText;

    private void OnEnable()
    {
        curIndex = GameSetting.CurrentGameMode.HasFlag( GameMode.NoSlider ) ? 1 : 0;
        ChangeText( texts[curIndex] );
    }

    protected override void CreateObject()
    {
        for ( int i = 0; i < ( int )BooleanOption.Count; i++ )
        {
            texts.Add( ( ( BooleanOption )i ).ToString() );
        }
    }

    public override void Process()
    {
        if ( curIndex == 0 ) GameSetting.CurrentGameMode &= ~GameMode.NoSlider;
        else                 GameSetting.CurrentGameMode |=  GameMode.NoSlider;

        string temp = ( GameSetting.CurrentGameMode & GameMode.NoSlider ) != 0 ? "On" : "Off";
        settingText.text = $"{temp}";
    }
}
