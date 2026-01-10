
public enum ConfigType : int
{
    // User Section
    Email = 0,
    IsSavedEmail,

    // Sound Section
    SoundBuffer = 100,
    Master,
    BGM,
    SFX,

    // GameSetting Section
    Random = 200,
    AutoPlay,
    NoFailed,
    NoSlider,
    FixedBPM,
    ConvertKey,

    NoteModel,
    ScrollSpeed,
    JudgeOffset,
    SliderOffset,

    SoundOffset,

    BGAOpacity,
    PanelOpacity,
    GearOffsetX,
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