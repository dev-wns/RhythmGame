using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NowPlaying : MonoBehaviour
{
    public static string MusicFile, File, BGFile, Title, Artist, Hash, Folder;
    public static string Level, Length;
    public static float Offset;
    public static double Median;
    public static int TimingCounts, NoteCounts, LongNoteCounts;
    public static AudioClip Clip;
    public static bool IsBGA, IsVirtual;
    public static int LengthMS;
    public int note, longNote;

    private MusicHandler player;

    private void Start ()
    {
        player = GameObject.FindWithTag( "World" ).GetComponent<MusicHandler>();
    }

    private void Update ()
    {
        note = NoteCounts;
        longNote = LongNoteCounts;

        float f = LengthMS / 1000.0f;
        string s = $" {Mathf.FloorToInt( f / 60.0f )} : ";
        s += ( f % 60.0f ).ToString( "00" );
        Length = s;
    }
}
