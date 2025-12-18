public class SoundOffsetOption : OptionSlider
{
    protected override void Awake()
    {
        base.Awake();

        curValue = GameSetting.OriginSoundOffset;
        UpdateValue( curValue );
    }

    public void InputProcess( float _value )
    {
        GameSetting.OriginSoundOffset = ( int )_value;
        UpdateText( _value );
    }

    public override void Process()
    {
        GameSetting.OriginSoundOffset = curValue;
    }
}