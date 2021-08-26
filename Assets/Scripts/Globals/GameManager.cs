using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public static List<Song> songs = new List<Song>();
    private void Awake()
    {
        // Osu Parsing
        Debug.Log( "Osu FileParsing Start" );
        FileReader parser = new FileReader();
        System.IO.DirectoryInfo info = new System.IO.DirectoryInfo( Application.streamingAssetsPath + "/Songs" );
        foreach ( var dir in info.GetDirectories() )
        {
            foreach ( var file in dir.GetFiles( "*.osu" ) )
            {
                Song song = parser.Read( file.FullName );
                if ( ReferenceEquals( null, song ) )
                {
                    Debug.Log( "parsing failed. no data was created. #Path : " + file.FullName );
                }

                song.preview.path = file.DirectoryName + "\\" + song.preview.name + ".mp3";
                songs.Add( song );
            }
        }
        Debug.Log( "Osu FileParsing End" );

        Debug.Log( "GamaManager Initizlize Successful." );
    }

    private void Start()
    {
        SceneChanger.Inst.Change( "Lobby" );
    }
}
