using System;
using UnityEngine;

public enum BooleanOption { Off, On, Count }

public enum Alignment { Left, Center, Right, Count, }

public enum PitchType { None, Normalize, Nightcore, Count, }

public enum GameRandom
{
    None,
    Mirror,
    Basic_Random,
    Half_Random,
    Max_Random,
    Count,
}

[Flags]
public enum GameMode
{
    None          = 0,
    AutoPlay      = 1 << 0,
    NoFail        = 1 << 1,
    NoSlider      = 1 << 2,
    FixedBPM      = 1 << 3,
    HardJudge     = 1 << 4,
    KeyConversion = 1 << 5,

    All      = int.MaxValue,
}

[Flags]
public enum GameVisualFlag
{
    None           = 0,
    TouchEffect    = 1 << 0,
    LaneEffect     = 1 << 1,
    ShowMeasure    = 1 << 2,

    All         = int.MaxValue,
}

public class GameSetting
{
    // Mode
    public static GameVisualFlag CurrentVisualFlag    = GameVisualFlag.All;
    public static GameMode       CurrentGameMode      = GameMode.NoFail; //GameMode.AutoPlay | GameMode.NoFail;
    public static GameRandom     CurrentRandom        = GameRandom.None;
    public static Alignment      CurrentGearAlignment = Alignment.Center;
    public static PitchType      CurrentPitchType     = PitchType.None;

    // Speed
    private static float OriginScrollSpeed = 6.0f; 
    public static float ScrollSpeed
    {

        get => OriginScrollSpeed;
        set
        {
            if ( value < 1d ) return;
            OriginScrollSpeed = value;
        }
    }

    public static float Weight => ScrollSpeed * 350f;
    public static float MinDistance => 1200f / Weight;

    // Sound
    public static int SoundOffset = 0;

    // Opacity Percentage ( 0 ~ 100 )
    public static int BGAOpacity   = 100;
    public static int PanelOpacity = 100;

    // IO
    public static readonly string SoundDirectoryPath = System.IO.Path.Combine( Application.streamingAssetsPath, "Songs" );
    public static readonly string FailedPath         = System.IO.Path.Combine( Application.streamingAssetsPath, "Failed" );
    public static readonly string RecordPath         = System.IO.Path.Combine( Application.streamingAssetsPath, "Records" );
    public static readonly string RecordFileName     = "Record.json";

    // Measure
    public static float MeasureHeight = 2.5f;

    // Jugdement
    private static float DefaultJudgePos = -435f;
    public static float JudgePos => DefaultJudgePos + JudgeOffset;
    public static int JudgeOffset = 0;
    public static float JudgeHeight = 50f;

    // note
    public static float NoteSizeMultiplier = 1f;
    public static float NoteWidth  => 110.5f * NoteSizeMultiplier; // 111
    public static float NoteBodyWidth => 110.5f * NoteSizeMultiplier;
    public static float NoteHeight => 63f * NoteSizeMultiplier;
    public static float NoteBlank  = 0f;
    public static float NoteStartPos => GearOffsetX + -( ( NoteWidth * ( NowPlaying.KeyCount - 1 ) ) + ( NoteBlank * ( NowPlaying.KeyCount + 1 ) ) ) * .5f;

    // Gear
    public static float GearOffsetX = 0f;
    public static float GearStartPos => GearOffsetX + ( -( ( NoteWidth * NowPlaying.KeyCount ) + ( NoteBlank * ( NowPlaying.KeyCount + 1 ) ) ) * .5f );
    public static float GearWidth    => ( ( NoteWidth * NowPlaying.KeyCount ) + ( NoteBlank * ( NowPlaying.KeyCount + 1 ) ) );

    // Pitch
    private static int pitch = 100;
    public static float CurrentPitch { get { return pitch * .01f; } set { pitch = ( int )value; } }

    // PPU
    public static int PPU = 100; // pixel per unit

    // Debug
    public static bool IsAutoRandom = false;
    public static bool UseClapSound = false;
}
