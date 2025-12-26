
public enum ConfigType : int
{
    // User Section
    Email = 0,
    IsSavedEmail,
    ScrollSpeed,
    SoundOffset,
    JudgeOffset,

    // Sound Section
    SoundBuffer = 100,
    Master,
    BGM,
    SFX,

    // GameSetting Section
    BGAOpacity = 200,
    PanelOpacity,
    GearOffsetX,
    AutoPlay,
    NoFailed,
    NoSlider,
    FixedBPM,
    ConvertKey,
    Measure,
    HitEffect,
    LaneEffect,

    // SystemSetting Section
    Resolution = 300,
    FrameLimit,
    AntiAliasing,
    ScreenMode,
    PollingRate,

    // KeySetting0 Section
    _4K = 400,
    _6K,
    _7K,
}