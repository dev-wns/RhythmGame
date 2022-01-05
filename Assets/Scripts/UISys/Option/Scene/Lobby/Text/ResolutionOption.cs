using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class ResolutionOption : OptionText
{ 
    protected override void Awake()
    {
        base.Awake();

        curIndex = ( int )SystemSetting.Resolution;
        ChangeText( texts[curIndex] );
    }

    protected override void CreateObject()
    {
        StringBuilder builder = new StringBuilder();
        for ( int i = 0; i < ( int )RESOLUTION.Count; i++ )
        {
            var replace = ( ( RESOLUTION )i ).ToString().Replace( "_", " " );
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
                builder.Append( ( ( RESOLUTION )i ).ToString() );
            }

            texts.Add( builder.ToString() );
        }
    }

    public override void Process()
    {
        SystemSetting.Resolution = ( RESOLUTION )curIndex;
        Debug.Log( ( RESOLUTION )curIndex );
    }
}
