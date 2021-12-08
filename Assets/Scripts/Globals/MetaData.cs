using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public struct Timings
{
    public float changeTime;
    public float bpm;

    public Timings( float _changeTime, float _bpm )
    {
        changeTime = _changeTime;
        bpm = _bpm;
    }
}

public struct Notes
{
    public int line;
    public float hitTiming;
    public int type;
    public int lengthLN;
    public Notes( int _x, float _hitTiming, int _type, int _lengthLN )
    {
        line = Mathf.FloorToInt( _x * 6f / 512f );
        hitTiming = _hitTiming;
        type = _type;
        lengthLN = _lengthLN;
    }
}

//public class MetaData
//{
//    public string title, artist, creator, version;
//    public string audioName, imgName;
//    public string audioPath, imgPath;
//    public int previewTime;

//    public List<Notes> notes = new List<Notes>();
//    public List<Timings> timings = new List<Timings>();

//    public Sprite background;
//    //public AudioClip clip;
//    //public FMOD.Sound sound;
//}

public class MetaData : MonoBehaviour
{
    public static List<Song> Songs { get; private set; } = new List<Song>();

    private void Awake()
    {
        // Osu Parsing
        string[] osuFiles = GetFilesInSubDirectories( GlobalSetting.OsuDirectoryPath, "*.osu" );
        for ( int i = 0; i < osuFiles.Length; i++ )
        {
            using ( Parser parser = new OsuParser( osuFiles[i] ) )
            {
                Songs.Add( parser.PreRead() );
            }
        }

        // BMS Parsing
        string[] bmsFiles = GetFilesInSubDirectories( GlobalSetting.BmsDirectoryPath, "*.bms" );
        for ( int i = 0; i < bmsFiles.Length; i++ )
        {
            using ( Parser parser = new BmsParser( bmsFiles[i] ) )
            {
                Songs.Add( parser.PreRead() );
            }
        }
    }

    private string[] GetFilesInSubDirectories( string _dirPath, string _extension )
    {
        List<string> path = new List<string>();

        string[] subDirectories = Directory.GetDirectories( _dirPath );
        foreach ( string subDir in subDirectories )
        {
            DirectoryInfo dirInfo = new DirectoryInfo( subDir );
            FileInfo[] files = dirInfo.GetFiles( _extension );
            for ( int i = 0; i < files.Length; i++ )
                path.Add( files[i].FullName );
        }

        return path.ToArray();
    }
}
