using System;

public class ScrollSpeedOption : OptionSlider
{
    public event Action OnChangedOption;

    protected override void Awake()
    {
        base.Awake();

        curValue = GameSetting.ScrollSpeed;
        UpdateValue( curValue );
    }

    public override void ChangedValue( float _value )
    {
        GameSetting.ScrollSpeed = ( int )_value;
        UpdateText( _value );
        OnChangedOption?.Invoke();
    }

    public override void Process()
    {
        GameSetting.ScrollSpeed = curValue;
    }
}