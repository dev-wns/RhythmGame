using System;
using UnityEngine;

public class NoSliderOption : OptionBoolButton
{
    public Action OnChangedOption;

    private void OnEnable()
    {
        CurrentBool = GameSetting.CurrentGameMode.HasFlag( GameMode.NoSlider );
        title.color = CurrentBool ? Color.white : DisableColor;
    }

    public override void Process()
    {
        base.Process();

        if ( CurrentBool ) GameSetting.CurrentGameMode |=  GameMode.NoSlider; 
        else               GameSetting.CurrentGameMode &= ~GameMode.NoSlider;

        title.color = CurrentBool ? Color.white : DisableColor;
        OnChangedOption?.Invoke();
    }
}
