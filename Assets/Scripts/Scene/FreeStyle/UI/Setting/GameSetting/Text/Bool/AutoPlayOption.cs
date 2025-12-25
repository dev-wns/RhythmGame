using UnityEngine;

public class AutoPlayOption : OptionBoolButton
{
    private void OnEnable()
    {
        CurrentBool = GameSetting.CurrentGameMode.HasFlag( GameMode.AutoPlay );
        title.color = CurrentBool ? Color.white : DisableColor;
    }

    public override void Process()
    {
        base.Process();

        if ( CurrentBool ) GameSetting.CurrentGameMode |=  GameMode.AutoPlay;
        else               GameSetting.CurrentGameMode &= ~GameMode.AutoPlay; 

        title.color = CurrentBool ? Color.white : DisableColor;

        Debug.Log( $"AutoPlay  {CurrentBool}" );
    }
}
