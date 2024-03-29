public enum Resolution   { _1920_1080, _1600_1024, _1280_960, _1024_768, _800_600, Count, }
public enum FrameRate    { vSync, No_Limit, _60, _144, _240, _960, Count, }
public enum ScreenMode   { Exclusive_FullScreen, FullScreen_Window, Windowed, Count, }
public enum AntiAliasing { None, _2xMultiSampling, _4xMultiSampling, _8xMultiSampling, Count, }

public class SystemSetting
{
    public static Resolution   CurrentResolution   = Resolution._1920_1080;
    public static FrameRate    CurrentFrameRate    = FrameRate.No_Limit;
    public static ScreenMode   CurrentScreenMode   = ScreenMode.FullScreen_Window;
    public static SoundBuffer  CurrentSoundBuffer  = SoundBuffer._64;
    public static AntiAliasing CurrentAntiAliasing = AntiAliasing.None;

    public static string CurrentSoundBufferString => CurrentSoundBuffer.ToString().Replace( "_", " " ).Trim();
}
