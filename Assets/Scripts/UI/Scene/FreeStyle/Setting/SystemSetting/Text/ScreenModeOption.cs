using UnityEngine;

public class ScreenModeOption : OptionText
{
    private void OnEnable()
    {
        CurrentIndex = ( int )SystemSetting.CurrentScreenMode;
        ChangeText( texts[CurrentIndex] );
    }

    protected override void CreateObject()
    {
        for ( int i = 0; i < ( int )ScreenMode.Count; i++ )
        {
            switch ( ( ScreenMode )i )
            {
                case ScreenMode.Exclusive_FullScreen: texts.Add( $"��üȭ��" );    break;
                case ScreenMode.FullScreen_Window:    texts.Add( $"��ü â���" ); break;
                case ScreenMode.Windowed:             texts.Add( $"â���" );      break;
            }
        }
    }

    public override void Process()
    {
        var type = ( ScreenMode )CurrentIndex;
        switch ( type )
        {
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
