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
    Random,
    Half_Random,
    Max_Random,
    Count,
}

public enum GameFader
{
    None,
    Fade_In,
    Fade_Out,
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
    BGAPlay     = 1 << 0,
    TouchEffect = 1 << 1,
    LineEffect  = 1 << 2,
    ShowMeasure = 1 << 3,
    ShowJudge   = 1 << 4,

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
    public static GameMode       CurrentGameMode      = GameMode.None;
    public static GameRandom     CurrentRandom        = GameRandom.None;
    public static GameFader      CurrentFader         = GameFader.None;
    public static Alignment      CurrentGearAlignment = Alignment.Center;

    // PPU
    public static int PPU = 100; // pixel per unit

    // Speed
    private static int OriginScrollSpeed = 15;
    public static float ScrollSpeed
    {
        
        get { return OriginScrollSpeed * .0027f; }
        set
        {
            var speed = OriginScrollSpeed + Mathf.FloorToInt( value );
            if ( speed < 1 )
            {
                Debug.LogWarning( $"ScrollSpeed : {OriginScrollSpeed}" );
                return;
            }

            OriginScrollSpeed = speed;
            Debug.Log( $"ScrollSpeed : {OriginScrollSpeed}" );
        }
    }
    public static float Weight      { get { return ( 60f / NowPlaying.Inst.CurrentSong.medianBpm ) * ScrollSpeed; } }
    public static float PreLoadTime { get { return ( 1500f / Weight ); } }

    // Sound
    public static float SoundPitch = 1f;
    public static int   SoundOffset = 0;

    // Opacity
    public static float BGAOpacity   = 0f;
    public static float PanelOpacity = 0f;

    // IO
    public static readonly string SoundDirectoryPath = System.IO.Path.Combine( Application.streamingAssetsPath, "Songs" );
    public static readonly string FailedPath         = System.IO.Path.Combine( Application.streamingAssetsPath, "Failed" );

    // Measure
    public static float MeasureHeight = 3f;

    // Jugdement
    public static float JudgePos    = -530f;
    public static float JudgeHeight = 100f; // scaleY

    // note
    public static float NoteWidth  = 80f;
    public static float NoteHeight = 30f;
    public static float NoteBlank  = 2f;
    public static float NoteStartPos { get { return -( ( NoteWidth * 5f ) + ( NoteBlank * 7f ) ) * .5f; } }

    // Gear
    public static float GearStartPos { get { return ( -( ( NoteWidth * 6f ) + ( NoteBlank * 7f ) ) * .5f ); } }
    public static float GearWidth    { get { return (  ( NoteWidth * 6f ) + ( NoteBlank * 7f ) ); } }

    public Dictionary<GameKeyAction, KeyCode> Keys = new Dictionary<GameKeyAction, KeyCode>();
    private readonly KeyCode[] defaultKeys = new KeyCode[]
    {
        KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.K, KeyCode.L, KeyCode.Semicolon
    };

    private void Awake()
    {
        for ( int i = 0; i < defaultKeys.Length; i++ )
        {
            Keys.Add( ( GameKeyAction )i, defaultKeys[i] );
        }
    }
}
