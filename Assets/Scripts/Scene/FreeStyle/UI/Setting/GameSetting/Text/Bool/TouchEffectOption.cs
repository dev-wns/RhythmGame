using UnityEngine;

public class TouchEffectOption : OptionBoolButton
{
    private void OnEnable()
    {
        CurrentBool = GameSetting.CurrentVisualFlag.HasFlag( VisualFlag.HitEffect );
        title.color = CurrentBool ? Color.white : DisableColor;
    }

    public override void Process()
    {
        base.Process();

        if ( CurrentBool ) GameSetting.CurrentVisualFlag |=  VisualFlag.HitEffect;
        else               GameSetting.CurrentVisualFlag &= ~VisualFlag.HitEffect; 

        title.color = CurrentBool ? Color.white : DisableColor;

        Debug.Log( $"HitEffect  {CurrentBool}" );
    }
}
