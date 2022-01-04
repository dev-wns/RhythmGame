using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum OPTION_BOOL { Off, On, Count }

public enum ALIGNMENT
{
    Left, Center, Right, Count,
}

public enum MOD
{
    None,
    Mirror,
    Random,
    Half_Random,
    Max_Random,
    Count,
}
public enum FADER
{
    None,
    Fade_In,
    Fade_Out,
    Count,
}

public class GameSetting : MonoBehaviour
{
    public static MOD GameMod = MOD.None;
    public static FADER GameFader = FADER.None;
    public static ALIGNMENT GearAlignment = ALIGNMENT.Center;

    public static float ScrollSpeed = 30f;
    public static float SoundPitch = 1f;

    public static float JudgePos = -400f;

    public static float BGAOpacity = 0f;
    public static float PanelOpacity = 0f;

    public static bool IsBGAPlay = true;
    public static bool IsTouchEffect = true;
    public static bool IsLineEffect = true;
    public static bool IsCreateMeasure = true;
}
