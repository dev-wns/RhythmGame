using UnityEngine;

public class ShowMeasureOption : OptionBoolButton
{
    private void OnEnable()
    {
        CurrentBool = GameSetting.CurrentVisualFlag.HasFlag( VisualFlag.ShowMeasure );
        title.color = CurrentBool ? Color.white : DisableColor;
    }

    public override void Process()
    {
        base.Process();

        if ( CurrentBool ) GameSetting.CurrentVisualFlag |=  VisualFlag.ShowMeasure; 
        else               GameSetting.CurrentVisualFlag &= ~VisualFlag.ShowMeasure;

        title.color = CurrentBool ? Color.white : DisableColor;
    }
}