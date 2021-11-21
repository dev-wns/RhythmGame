using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
//using Timings = Song.Timings;
//using Notes = Song.Notes;

public class FileReader
{
    // preview, timing data parsing

    bool StringToBoolean( string _value )
    {
        int value = int.Parse( _value );
        if ( value != 0 ) return true;
        else return false;
    }

    public MetaData Read( string _path )
    {
        string line;
        StreamReader reader = new StreamReader( _path );
        MetaData data = new MetaData();

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

                data.audioName = Path.GetFileName( arr[ 0 ].Substring( 14 ).Trim() );
                data.audioPath = Path.GetDirectoryName( _path ) + "\\" + data.audioName;
                data.previewTime = int.Parse( arr[ 2 ].Substring( 12 ).Trim() );
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

                data.title = arr[0].Substring( 6 ).Trim();
                data.artist = arr[2].Substring( 7 ).Trim();
                data.creator = arr[4].Substring( 8 ).Trim();
                data.version = arr[5].Substring( 8 ).Trim();
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
                data.imgName = img[ 2 ].Trim().Replace( "\"", string.Empty );
                data.imgPath = Path.GetDirectoryName( _path ) + "\\" + data.imgName;
            }

            if ( line.Contains( "[TimingPoints]" ) )
            {
                while ( !( string.IsNullOrEmpty( line = reader.ReadLine() ) || line.Contains( "[Colours]" ) || line.Contains( "[HitObjects]" ) ) )
                {
                    string[] arr = line.Split( ',' );
                    data.timings.Add( new MetaData.Timings( float.Parse( arr[0] ), float.Parse( arr[1] ), StringToBoolean( arr[6] ) ) );
                }
            }

            if ( line.Contains( "[HitObjects]" ) )
            {
                while ( !string.IsNullOrEmpty( line = reader.ReadLine() ) )
                {
                    string[] arr = line.Split( ',' );
                    string[] LNTiming = arr[5].Split( ':' );
                    data.notes.Add( new MetaData.Notes( int.Parse( arr[0] ), int.Parse( arr[1] ), float.Parse( arr[2] ), int.Parse( arr[3] ), int.Parse( LNTiming[0] ) ) );
                }
            }
        }
        reader.Close();

        int idx = data.audioName.IndexOf( "-" );
        if ( idx >= 0 )
        {
            string src = data.audioName;
            data.audioName = data.audioName.Replace( "-", "" );
            File.Move( Path.GetDirectoryName( _path ) + "\\" + src, Path.GetDirectoryName( _path ) + "\\" + data.audioName );

            string[] lines = File.ReadAllLines( _path );
            var pos = Array.FindIndex( lines, row => row.Contains( "AudioFilename:" ) );
            if ( pos > 0 )
            {
                lines[pos] = "AudioFilename:" + data.audioName;
                File.WriteAllLines( _path, lines );
            }
        }

        return data;
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
