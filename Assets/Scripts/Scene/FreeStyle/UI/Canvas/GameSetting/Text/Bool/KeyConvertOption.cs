using System;
using UnityEngine;

public class KeyConvertOption : OptionBoolButton
{
    public Action OnChangedOption;

    private void OnEnable()
    {
        CurrentBool = GameSetting.HasFlag( GameMode.ConvertKey );
        title.color = CurrentBool ? Color.white : DisableColor;
    }

    public override void Process()
    {
        base.Process();

        if ( CurrentBool ) GameSetting.CurrentGameMode |=  GameMode.ConvertKey; 
        else               GameSetting.CurrentGameMode &= ~GameMode.ConvertKey;

        title.color = CurrentBool ? Color.white : DisableColor;
        OnChangedOption?.Invoke();
    }
}
