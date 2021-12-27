using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenMode : LobbyOptionButton
{
    public FullScreenMode screenMode;

    public override void Process()
    {
        screenMode = ( FullScreenMode )key;
        Debug.Log( $"ScreenMode {screenMode}" );

        // Screen.SetResolution( 1920, 1080, screenMode );
    }
}
