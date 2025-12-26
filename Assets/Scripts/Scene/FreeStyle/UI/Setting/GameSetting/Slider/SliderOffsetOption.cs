public class SliderOffsetOption : OptionSlider
{
    protected override void Awake()
    {
        base.Awake();

        curValue = GameSetting.SliderOffset;
        UpdateValue( curValue );
    }

    public override void ChangedValue( float _value )
    {
        GameSetting.SliderOffset = ( int )_value;
        UpdateText( _value );
    }

    public override void Process()
    {
        GameSetting.SliderOffset = curValue;
    }
}