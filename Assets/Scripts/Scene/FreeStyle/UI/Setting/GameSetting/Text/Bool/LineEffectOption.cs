using UnityEngine;

public class LineEffectOption : OptionBoolButton
{
    private void OnEnable()
    {
        CurrentBool = GameSetting.CurrentVisualFlag.HasFlag( VisualFlag.LaneEffect );
        title.color = CurrentBool ? Color.white : DisableColor;
    }

    public override void Process()
    {
        base.Process();

        if ( CurrentBool ) GameSetting.CurrentVisualFlag |=  VisualFlag.LaneEffect; 
        else               GameSetting.CurrentVisualFlag &= ~VisualFlag.LaneEffect;

        title.color = CurrentBool ? Color.white : DisableColor;

        Debug.Log( $"LineEffect  {CurrentBool}" );
    }
}
