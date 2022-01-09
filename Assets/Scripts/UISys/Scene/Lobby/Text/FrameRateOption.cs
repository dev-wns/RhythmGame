using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class FrameRateOption : OptionText
{
    protected override void Awake()
    {
        base.Awake();

        curIndex = ( int )SystemSetting.FrameRate;
        ChangeText( texts[curIndex] );
    }

    protected override void CreateObject()
    {
        StringBuilder builder = new StringBuilder();
        for ( int i = 0; i < ( int )FRAME_RATE.Count; i++ )
        {
            var text = ( ( FRAME_RATE )i ).ToString();

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
        var type = ( FRAME_RATE )curIndex;
        switch ( type )
        {
            case FRAME_RATE.vSync:
                 QualitySettings.vSyncCount = 1;
                 break;

            case FRAME_RATE.No_Limit:
                 QualitySettings.vSyncCount  = 0;
                 Application.targetFrameRate = 10000;
                 break;

            case FRAME_RATE._60:
            case FRAME_RATE._144:
            case FRAME_RATE._300:
            case FRAME_RATE._960:
            {
                QualitySettings.vSyncCount = 0;
                var frame = ( type ).ToString().Replace( "_", " " );
                Application.targetFrameRate = int.Parse( frame );
            } break;

            default: break;
        }

        SystemSetting.FrameRate = type;
    }
}
