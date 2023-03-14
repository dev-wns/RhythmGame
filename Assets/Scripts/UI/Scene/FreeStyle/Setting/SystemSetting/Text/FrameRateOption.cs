using UnityEngine;

public class FrameRateOption : OptionText
{
    private void OnEnable()
    {
        CurrentIndex = ( int )SystemSetting.CurrentFrameRate;
        ChangeText( texts[CurrentIndex] );
    }

    protected override void CreateObject()
    {
        for ( int i = 0; i < ( int )FrameRate.Count; i++ )
        {
            switch ( ( FrameRate )i )
            {
                case FrameRate.No_Limit: texts.Add( $"제한없음" );    break;
                case FrameRate.vSync:    texts.Add( $"수직 동기화" ); break;
                case FrameRate._60:      texts.Add( $"60 FPS" );     break;
                case FrameRate._144:     texts.Add( $"144 FPS" );    break;
                case FrameRate._240:     texts.Add( $"240 FPS" );    break;
                case FrameRate._960:     texts.Add( $"960 FPS" );    break;
            }
        }
    }

    public override void Process()
    {
        var type = ( FrameRate )CurrentIndex;
        switch ( type )
        {
            case FrameRate.vSync:
                 QualitySettings.vSyncCount = 1;
                 break;

            case FrameRate.No_Limit:
                 QualitySettings.vSyncCount  = 0;
                 Application.targetFrameRate = 10000;
                 break;

            case FrameRate._60:
            case FrameRate._144:
            case FrameRate._240:
            case FrameRate._960:
            {
                QualitySettings.vSyncCount = 0;
                var frame = ( type ).ToString().Replace( "_", " " );
                Application.targetFrameRate = int.Parse( frame );
            } break;

            default: break;
        }

        SystemSetting.CurrentFrameRate = type;
    }
}
