using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Globals
{
    public static Timer Timer = new Timer();
}

public class GlobalSetting : MonoBehaviour
{
    public static int PPU { get; private set; } = 100; // pixel per unit

    // IO
    public static string SoundDirectoryPath { get; private set; } = System.IO.Path.Combine( Application.streamingAssetsPath, "Songs" );
    public static string FailedPath { get; private set; } = System.IO.Path.Combine( Application.streamingAssetsPath, "Failed" );
    public static string DefaultImagePath { get; private set; } = System.IO.Path.Combine( "Assets", "Textures", "Default", "DefaultImage.jpg" );

    // Measure
    public static float MeasureHeight { get; private set; } = 3f;

    // Jugdement
    public static float JudgeLine = -520f; // posY
    public static float JudgeHeight { get; private set; } = 100f; // scaleY

    // note
    public static float NoteWidth  { get; private set; } = 80f;
    public static float NoteHeight { get; private set; } = 30f;
    public static float NoteBlank  { get; private set; } = 2f;
    public static float NoteStartPos { get { return -( ( NoteWidth * 5f ) + ( NoteBlank * 7f ) ) * .5f; } }

    // Gear
    public static float GearStartPos { get { return ( -( ( NoteWidth * 6f ) + ( NoteBlank * 7f ) ) * .5f ); } }
    public static float GearWidth    { get { return ( ( NoteWidth * 6f ) + ( NoteBlank * 7f ) ); } }
}
