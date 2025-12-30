using UnityEngine;

public class NoFailOption : OptionBoolButton
{
    private void OnEnable()
    {
        CurrentBool = GameSetting.CurrentGameMode.HasFlag( GameMode.NoFail );
        title.color = CurrentBool ? Color.white : DisableColor;
    }

    public override void Process()
    {
        base.Process();

        if ( CurrentBool ) GameSetting.CurrentGameMode |=  GameMode.NoFail; 
        else               GameSetting.CurrentGameMode &= ~GameMode.NoFail;

        title.color = CurrentBool ? Color.white : DisableColor;
    }
}
