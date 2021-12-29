using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenMode : CustomButton
{
    public FullScreenMode screenMode;

    public override void Process()
    {
        base.Process();
        screenMode = ( FullScreenMode )key;
        Debug.Log( $"ScreenMode {screenMode}" );

        // Screen.SetResolution( 1920, 1080, screenMode );
    }
}
