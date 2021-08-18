using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileReader
{

    private void PreRead( string _path )
    {
        string line;
        StreamReader reader = new StreamReader( _path );
        Sound sound = new Sound();
        while ( ( line = reader.ReadLine() ) is not null )
        {
            if ( line.Contains( "[General]" ) )
            {
                List<string> arr = new List<string>();
                for ( int index = 0; index < 3; ++index )
                {
                    if ( line.Contains( "[Metadata]" ) )
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
                    if ( line.Contains( "[Events]" ) )
                    {
                        break;
                    }
                    arr.Add( line = reader.ReadLine() );
                }

                sound.preview.title = arr[ 0 ].Substring( 6 ).Trim();
                sound.preview.artist = arr[ 2 ].Substring( 7 ).Trim();
            }

            if ( line.Contains( "[Event]" ) )
            {
                List<string> arr = new List<string>();
                for ( int index = 0; index < 4; ++index )
                {
                    if ( line.Contains( "[TimingPoints]" ) )
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
                while ( !line.Contains( "[HitObjects]" ) )
                {
                    line = reader.ReadLine();
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
