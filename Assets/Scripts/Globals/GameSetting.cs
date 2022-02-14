using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum BooleanOption { Off, On, Count }

public enum Alignment { Left, Center, Right, Count, }

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
    None     = 0,
    AutoPlay = 1 << 0,
    NoFail   = 1 << 1,
    NoSlider = 1 << 2,

    All      = int.MaxValue,
}

[Flags]
public enum GameVisualFlag
{
    None        = 0,
    TouchEffect = 1 << 0,
    LaneEffect  = 1 << 1,
    ShowMeasure = 1 << 2,

    All         = int.MaxValue,
}

public enum GameKeyAction : int
{
    _0, _1, _2, _3, _4, _5, Count // InGame Input Keys
};

public class GameSetting : SingletonUnity<GameSetting>
{
    // Mode
    public static GameVisualFlag CurrentVisualFlag    = GameVisualFlag.All;
    public static GameMode       CurrentGameMode      = GameMode.AutoPlay;
    public static GameRandom     CurrentRandom        = GameRandom.None;
    public static Alignment      CurrentGearAlignment = Alignment.Center;

    // PPU
    public static int PPU = 100; // pixel per unit

    // Speed
    private static double OriginScrollSpeed = 6.8d;
    public static double ScrollSpeed
    {

        get => OriginScrollSpeed;
        set
        {
            if ( value < 1 )
            {
                Debug.LogWarning( $"ScrollSpeed : {OriginScrollSpeed}" );
                return;
            }

            OriginScrollSpeed = value;
            Debug.Log( $"ScrollSpeed : {OriginScrollSpeed}" );
        }
    }
    public static double Weight => ( 320d * ScrollSpeed ) / NowPlaying.Inst.CurrentSong.medianBpm;
    public static double PreLoadTime => ( 1500d / Weight );

    // Sound
    public static int SoundOffset = 0;

    // Opacity Percentage ( 0 ~ 100 )
    public static float BGAOpacity   = 100f;
    public static float PanelOpacity = 100f;

    // IO
    public static readonly string SoundDirectoryPath = System.IO.Path.Combine( Application.streamingAssetsPath, "Songs" );
    public static readonly string FailedPath         = System.IO.Path.Combine( Application.streamingAssetsPath, "Failed" );

    // Measure
    public static float MeasureHeight = 3f;

    // Jugdement
    public static float JudgePos    = -490f;
    public static float JudgeHeight = 100f; // scaleY

    // note
    public static float NoteWidth  = 80f;
    public static float NoteHeight = 30f;
    public static float NoteBlank  = 2f;
    public static float NoteStartPos => -( ( NoteWidth * 5f ) + ( NoteBlank * 7f ) ) * .5f;

    // Gear
    public static float GearStartPos => ( -( ( NoteWidth * 6f ) + ( NoteBlank * 7f ) ) * .5f );
    public static float GearWidth    => ( ( NoteWidth * 6f ) + ( NoteBlank * 7f ) );

    public Dictionary<GameKeyAction, KeyCode> Keys = new Dictionary<GameKeyAction, KeyCode>();
    private readonly KeyCode[] defaultKeys = new KeyCode[]
    {
        KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.L, KeyCode.Semicolon, KeyCode.Quote,
    };

    private void Awake()
    {
        for ( int i = 0; i < defaultKeys.Length; i++ )
        {
            Keys.Add( ( GameKeyAction )i, defaultKeys[i] );
        }
    }
}
