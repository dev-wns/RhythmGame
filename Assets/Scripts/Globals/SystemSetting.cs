using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum Resolution
{
    _1920_1080,
    _1600_1024,
    _1280_960,
    _1024_768,
    _800_600,
    Count,
}

public enum FrameRate
{
    vSync,
    No_Limit,
    _60,
    _144,
    _300,
    _960,
    Count,
}

public enum ScreenMode
{
    Exclusive_FullScreen,
    FullScreen_Window,
    Maximized_Window,
    Windowed,
    Count,
}

public class SystemSetting : MonoBehaviour
{
    public static Resolution Resolution  = Resolution._1920_1080;
    public static FrameRate FrameRate   = FrameRate.No_Limit;
    public static ScreenMode ScreenMod  = ScreenMode.Exclusive_FullScreen;
}
