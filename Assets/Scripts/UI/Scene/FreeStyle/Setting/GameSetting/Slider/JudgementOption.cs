public class JudgementOption : OptionSlider
{
    protected override void Awake()
    {
        base.Awake();

        //curValue = GameSetting.JudgePos + ( Screen.height * .5f );
        curValue = GameSetting.JudgeOffset;

        UpdateValue( curValue );
    }

    public override void Process()
    {
        // GameSetting.JudgePos = curValue - ( Screen.height * .5f );
        GameSetting.JudgeOffset = curValue;
    }
}
