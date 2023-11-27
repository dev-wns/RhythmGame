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
                case ScreenMode.Exclusive_FullScreen: texts.Add( $"��üȭ��" );          break;
                case ScreenMode.FullScreen_Window:    texts.Add( $"�׵θ� ���� â���" ); break;
                case ScreenMode.Windowed:             texts.Add( $"â���" );            break;
            }
        }
    }

    public override void Process()
    {
        var replace = SystemSetting.CurrentResolution.ToString().Replace( "_", " " );
        var split = replace.Trim().Split( ' ' );

        var width  = int.Parse( split[0] );
        var height = int.Parse( split[1] );

        var type = ( ScreenMode )CurrentIndex;
        switch ( type )
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

        SystemSetting.CurrentScreenMode = type;
    }
}
