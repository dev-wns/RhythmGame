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
        SystemSetting.FrameRate = ( FRAME_RATE )curIndex;
        Debug.Log( ( FRAME_RATE )curIndex );
    }
}
