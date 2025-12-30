public class JudgementOption : OptionSlider
{
    protected override void Awake()
    {
        base.Awake();

        //curValue = GameSetting.JudgePos + ( Screen.height * .5f );
        curValue = GameSetting.JudgeOffset;

        UpdateValue( curValue );
    }

    public override void ChangedValue( float _value )
    {
        GameSetting.JudgeOffset = ( int )_value;
        UpdateText( _value );
    }

    public override void Process()
    {
        // GameSetting.JudgePos = curValue - ( Screen.height * .5f );
        GameSetting.JudgeOffset = curValue;
    }
}
