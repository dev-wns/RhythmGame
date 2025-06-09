using System.Text;
using UnityEngine;

public class ResolutionOption : OptionText
{
    private void OnEnable()
    {
        CurrentIndex = ( int )SystemSetting.CurrentResolution;
        ChangeText( texts[CurrentIndex] );
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
        var replace = ( ( Resolution )CurrentIndex ).ToString().Replace( "_", " " );
        var split = replace.Trim().Split( ' ' );

        var width  = int.Parse( split[0] );
        var height = int.Parse( split[1] );

        switch ( SystemSetting.CurrentScreenMode )
        {
            case ScreenMode.Exclusive_FullScreen:
            Screen.SetResolution( width, height, FullScreenMode.ExclusiveFullScreen );
            break;

            case ScreenMode.FullScreen_Window:
            Screen.SetResolution( width, height, FullScreenMode.FullScreenWindow );
            break;

            case ScreenMode.Windowed:
            Screen.SetResolution( width, height, FullScreenMode.Windowed );
            break;
        }

        SystemSetting.CurrentResolution = ( Resolution )CurrentIndex;
    }
}
