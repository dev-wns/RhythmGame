using UnityEngine;

public class GearAlignmentOption : OptionSlider
{
    protected override void Awake()
    {
        base.Awake();

        curValue = Mathf.RoundToInt( GameSetting.GearOffsetX );
        UpdateValue( curValue );
    }

    public void InputProcess( float _value )
    {
        GameSetting.GearOffsetX = ( int )_value;
        UpdateText( _value );
    }

    public override void Process()
    {
        GameSetting.GearOffsetX = curValue;
    }
}
