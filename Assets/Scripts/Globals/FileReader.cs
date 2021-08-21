using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Timings = SoundData.Timings;
using Notes = SoundData.Notes;

public class FileReader
{
    // preview, timing data parsing
    public SoundData Read( string _path )
    {
        string line;
        StreamReader reader = new StreamReader( _path );
        SoundData sound = new SoundData();

        while ( ( line = reader.ReadLine() ) != null )
        {
            if ( line.Contains( "[General]" ) )
            {
                List<string> arr = new List<string>();
                for ( int index = 0; index < 3; ++index )
                {
                    if ( string.IsNullOrEmpty( line ) || line.Contains( "[Metadata]" ) )
                    {
                        break;
                    }
                    arr.Add( line = reader.ReadLine() );
                }

                sound.preview.name = Path.GetFileNameWithoutExtension( arr[ 0 ].Substring( 14 ).Trim() );
                sound.preview.time = int.Parse( arr[ 2 ].Substring( 12 ).Trim() );
            }

            if ( line.Contains( "[Metadata]" ) )
            {
                List<string> arr = new List<string>();
                for ( int index = 0; index < 6; ++index )
                {
                    if ( string.IsNullOrEmpty( line ) || line.Contains( "[Events]" ) )
                    {
                        break;
                    }
                    arr.Add( line = reader.ReadLine() );
                }

                sound.preview.title = arr[ 0 ].Substring( 6 ).Trim();
                sound.preview.artist = arr[ 2 ].Substring( 7 ).Trim();
            }

            if ( line.Contains( "[Events]" ) )
            {
                List<string> arr = new List<string>();
                for ( int index = 0; index < 4; ++index )
                {
                    if ( string.IsNullOrEmpty( line ) || line.Contains( "[TimingPoints]" ) )
                    {
                        break;
                    }
                    arr.Add( line = reader.ReadLine() );
                }

                string[] img = arr[ 1 ].Split( ',' );
                sound.preview.img = Path.GetDirectoryName( _path ) + "\\" + img[ 2 ].Trim().Replace( "\"", string.Empty );
            }

            if ( line.Contains( "[TimingPoints]" ) )
            {
                while ( true )
                {
                    if ( string.IsNullOrEmpty( line = reader.ReadLine() ) )
                    {
                        continue;
                    }

                    if ( line.Contains( "[Colours]" ) || line.Contains( "[HitObjects]" ) )
                    {
                        break;
                    }

                    string[] arr = line.Split( ',' );
                    sound.timings.Add( new Timings( float.Parse( arr[ 0 ] ), float.Parse( arr[ 1 ] ) ) );
                }
            }

            if ( line.Contains( "[Colours]" ) || line.Contains( "[HitObjects]" ) )
            {
                break;
            }
        }
        reader.Close();

        return sound;
    }

    // directories since streaming asset path
    public static string[] GetFiles( string _path, string _extension = "*.osu" )
    {
        List<string> directories = new List<string>();
        DirectoryInfo info = new DirectoryInfo( Application.streamingAssetsPath + _path );
        foreach ( var dir in info.GetDirectories() )
        {
            foreach ( var file in dir.GetFiles( _extension ) )
            {
                directories.Add( file.FullName );
            }
        }

        return directories.ToArray();
    }
}
