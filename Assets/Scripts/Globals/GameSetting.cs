using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum BooleanOption { Off, On, Count }

public enum Alignment
{
    Left, Center, Right, Count,
}

public enum GameMod
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

public class GameSetting : MonoBehaviour
{
    public static GameMod   Mod           = GameMod.None;
    public static GameFader Fader         = GameFader.None;
    public static Alignment GearAlignment = Alignment.Center;

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

    public static float JudgePos = -540f;

    public static float BGAOpacity = 0f;
    public static float PanelOpacity = 0f;

    public static bool IsBGAPlay       = true;
    public static bool IsTouchEffect   = true;
    public static bool IsLineEffect    = true;
    public static bool IsCreateMeasure = true;
}
