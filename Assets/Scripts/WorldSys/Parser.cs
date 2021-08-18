using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Parser : MonoBehaviour
{

    // preview, timing data parsing
    private void PreRead( string _path )
    {
        string line;
        StreamReader reader = new StreamReader( _path );
        Sound sound = new Sound();
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

                sound.preview.audio = arr[0].Substring( 14 ).Trim();
                sound.preview.time = int.Parse( arr[2].Substring( 12 ).Trim() );
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
                sound.preview.img = img[ 2 ].Trim();
            }

            if ( line.Contains( "[TimingPoints]" ) )
            {
                while ( true )
                {
                    if ( string.IsNullOrEmpty( line = reader.ReadLine() ) )
                    {
                        continue;
                    }

                    if ( line.Contains( "[HitObjects]" ) )
                    {
                        break;
                    }

                    string[] arr = line.Split( ',' );
                    sound.timings.Add( new Sound.Timing( int.Parse( arr[ 0 ] ), float.Parse( arr[ 1 ] ) ) );
                }
            }

            if ( line.Contains( "[HitObjects]" ) )
            {
                break;
            }
        }
    }
}
