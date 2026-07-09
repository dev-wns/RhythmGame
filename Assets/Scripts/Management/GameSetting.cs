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
            if ( value < 1 || value > 50 )
                 return;

            OriginScrollSpeed = value;
            Config.Inst.Write( ConfigType.ScrollSpeed, OriginScrollSpeed );
            OnChangedSpeed?.Invoke( OriginScrollSpeed );
        }
    }
    private static int OriginScrollSpeed;
    public  static event Action<int> OnChangedSpeed;

    public static float Weight => ( ( 9 + ScrollSpeed ) * 0.039638f ) / SoundPitch;
    // 0.001030 // 0.069161f = 6.7 / 0.071193f = 6.8 / 0.071226f = 6.9 / 0.072258 = 7.0 / 0.073295 = 7.1 / 0.074325 = 7.2 / 0.075355 = 7.3
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

    public Song ApplyGameMode( in Song _song )
    {
        Song newSong = _song;
        //newSong.mainBPM = _song.mainBPM * SoundPitch;

        // 게임 모드가 적용되어 바뀐 정보
        if ( CurrentGameMode.HasFlag( GameMode.ConvertKey ) &&  _song.keyCount == 7 )
        {
            newSong.keyCount     = 6;
            newSong.noteCount   -= _song.delNoteCount;
            newSong.sliderCount -= _song.delSliderCount;
        }
        
        if ( CurrentGameMode.HasFlag( GameMode.NoSlider ) )
        {
            newSong.noteCount  += newSong.sliderCount;
            newSong.sliderCount = 0;
        }

        // 프리뷰 지점이 없을때 중간부터 재생
        newSong.previewTime = _song.previewTime <= 0 || _song.previewTime > _song.totalTime ? ( int )( _song.totalTime * .35f ) : _song.previewTime;

        int keyCount = newSong.keyCount;
        NoteStartPos = GearOffsetX - ( ( NoteWidth * ( keyCount - 1 ) ) + ( NoteBlank * ( keyCount + 1 ) ) ) * .5f;
        GearStartPos = GearOffsetX - ( ( NoteWidth *   keyCount       ) + ( NoteBlank * ( keyCount + 1 ) ) ) * .5f;
        GearWidth    = ( NoteWidth * keyCount ) + ( NoteBlank * ( keyCount + 1 ) );

        return newSong;
    }

    public void ReadUserData()
    {
        // Game Mode
        CurrentRandom     = Config.Inst.Read( ConfigType.Random,    out int random ) ? ( GameRandom )random : GameRandom.None;
        CurrentGameMode   = Config.Inst.Read( ConfigType.GameMode,  out int mode   ) ? ( GameMode   )mode   : GameMode.None;
        CurrentVisualFlag = Config.Inst.Read( ConfigType.VisualFag, out int visual ) ? ( VisualFlag )visual : VisualFlag.None;

        // Game Setting
        CurrentNoteModel = Config.Inst.Read( ConfigType.NoteModel,    out int model        ) ? ( NoteModel )model : NoteModel.Block;
        ScrollSpeed      = Config.Inst.Read( ConfigType.ScrollSpeed,  out int scrollSpeed  ) ? scrollSpeed  : 1;
        JudgeOffset      = Config.Inst.Read( ConfigType.JudgeOffset,  out int judgeOffset  ) ? judgeOffset  : 0;
        SliderOffset     = Config.Inst.Read( ConfigType.SliderOffset, out int sliderOffset ) ? sliderOffset : 0;

        // Sound
        SoundPitch  = 1.0f;
        SoundOffset = Config.Inst.Read( ConfigType.SoundOffset,  out int soundOffset  ) ? soundOffset : 0;

        // UI
        BGAOpacity   = Config.Inst.Read( ConfigType.BGAOpacity,   out int   bgaOpacity   ) ? bgaOpacity   : 100;
        PanelOpacity = Config.Inst.Read( ConfigType.PanelOpacity, out int   panelOpacity ) ? panelOpacity : 100;
        GearOffsetX  = Config.Inst.Read( ConfigType.GearOffsetX,  out float gearOffsetX  ) ? gearOffsetX  : 0f;
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
