using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum RESOLUTION
{
    _1920_1080,
    _1600_1024,
    _1280_960,
    _1024_768,
    _800_600,
    Count,
}

public enum FRAME_RATE
{
    vSync,
    No_Limit,
    _60,
    _144,
    _300,
    _960,
    Count,
}

public enum SCREEN_MODE
{
    Exclusive_FullScreen,
    FullScreen_Window,
    Maximized_Window,
    Windowed,
    Count,
}

public class SystemSetting : MonoBehaviour
{
    public static RESOLUTION Resolution  = RESOLUTION._1920_1080;
    public static FRAME_RATE FrameRate   = FRAME_RATE.No_Limit;
    public static SCREEN_MODE ScreenMod  = SCREEN_MODE.Exclusive_FullScreen;
}
