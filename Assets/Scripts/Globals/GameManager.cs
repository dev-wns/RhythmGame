using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonUnity<GameManager>
{
    public static List<Song> Songs = new List<Song>();
    public static Song CurrentSound { get; private set; }
    public static int CurrentSoundIndex { get; private set; }

    private void Awake()
    {
        DontDestroyOnLoad( this );

        QualitySettings.vSyncCount = 0;

        //SoundManager.Inst.Initialize();
        //GlobalKeySetting.Inst.Initialize();

        // Osu Parsing
        string[] osuFiles = GetFilesInSubDirectories( GlobalSetting.OsuDirectoryPath, "*.osu" );
        for ( int i = 0; i < osuFiles.Length; i++ )
        {
            using ( Parser parser = new OsuParser( osuFiles[i] ) )
            {
                var song = parser.PreRead();
                if ( parser.IsComplete ) Songs.Add( song );
            }
        }

        // BMS Parsing
        string[] bmsFiles = GetFilesInSubDirectories( GlobalSetting.BmsDirectoryPath, "*.bms" );
        for ( int i = 0; i < bmsFiles.Length; i++ )
        {
            using ( Parser parser = new BmsParser( bmsFiles[i] ) )
            {
                var song = parser.PreRead();
                if ( parser.IsComplete ) Songs.Add( song );
            }
        }

        if ( Songs.Count > 0 ) { CurrentSound = Songs[0]; }
        Debug.Log( "Parse Success " );
    }

    private void Update()
    {
        //SoundManager.Inst.Update();
    }

    private void OnApplicationQuit()
    {
        //SoundManager.Inst.Release();
    }

    public void SelectSong( int _index )
    {
        if ( _index < 0 || _index > Songs.Count - 1 )
        {
            Debug.Log( $"Sound Select Out Of Range. Index : {_index}" );
            return;
        }

        CurrentSound = Songs[_index];
        CurrentSoundIndex = _index;
    }
    private string[] GetFilesInSubDirectories( string _dirPath, string _extension )
    {
        List<string> path = new List<string>();

        string[] subDirectories;
        try { subDirectories = Directory.GetDirectories( _dirPath ); }
        catch ( System.Exception e )
        {
            // 대부분 폴더가 없는 경우.
            Debug.Log( e.ToString() );
            return path.ToArray();
        }

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
