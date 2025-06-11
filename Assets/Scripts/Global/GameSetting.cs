using System;

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

public class GameSetting
{
    // Mode
    public static VisualFlag     CurrentVisualFlag    = VisualFlag.All;
    public static GameMode       CurrentGameMode      = GameMode.NoFail; // | GameMode.AutoPlay;
    public static GameRandom     CurrentRandom        = GameRandom.None;
    public static Alignment      CurrentAlignment     = Alignment.Center;
    public static PitchType      CurrentPitchType     = PitchType.None;

    public static bool HasFlag<T>( T _type ) where T : Enum
    {
        try
        { 
            switch ( _type )
            {
                case VisualFlag: return CurrentVisualFlag.HasFlag( _type );
                case GameMode:   return CurrentGameMode.HasFlag(   _type );
                case GameRandom: return CurrentRandom.HasFlag(     _type );
                case Alignment:  return CurrentAlignment.HasFlag(  _type );
                case PitchType:  return CurrentPitchType.HasFlag(  _type );
                default:         throw new Exception( $"{_type} is Invalid Value" );
            }
        }
        catch( Exception _error )
        {
            Debug.LogError( _error );
        }

        return false;
    }

    // Speed
    private static int OriginScrollSpeed = 30;
    public static int ScrollSpeed
    {

        get => OriginScrollSpeed;
        set
        {
            if ( value < 1 ) return;
            OriginScrollSpeed = value;
        }
    }

    public static float Weight => ScrollSpeed / 12.7037f; // ( 13720 / 1080 )
    public static float MinDistance => 1200f / Weight;

    // Sound
    public static int SoundOffset  = -50;

    // Opacity ( 0 ~ 100 )
    public static int BGAOpacity   = 10;
    public static int PanelOpacity = 100;

    // Measure
    public static float MeasureHeight = 2.5f;

    // Judgement
    private static float DefaultJudgePos = -435f;
    public static float JudgePos    => DefaultJudgePos + JudgeOffset;
    public static int   JudgeOffset = -33;
    public static float JudgeHeight = 50f;

    // note
    public static float NoteSizeMultiplier = 1f;
    public static float NoteWidth     => 110.5f * NoteSizeMultiplier; // 111
    public static float NoteHeight    => 63f    * NoteSizeMultiplier;
    public static float NoteBodyWidth => 110.5f * NoteSizeMultiplier;
    public static float NoteBlank     =  0f;
    public static float NoteStartPos  => GearOffsetX + -( ( NoteWidth * ( NowPlaying.KeyCount - 1 ) ) + ( NoteBlank * ( NowPlaying.KeyCount + 1 ) ) ) * .5f;

    // Gear
    public static float GearOffsetX  = 0f;
    public static float GearStartPos => GearOffsetX + ( -( ( NoteWidth * NowPlaying.KeyCount ) + ( NoteBlank * ( NowPlaying.KeyCount + 1 ) ) ) * .5f );
    public static float GearWidth    => ( ( NoteWidth * NowPlaying.KeyCount ) + ( NoteBlank * ( NowPlaying.KeyCount + 1 ) ) );

    // Pitch
    private static float pitch = 1f;
    public static float CurrentPitch { get => pitch; set => pitch = value; }

    // PPU
    public static int PPU = 100; // pixel per unit

    // Debug
    public static bool IsAutoRandom = false;
    public static bool UseClapSound = false;
}
