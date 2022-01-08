using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Globals
{
    public static Timer Timer = new Timer();
}

public class GlobalSetting : MonoBehaviour
{
    private static float OriginScrollSpeed = 28;
    public static float ScrollSpeed { get { return OriginScrollSpeed * .0015f; } }
    public static int SoundOffset = -100;

    public delegate void DelScrollChanged();
    public static event DelScrollChanged OnScrollChanged;

    public static float PPU { get; private set; } = 100f; // pixel per unit

    // IO
    public static string OsuDirectoryPath { get; private set; } = System.IO.Path.Combine( Application.streamingAssetsPath, "Osu" );
    public static string BmsDirectoryPath { get; private set; } = System.IO.Path.Combine( Application.streamingAssetsPath, "Bms" );

    public static string SoundDirectoryPath { get; private set; } = System.IO.Path.Combine( Application.streamingAssetsPath, "Songs" );
    public static string FailedPath { get; private set; } = System.IO.Path.Combine( Application.streamingAssetsPath, "Failed" );
    public static string DefaultImagePath { get; private set; } = System.IO.Path.Combine( "Assets", "Textures", "Default", "DefaultImage.jpg" );

    // Measure
    public static float MeasureHeight { get; private set; } = 3f;

    // Jugdement
    public static float JudgeLine = -500f; // posY
    public static float JudgeHeight { get; private set; } = 100f; // scaleY

    // note
    public static float NoteWidth  { get; private set; } = 80f;
    public static float NoteHeight { get; private set; } = 30f;
    public static float NoteBlank  { get; private set; } = 2f;
    public static float NoteStartPos { get { return -( ( NoteWidth * 5f ) + ( NoteBlank * 7f ) ) * .5f; } }

    // Gear
    public static float GearStartPos { get { return ( -( ( NoteWidth * 6f ) + ( NoteBlank * 7f ) ) * .5f ); } }
    public static float GearWidth    { get { return ( ( NoteWidth * 6f ) + ( NoteBlank * 7f ) ); } }

    private void Update()
    {
        if ( Input.GetKeyDown( KeyCode.Alpha1 ) )
        {
            OriginScrollSpeed -= 1;
            OnScrollChanged();
            Debug.Log( string.Format( "Current ScrollSpeed {0}", OriginScrollSpeed ) );
        }

        if ( Input.GetKeyDown( KeyCode.Alpha2 ) )
        {
            OriginScrollSpeed += 1;
            OnScrollChanged();
            Debug.Log( string.Format( "Current ScrollSpeed {0}", OriginScrollSpeed ) );
        }

        if ( OriginScrollSpeed < 1 ) OriginScrollSpeed = 1;
    }
}
