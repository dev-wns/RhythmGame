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
public enum GameMod
{
    None     = 0,
    AutoPlay = 1 << 0,
    NoFail   = 1 << 1,
    NoSlider = 1 << 2,

    All      = int.MaxValue,
}

[Flags]
public enum VisualMod
{
    None        = 0,
    BGAPlay     = 1 << 0,
    TouchEffect = 1 << 1,
    LineEffect  = 1 << 2,
    ShowMeasure = 1 << 3,
    ShowJudge   = 1 << 4,

    All         = int.MaxValue,
}

public class GameSetting : MonoBehaviour
{
    public static GameMod    CurrentGameMod       = GameMod.None;
    public static VisualMod  CurrentVisualMod     = VisualMod.All;
    public static GameRandom CurrentRandom        = GameRandom.None;
    public static GameFader  CurrentFader         = GameFader.None;
    public static Alignment  CurrentGearAlignment = Alignment.Center;

    private static int OriginScrollSpeed = 25;
    public static float ScrollSpeed
    {
        
        get { return OriginScrollSpeed * .0015f; }
        set
        {
            var speed = OriginScrollSpeed + Mathf.FloorToInt( value );
            if ( speed <= 1 )
            {
                Debug.Log( $"ScrollSpeed : {OriginScrollSpeed}" );
                return;
            }

            OriginScrollSpeed = speed;
            Debug.Log( $"ScrollSpeed : {OriginScrollSpeed}" );
        }
    }

    public static float PreLoadTime { get { return ( 1250f / Weight ); } }
    
    // 60bpm은 분당 1/4박자 60개, 스크롤 속도가 1일때 한박자(1/4) 시간은 1초
    public static float Weight { get { return ( 60f / NowPlaying.Inst.CurrentSong.medianBpm ) * ScrollSpeed; } }


    public static float SoundPitch = 1f;

    public static float JudgePos = -400f;

    public static float BGAOpacity = 0f;
    public static float PanelOpacity = 0f;
}
