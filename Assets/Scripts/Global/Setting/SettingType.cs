using System;

// ==================================================================
// ========================= System Setting =========================
// ==================================================================
public enum Resolution   { _1920_1080, _1600_900, _1280_720, Count, }
public enum FrameRate    { vSync, No_Limit, _60, _144, _240, _360, _480, _960, Count, }
public enum ScreenMode   { Exclusive_FullScreen, FullScreen_Window, Windowed, Count, }
public enum AntiAliasing { None = 0, _2xMultiSampling = 2, _4xMultiSampling = 4, _8xMultiSampling = 8, Count, }
public enum SoundBuffer  { _64, _128, _256, _512, _1024, Count, }
public enum PollingRate  { _125, _500, _1000, _3000, _8000, Count, }

// ==================================================================
// ========================== Game Setting ==========================
// ==================================================================
public enum PitchType  { None, Normalize, Nightcore, Count, }
public enum NoteModel  { Block, BlockFade, OnlyBody, Round, RoundFade, Circle, Count, }
public enum GameRandom { None, Mirror, Basic_Random, Half_Random, Max_Random, Count, }

[Flags]
public enum GameMode
{
    None       = 0,
    AutoPlay   = 1 << 0,
    NoFail     = 1 << 1,
    NoSlider   = 1 << 2,
    FixedBPM   = 1 << 3,
    ConvertKey = 1 << 4,

    All      = int.MaxValue,
}

[Flags]
public enum VisualFlag
{
    None        = 0,
    HitEffect   = 1 << 0,
    LaneEffect  = 1 << 1,
    ShowMeasure = 1 << 2,

    All         = int.MaxValue,
}