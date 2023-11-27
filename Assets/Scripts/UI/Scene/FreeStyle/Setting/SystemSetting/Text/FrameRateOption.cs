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

        QualitySettings.vSyncCount = SystemSetting.CurrentFrameRate == FrameRate.vSync ? 1 : 0;
        switch ( type )
        {
            case FrameRate.vSync:
            case FrameRate.No_Limit: Application.targetFrameRate = 0; break;

            case FrameRate._60:
            case FrameRate._144:
            case FrameRate._240:
            case FrameRate._960:
            {
                QualitySettings.vSyncCount = 0;
                var frame = ( type ).ToString().Replace( "_", " " );
                Application.targetFrameRate = int.Parse( frame );
            } break;
        }

        SystemSetting.CurrentFrameRate = type;
    }
}
