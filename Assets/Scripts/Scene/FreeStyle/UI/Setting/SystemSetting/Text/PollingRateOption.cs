using UnityEngine;

public class PollingRateOption : OptionText
{
    private void OnEnable()
    {
        CurrentIndex = ( int ) SystemSetting.CurrentPollingRate;
        ChangeText( texts[CurrentIndex] );
    }

    protected override void CreateObject()
    {
        for ( int i = 0; i < ( int ) PollingRate.Count; i++ )
        {
            switch ( ( PollingRate ) i )
            {
                case PollingRate._125:  texts.Add( $"125 Hz"            ); break;
                case PollingRate._500:  texts.Add( $"500 Hz"            ); break;
                case PollingRate._1000: texts.Add( $"1000 Hz( ±âº»°ª )" ); break;
                case PollingRate._3000: texts.Add( $"3000 Hz"          ); break;
                case PollingRate._8000: texts.Add( $"8000 Hz"          ); break;
            }
        }
    }

    public override void Process()
    {
        SystemSetting.CurrentPollingRate = ( PollingRate ) CurrentIndex;
    }
}
