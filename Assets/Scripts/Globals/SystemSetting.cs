
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

public class SystemSetting
{
    public static Resolution  CurrentResolution  = Resolution._1920_1080;
    public static FrameRate   CurrentFrameRate   = FrameRate.No_Limit;
    public static ScreenMode  CurrentScreenMode  = ScreenMode.Exclusive_FullScreen;
    public static SoundBuffer CurrentSoundBuffer = SoundBuffer._256;
    public static string CurrentSoundBufferString => CurrentSoundBuffer.ToString().Replace( "_", " " ).Trim();
}
