using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    ShowGearKey = 1 << 3,

    All         = int.MaxValue,
}

public class GameSetting
{
    // Mode
    public static GameVisualFlag CurrentVisualFlag    = GameVisualFlag.All;
    public static GameMode       CurrentGameMode      = GameMode.AutoPlay | GameMode.NoFail;
    public static GameRandom     CurrentRandom        = GameRandom.None;
    public static Alignment      CurrentGearAlignment = Alignment.Center;
    public static PitchType      CurrentPitchType     = PitchType.None;

    // Speed
    private static double OriginScrollSpeed = 9.1d; 
    public static double ScrollSpeed
    {

        get => OriginScrollSpeed;
        set
        {
            if ( value < 1d ) return;
            OriginScrollSpeed = value;
        }
    }
    
    /// ä������ �ٸ� BPM���� ���� �ӵ��� �����ϰ� �ϱ�����
    /// �ش� ä������ ���� ���� ���ӵǴ� BPM���� ����Ͽ� ��ũ�Ѽӵ��� �����Ѵ�.
    /// - ���� : A + ( Speed * ( B / BPM ) )
    /// 
    /// - A : �ּ� �ӵ� 
    /// 270BPM 16��Ʈ ��Ÿ( ��Ʈ�� )��� �ӵ��� 1.7�϶� ��Ʈ�� ��ġ�� �ʾұ� ������ �˳��� �ּ� �ӵ� 1�� �߰���.
    /// 
    /// - B : 1�� ����Ǵ� �ӵ��� ũ��
    /// ���� �������� ������ ���� ����
    public static double Weight => 1d + ( ScrollSpeed * ( 240d / ( NowPlaying.Inst.CurrentSong.medianBpm * CurrentPitch ) ) );
    public static double PreLoadTime => 1200d / Weight;

    // Sound
    public static int SoundOffset = 0;

    // Opacity Percentage ( 0 ~ 100 )
    public static float BGAOpacity   = 100f;
    public static float PanelOpacity = 100f;

    // IO
    public static readonly string SoundDirectoryPath = System.IO.Path.Combine( Application.streamingAssetsPath, "Songs" );
    public static readonly string FailedPath         = System.IO.Path.Combine( Application.streamingAssetsPath, "Failed" );

    // Measure
    public static float MeasureHeight = 2f;

    // Jugdement
    private static float DefaultJudgePos = -340f;
    public static float JudgePos => DefaultJudgePos + JudgeOffset;
    public static float JudgeOffset = 0f;
    public static float JudgeHeight = 50f;

    // note
    public static float NoteWidth  = 80f; // 83f; // 75f
    public static float NoteHeight = 80f; // 65f; // 90f; // 1.28125
    public static float NoteBlank  = 0f; //7.5f;
    public static float NoteStartPos => -( ( NoteWidth * 5f ) + ( NoteBlank * 7f ) ) * .5f;

    // Gear
    public static float GearStartPos => ( -( ( NoteWidth * 6f ) + ( NoteBlank * 7f ) ) * .5f );
    public static float GearWidth    => ( ( NoteWidth * 6f ) + ( NoteBlank * 7f ) );

    // Pitch
    private static int pitch = 100;
    public static float CurrentPitch { get { return pitch * .01f; } set { pitch = ( int )value; } }

    // PPU
    public static int PPU = 100; // pixel per unit

    // Debug
    public static bool IsAutoRandom = false;
}
