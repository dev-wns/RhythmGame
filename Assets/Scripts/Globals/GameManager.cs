using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static Dictionary<string /* sound name */, SoundInfo> SoundInfomations = new Dictionary<string, SoundInfo>();
    private Parser parser = new Parser();

    private void Awake()
    {
        Stopwatch sw = new Stopwatch();
        // Osu Parsing
        sw.Start();
        DirectoryInfo info = new DirectoryInfo( Application.streamingAssetsPath + "/Songs" );
        foreach ( var dir in info.GetDirectories() )
        {
            foreach ( var file in dir.GetFiles( "*.osu" ) )
            {
                SoundInfo soundInfo = parser.Read( new Parser.FileInfo( Parser.Extension.Osu, file.FullName ) );
                if ( ReferenceEquals( null, soundInfo ) )
                {
                    UnityEngine.Debug.Log( "parsing failed. no data was created. #Path : " + file.FullName );
                }

                SoundInfomations.Add( soundInfo.preview.audio, soundInfo );
            }
        }
        sw.Stop();
        UnityEngine.Debug.Log( "time to read the *.osu file in the folder : " + sw.ElapsedMilliseconds / 1000f );
    }
}
