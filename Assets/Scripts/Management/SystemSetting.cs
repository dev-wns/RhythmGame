using UnityEngine;

public class SystemSetting : Singleton<SystemSetting>
{
    public static Resolution   CurrentResolution   = Resolution._1280_720;
    public static FrameRate    CurrentFrameRate    = FrameRate.No_Limit;
    public static ScreenMode   CurrentScreenMode   = ScreenMode.Windowed;
    public static SoundBuffer  CurrentSoundBuffer  = SoundBuffer._64;
    public static AntiAliasing CurrentAntiAliasing = AntiAliasing.None;
    public static PollingRate  CurrentPollingRate  = PollingRate._3000;
    public static int InputTargetFrame => int.Parse( CurrentPollingRate.ToString().Replace( "_", " " ) );
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
