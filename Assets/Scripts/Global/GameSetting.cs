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

public class GameSetting : Singleton<GameSetting>
{
    // Mode
    public static VisualFlag     CurrentVisualFlag    = VisualFlag.All;
    public static GameMode       CurrentGameMode      = GameMode.NoFail; // | GameMode.AutoPlay;
    public static GameRandom     CurrentRandom        = GameRandom.None;
    public static Alignment      CurrentAlignment     = Alignment.Center;
    public static PitchType      CurrentPitchType     = PitchType.None;

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

    public static float Weight => ScrollSpeed / 12.50f;// ( ScrollSpeed / 13.720f ); //ScrollSpeed / 12.7037f;
    public static float MinDistance => 1200f / Weight;

    // Sound
    public static int SoundOffset  = -50;
    public static int ScreenOffset = -25;
    public static int LNOffset     = -25;

    // Opacity ( 0 ~ 100 )
    public static int BGAOpacity   = 10;
    public static int PanelOpacity = 100;

    // Measure
    public static float MeasureHeight = 2.5f;

    // Judgement
    public static float JudgePos         => DefaultJudgePos + JudgeOffset;
    public static int   JudgeOffset      = -33;
    public static float JudgeHeight      = 50f;
    private static float DefaultJudgePos = -435f;

    // note
    public static float NoteSizeMultiplier = 1f;
    public static float NoteWidth     => 125.5f; //120f; // 110.5
    public static float NoteHeight    => 70f; //70f; // 63
    public static float NoteBodyWidth => 110.5f;
    public static float NoteBlank     =  0f;
    public static float NoteStartPos  => GearOffsetX - ( ( NoteWidth * ( NowPlaying.KeyCount - 1 ) ) + ( NoteBlank * ( NowPlaying.KeyCount + 1 ) ) ) * .5f;

    // Gear
    public static float GearOffsetX  = 0f;
    public static float GearStartPos => GearOffsetX - ( ( NoteWidth * NowPlaying.KeyCount ) + ( NoteBlank * ( NowPlaying.KeyCount + 1 ) ) ) * .5f;
    public static float GearWidth    => ( NoteWidth * NowPlaying.KeyCount ) + ( NoteBlank * ( NowPlaying.KeyCount + 1 ) );

    // Pitch
    public  static float CurrentPitch { get => pitch; set => pitch = value; }
    private static float pitch = 1f;

    protected override void Awake()
    {
        ScrollSpeed  = Config.Inst.Read( ConfigType.ScrollSpeed,  out int   scrollSpeed  ) ? scrollSpeed  : 30;
        SoundOffset  = Config.Inst.Read( ConfigType.SoundOffset,  out int   soundOffset  ) ? soundOffset  : 0;
        JudgeOffset  = Config.Inst.Read( ConfigType.JudgeOffset,  out int   judgeOffset  ) ? judgeOffset  : 0;
        BGAOpacity   = Config.Inst.Read( ConfigType.BGAOpacity,   out int   bgaOpacity   ) ? bgaOpacity   : 0;
        PanelOpacity = Config.Inst.Read( ConfigType.PanelOpacity, out int   panelOpacity ) ? panelOpacity : 0;
        GearOffsetX  = Config.Inst.Read( ConfigType.GearOffsetX,  out float gearOffsetX  ) ? gearOffsetX  : 0f;
         
        CurrentGameMode = GameMode.None;
        if ( Config.Inst.Read( ConfigType.AutoPlay, out bool isAuto      ) && isAuto     ) CurrentGameMode |= GameMode.AutoPlay;
        if ( Config.Inst.Read( ConfigType.NoFailed, out bool isNoFail    ) && isNoFail   ) CurrentGameMode |= GameMode.NoFail;
        if ( Config.Inst.Read( ConfigType.NoSlider, out bool isNoSlider  ) && isNoSlider ) CurrentGameMode |= GameMode.NoSlider;
        if ( Config.Inst.Read( ConfigType.FixedBPM, out bool isFixedBPM  ) && isFixedBPM ) CurrentGameMode |= GameMode.FixedBPM;
        if ( Config.Inst.Read( ConfigType.ConvertKey, out bool isConvert ) && isConvert  ) CurrentGameMode |= GameMode.ConvertKey;

        CurrentVisualFlag = VisualFlag.None;
        if ( Config.Inst.Read( ConfigType.Measure,    out bool showMeasure ) && showMeasure ) CurrentVisualFlag |= VisualFlag.ShowMeasure;
        if ( Config.Inst.Read( ConfigType.HitEffect,  out bool hitEffect   ) && hitEffect   ) CurrentVisualFlag |= VisualFlag.HitEffect;
        if ( Config.Inst.Read( ConfigType.LaneEffect, out bool laneEffect  ) && laneEffect  ) CurrentVisualFlag |= VisualFlag.LaneEffect;
    }

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
                default: throw new Exception( $"{_type} is Invalid Value" );
            }
        }
        catch ( Exception _error )
        {
            Debug.LogError( _error );
        }

        return false;
    }
}
