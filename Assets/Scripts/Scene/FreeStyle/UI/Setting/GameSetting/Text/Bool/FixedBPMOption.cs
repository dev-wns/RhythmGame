using System;
using UnityEngine;

public class FixedBPMOption : OptionBoolButton
{
    public Action OnChangeOption;

    private void OnEnable()
    {
        CurrentBool = GameSetting.CurrentGameMode.HasFlag( GameMode.FixedBPM );
        title.color = CurrentBool ? Color.white : DisableColor;
    }

    public override void Process()
    {
        base.Process();

        if ( CurrentBool ) GameSetting.CurrentGameMode |=  GameMode.FixedBPM; 
        else               GameSetting.CurrentGameMode &= ~GameMode.FixedBPM;

        title.color = CurrentBool ? Color.white : DisableColor;
        OnChangeOption?.Invoke();

        Debug.Log( $"FixedBPM  {CurrentBool}" );
    }
}
