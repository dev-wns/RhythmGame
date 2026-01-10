using System;

public class GameSetting : Singleton<GameSetting>
{
    // Mode
    public static VisualFlag CurrentVisualFlag = VisualFlag.All;
    public static GameMode   CurrentGameMode   = GameMode.NoFail; // | GameMode.AutoPlay;
    public static GameRandom CurrentRandom     = GameRandom.None;
    public static PitchType  CurrentPitchType  = PitchType.None;
    public static NoteModel  CurrentNoteModel  = NoteModel.Block;

    // Scroll Speed
    public static int ScrollSpeed 
    {
        get => OriginScrollSpeed;
        set
        {
            if ( value < 0 || value > 50 )
                 return;

            OriginScrollSpeed = value;
            Config.Inst.Write( ConfigType.ScrollSpeed, OriginScrollSpeed );
            OnChangedSpeed?.Invoke( OriginScrollSpeed );
        }
    }
    private static int OriginScrollSpeed;
    public  static event Action<int> OnChangedSpeed;

    public static float Weight => ( ScrollSpeed / SoundPitch ) * 0.073810f; // 0.001030 // 0.069161f = 6.7 / 0.071193f = 6.8 / 0.071226f = 6.9 / 0.072258 = 7.0 / 0.073295 = 7.1 / 0.074325 = 7.2 / 0.075355 = 7.3
    public static float MinDistance => 1200f / Weight;

    // Sound
    public static float SoundPitch;
    public static int   SoundOffset;

    // Opacity ( 0 ~ 100 )
    public static int BGAOpacity;
    public static int PanelOpacity;

    // Measure
    public static float MeasureHeight => 2.5f;

    // Judgement
    public static float JudgePos => -435f + JudgeOffset;
    public static int   JudgeOffset;

    // Note
    public static int   SliderOffset  = 0;
    public static float NoteWidth     => 110.5f;// 125.5f;
    public static float NoteHeight    => 60.5f; // 63
    public static float NoteBlank     => 0f;
    public static float NoteStartPos { get; private set; }

    // Gear
    public static float GearOffsetX;
    public static float GearStartPos { get; private set; }
    public static float GearWidth    { get; private set; }

    public void UpdatePosition( int _keyCount )
    {
        NoteStartPos = GearOffsetX - ( ( NoteWidth * ( _keyCount - 1 ) ) + ( NoteBlank * ( _keyCount + 1 ) ) ) * .5f;
        GearStartPos = GearOffsetX - ( ( NoteWidth * _keyCount ) + ( NoteBlank * ( _keyCount + 1 ) ) ) * .5f;
        GearWidth    = ( NoteWidth * _keyCount ) + ( NoteBlank * ( _keyCount + 1 ) );
    }

    public void Initialize()
    {
        // Game Mode
        if ( !Config.Inst.Read( ConfigType.Random,     out CurrentRandom ) )                 CurrentRandom    = GameRandom.None;

        CurrentGameMode = GameMode.None;
        if (  Config.Inst.Read( ConfigType.AutoPlay,   out bool isAuto     ) && isAuto     ) CurrentGameMode |= GameMode.AutoPlay;
        if (  Config.Inst.Read( ConfigType.NoFailed,   out bool isNoFail   ) && isNoFail   ) CurrentGameMode |= GameMode.NoFail;
        if (  Config.Inst.Read( ConfigType.NoSlider,   out bool isNoSlider ) && isNoSlider ) CurrentGameMode |= GameMode.NoSlider;
        if (  Config.Inst.Read( ConfigType.FixedBPM,   out bool isFixedBPM ) && isFixedBPM ) CurrentGameMode |= GameMode.FixedBPM;
        if (  Config.Inst.Read( ConfigType.ConvertKey, out bool isConvert  ) && isConvert  ) CurrentGameMode |= GameMode.ConvertKey;

        // Game Setting
        if ( !Config.Inst.Read( ConfigType.NoteModel, out CurrentNoteModel ) ) CurrentNoteModel = NoteModel.Block;
        ScrollSpeed  = Config.Inst.Read( ConfigType.ScrollSpeed,  out int scrollSpeed  ) ? scrollSpeed  : 1;
        JudgeOffset  = Config.Inst.Read( ConfigType.JudgeOffset,  out int judgeOffset  ) ? judgeOffset  : 0;
        SliderOffset = Config.Inst.Read( ConfigType.SliderOffset, out int sliderOffset ) ? sliderOffset : 0;

        // Sound
        SoundPitch  = 1.0f;
        SoundOffset = Config.Inst.Read( ConfigType.SoundOffset,  out int soundOffset  ) ? soundOffset : 0;

        // UI
        BGAOpacity   = Config.Inst.Read( ConfigType.BGAOpacity,   out int   bgaOpacity   ) ? bgaOpacity   : 100;
        PanelOpacity = Config.Inst.Read( ConfigType.PanelOpacity, out int   panelOpacity ) ? panelOpacity : 100;
        GearOffsetX  = Config.Inst.Read( ConfigType.GearOffsetX,  out float gearOffsetX  ) ? gearOffsetX  : 0f;

        CurrentVisualFlag = VisualFlag.All;
        if ( Config.Inst.Read( ConfigType.Measure,    out bool showMeasure ) && !showMeasure ) CurrentVisualFlag &= ~VisualFlag.ShowMeasure;
        if ( Config.Inst.Read( ConfigType.HitEffect,  out bool hitEffect   ) && !hitEffect   ) CurrentVisualFlag &= ~VisualFlag.HitEffect;
        if ( Config.Inst.Read( ConfigType.LaneEffect, out bool laneEffect  ) && !laneEffect  ) CurrentVisualFlag &= ~VisualFlag.LaneEffect;
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

    public static string ToString( GameRandom _random )
    {
        return _random switch 
        {
            GameRandom.None         => "None",
            GameRandom.Mirror       => "Mirror",
            GameRandom.Basic_Random => "Basic",
            GameRandom.Half_Random  => "Half",
            GameRandom.Max_Random   => "Max",
            _                       => string.Empty,
        };
    }
}
