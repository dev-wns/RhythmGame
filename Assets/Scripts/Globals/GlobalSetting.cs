using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSetting : MonoBehaviour
{
    public static float ScrollSpeed = 9f * 10f - 8f;

    public static bool IsFixedScroll = true;

    public static float PPU = 100f; // pixel per unit

    // Measure
    public static float MeasureHeight = 3f;

    // Jugdement
    public static float JudgeLine = -400f;   // posY
    public static float JudgeHeight = 10f; // scaleY

    // note
    public static float NoteWidth = 100f, NoteHeight = 30f, NoteBlank = 2f;

    public static float NoteStartPos { get { return -( ( NoteWidth * 5f ) + ( NoteBlank * 7f ) ) * .5f; } }

    // Gear
    public static float GearStartPos { get { return ( -( ( NoteWidth * 6f ) + ( NoteBlank * 7f ) ) * .5f ); } }

    public static float GearWidth    { get { return ( ( NoteWidth * 6f ) + ( NoteBlank * 7f ) ); } }
}
