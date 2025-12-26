public class BGAOpacityOption : OptionSlider
{
    protected override void Awake()
    {
        base.Awake();

        curValue = GameSetting.BGAOpacity;
        UpdateValue( curValue );
    }

    public override void ChangedValue( float _value )
    {
        GameSetting.BGAOpacity = ( int )_value;
        UpdateText( _value );
    }

    public override void Process()
    {
        GameSetting.BGAOpacity = curValue;
    }
}
