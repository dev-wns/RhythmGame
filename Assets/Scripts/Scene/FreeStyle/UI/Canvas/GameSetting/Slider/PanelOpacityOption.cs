public class PanelOpacityOption : OptionSlider
{
    protected override void Awake()
    {
        base.Awake();

        curValue = GameSetting.PanelOpacity;
        UpdateValue( curValue );
    }

    public override void ChangedValue( float _value )
    {
        GameSetting.PanelOpacity = ( int )_value;
        UpdateText( _value );
    }

    public override void Process()
    {
        GameSetting.PanelOpacity = curValue;
    }
}
