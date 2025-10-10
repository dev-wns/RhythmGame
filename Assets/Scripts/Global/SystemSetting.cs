using UnityEngine;

public enum Resolution { _1920_1080, _1600_900, _1280_720, Count, }
public enum FrameRate { vSync, No_Limit, _60, _144, _240, _360, _480, _960, Count, }
public enum ScreenMode { Exclusive_FullScreen, FullScreen_Window, Windowed, Count, }
public enum AntiAliasing { None = 0, _2xMultiSampling = 2, _4xMultiSampling = 4, _8xMultiSampling = 8, Count, }
public enum SoundBuffer { _64, _128, _256, _512, _1024, Count, }
public enum PollingRate { _125, _500, _1000, _3000, _8000, Count, }

public class SystemSetting : Singleton<SystemSetting>
{
    public static Resolution   CurrentResolution   = Resolution._1280_720;
    public static FrameRate    CurrentFrameRate    = FrameRate.No_Limit;
    public static ScreenMode   CurrentScreenMode   = ScreenMode.Windowed;
    public static SoundBuffer  CurrentSoundBuffer  = SoundBuffer._64;
    public static AntiAliasing CurrentAntiAliasing = AntiAliasing.None;
    public static PollingRate  CurrentPollingRate  = PollingRate._3000;
    public static int InputTargetFrame 
    { 
        get
        {
            int pollingRate = int.Parse( CurrentPollingRate.ToString().Replace( "_", " " ) );
            Debug.Log( $"Set PollingRate {pollingRate} hz" );

            return pollingRate;
        }
    }

    public static string CurrentSoundBufferString => CurrentSoundBuffer.ToString().Replace( "_", " " ).Trim();

    protected override void Awake()
    {
        base.Awake();
        if ( !Config.Inst.Read( ConfigType.Resolution,   out CurrentResolution   ) ) CurrentResolution   = Resolution._1920_1080;
        if ( !Config.Inst.Read( ConfigType.FrameLimit,   out CurrentFrameRate    ) ) CurrentFrameRate    = FrameRate.vSync;
        if ( !Config.Inst.Read( ConfigType.AntiAliasing, out CurrentAntiAliasing ) ) CurrentAntiAliasing = AntiAliasing.None;
        if ( !Config.Inst.Read( ConfigType.ScreenMode,   out CurrentScreenMode   ) ) CurrentScreenMode   = ScreenMode.Windowed;
        if ( !Config.Inst.Read( ConfigType.SoundBuffer,  out CurrentSoundBuffer  ) ) CurrentSoundBuffer  = SoundBuffer._1024;
        if ( !Config.Inst.Read( ConfigType.PollingRate,  out CurrentPollingRate  ) ) CurrentPollingRate  = PollingRate._1000;

        UpdateScreen();
    }

    public void UpdateScreen()
    {
        QualitySettings.antiAliasing = ( int )CurrentAntiAliasing;
        QualitySettings.vSyncCount   = CurrentFrameRate == FrameRate.vSync ? 1 : 0;
        switch ( CurrentFrameRate )
        {
            case FrameRate.No_Limit: Application.targetFrameRate = 0; break;
            case FrameRate._60:
            case FrameRate._144:
            case FrameRate._240:
            case FrameRate._360:
            case FrameRate._480:
            case FrameRate._960:
            {
                var frame = ( CurrentFrameRate ).ToString().Replace( "_", " " );
                Application.targetFrameRate = int.Parse( frame );
            } break;
        }

        var replace = CurrentResolution.ToString().Replace( "_", " " );
        var split   = replace.Trim().Split( ' ' );
        var width   = int.Parse( split[0] );
        var height  = int.Parse( split[1] );
        switch ( CurrentScreenMode )
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
    }
}
