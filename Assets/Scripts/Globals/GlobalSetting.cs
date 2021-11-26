using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSetting : MonoBehaviour
{
    public static float ScrollSpeed = 9f * 10f - 8f;
    public static float JudgeLine = -400f;

    public static bool IsFixedScroll = true;


    public static float MeasureHeight = 3f;

    // Gear
    public static float NoteWidth = 100f, NoteBlank = 2f;
    public static float NoteStartPos
    {
        get { return -( ( NoteWidth * 5f ) + ( NoteBlank * 7f ) ) / 2f; }
    }

    public static float GearStartPos
    {
        get { return -( ( NoteWidth * 6f ) + ( NoteBlank * 7f ) ) / 2f; }
    }

    public static float GearWidth
    {
        get { return ( NoteWidth * 6f ) + ( NoteBlank * 7f ); }
    }
}
