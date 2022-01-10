using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class ResolutionOption : OptionText
{ 
    protected override void Awake()
    {
        base.Awake();

        curIndex = ( int )SystemSetting.CurrentResolution;
        ChangeText( texts[curIndex] );
    }

    protected override void CreateObject()
    {
        StringBuilder builder = new StringBuilder();
        for ( int i = 0; i < ( int )Resolution.Count; i++ )
        {
            var replace = ( ( Resolution )i ).ToString().Replace( "_", " " );
            var split = replace.Trim().Split( ' ' );

            builder.Clear();
            if ( split.Length == 2 )
            {
                builder.Append( split[0] );
                builder.Append( "x" );
                builder.Append( split[1] );
            }
            else
            {
                builder.Append( ( ( Resolution )i ).ToString() );
            }

            texts.Add( builder.ToString() );
        }
    }

    public override void Process()
    {
        var replace = ( ( Resolution )curIndex ).ToString().Replace( "_", " " );
        var split = replace.Trim().Split( ' ' );

        var width  = int.Parse( split[0] );
        var height = int.Parse( split[1] );

        Screen.SetResolution( width, height, ( FullScreenMode )SystemSetting.CurrentScreenMode );
        SystemSetting.CurrentResolution = ( Resolution )curIndex;
    }
}
