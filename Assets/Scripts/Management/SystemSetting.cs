using UnityEngine;

public class SystemSetting : Singleton<SystemSetting>
{
    public static Resolution   CurrentResolution   = Resolution._1280_720;
    public static FrameRate    CurrentFrameRate    = FrameRate.No_Limit;
    public static ScreenMode   CurrentScreenMode   = ScreenMode.Windowed;
    public static SoundBuffer  CurrentSoundBuffer  = SoundBuffer._64;
    public static AntiAliasing CurrentAntiAliasing = AntiAliasing.None;

    public uint SoundBufferToInt( SoundBuffer _type )
    {
        return _type switch 
        {
            SoundBuffer._64   => 64,
            SoundBuffer._128  => 128,
            SoundBuffer._256  => 256,
            SoundBuffer._512  => 512,
            SoundBuffer._1024 => 1024,
            _                 => 1024,
        };
    }

    public Vector2 ResolustionToVector( Resolution _type )
    {
        return _type switch 
        {
            Resolution._1280_720  => new Vector2( 1280, 720 ),
            Resolution._1600_900  => new Vector2( 1600, 900 ),
            Resolution._1920_1080 => new Vector2( 1920, 1080 ),
            _                     => new Vector2( 1920, 1080 ),
        };
    }

    public int FrameRateToInt( FrameRate _type )
    {
        return _type switch 
        {
            FrameRate._60  => 60,
            FrameRate._144 => 144,
            FrameRate._240 => 240,
            FrameRate._360 => 360,
            FrameRate._480 => 480,
            FrameRate._960 => 960,
            _              => 0,
        };
    }

    public void ReadUserData()
    {
        CurrentResolution   = Config.Inst.Read( ConfigType.Resolution,   out int resolution   ) ? ( Resolution   )resolution   : Resolution._1920_1080;
        CurrentFrameRate    = Config.Inst.Read( ConfigType.FrameLimit,   out int frameRate    ) ? ( FrameRate    )frameRate    : FrameRate.vSync;
        CurrentAntiAliasing = Config.Inst.Read( ConfigType.AntiAliasing, out int antiAliasing ) ? ( AntiAliasing )antiAliasing : AntiAliasing.None;
        CurrentScreenMode   = Config.Inst.Read( ConfigType.ScreenMode,   out int screenMode   ) ? ( ScreenMode   )screenMode   : ScreenMode.Windowed;
        CurrentSoundBuffer  = Config.Inst.Read( ConfigType.SoundBuffer,  out int soundBuffer  ) ? ( SoundBuffer  )soundBuffer  : SoundBuffer._1024;

        ApplyScreenSetting();
    }

    public void ApplyScreenSetting()
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
            case FrameRate._960: Application.targetFrameRate = FrameRateToInt( CurrentFrameRate );
            break;
        }

        Vector2 screen = ResolustionToVector( CurrentResolution );
        switch ( CurrentScreenMode )
        {
            case ScreenMode.Exclusive_FullScreen:
            Screen.SetResolution( ( int )screen.x, ( int )screen.y, FullScreenMode.ExclusiveFullScreen );
            break;

            case ScreenMode.FullScreen_Window:
            Screen.SetResolution( ( int )screen.x, ( int )screen.y, FullScreenMode.FullScreenWindow );
            break;

            case ScreenMode.Windowed:
            Screen.SetResolution( ( int )screen.x, ( int )screen.y, FullScreenMode.Windowed );
            break;
        }
    }
}
