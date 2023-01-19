using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class ScreenModeOption : OptionText
{
    private void OnEnable()
    {
        CurrentIndex = ( int )SystemSetting.CurrentScreenMode;
        ChangeText( texts[CurrentIndex] );
    }

    protected override void CreateObject()
    {
        StringBuilder builder = new StringBuilder();
        for ( int i = 0; i < ( int )ScreenMode.Count; i++ )
        {
            if ( ( ScreenMode )i == ScreenMode.Maximized_Window ) continue;

            var text = ( ( ScreenMode )i ).ToString();
            builder.Clear();
            builder.Append( text.Replace( "_", " " ).Trim() );

            texts.Add( builder.ToString() );
        }
    }

    public override void Process()
    {
        var type = ( ScreenMode )CurrentIndex;
        switch ( type )
        {
            case ScreenMode.Maximized_Window: // mac Àü¿ë
            break;

            default:
            {
                var replace = ( ( Resolution )CurrentIndex ).ToString().Replace( "_", " " );
                var split = replace.Trim().Split( ' ' );

                var width = int.Parse( split[0] );
                var height = int.Parse( split[1] );

                Screen.SetResolution( width, height, ( FullScreenMode )type );
            }
            break;
        }

        SystemSetting.CurrentScreenMode = type;
    }
}
