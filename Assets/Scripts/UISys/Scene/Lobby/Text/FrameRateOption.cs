using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class FrameRateOption : OptionText
{
    protected override void Awake()
    {
        base.Awake();

        curIndex = ( int )SystemSetting.CurrentFrameRate;
        ChangeText( texts[curIndex] );
    }

    protected override void CreateObject()
    {
        StringBuilder builder = new StringBuilder();
        for ( int i = 0; i < ( int )FrameRate.Count; i++ )
        {
            var text = ( ( FrameRate )i ).ToString();

            builder.Clear();
            builder.Append( text.Replace( "_", " " ).Trim() );
            if ( i >= 2 )
            {
                builder.Append( " FPS" );
            }

            texts.Add( builder.ToString() );
        }
    }

    public override void Process()
    {
        var type = ( FrameRate )curIndex;
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
            case FrameRate._300:
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
