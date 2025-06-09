public class AntiAliasingOption : OptionText
{
    private void OnEnable()
    {
        CurrentIndex = ( int )SystemSetting.CurrentAntiAliasing;
        ChangeText( texts[CurrentIndex] );
    }

    protected override void CreateObject()
    {
        for ( int i = 0; i < ( int )AntiAliasing.Count; i++ )
        {
            switch ( ( AntiAliasing )i )
            {
                case AntiAliasing.None: texts.Add( $"Off" ); break;
                case AntiAliasing._2xMultiSampling: texts.Add( $"2x MSAA Multi Sampling" ); break;
                case AntiAliasing._4xMultiSampling: texts.Add( $"4x MSAA Multi Sampling" ); break;
                case AntiAliasing._8xMultiSampling: texts.Add( $"8x MSAA Multi Sampling" ); break;
            }
        }
    }

    public override void Process()
    {
        SystemSetting.CurrentAntiAliasing = ( AntiAliasing )CurrentIndex;
    }
}
