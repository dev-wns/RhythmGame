using System.Collections.Generic;
using System.IO;

public class Parser
{
    public enum Extension { Osu, Bms, Custom }

    public struct FileInfo
    {
        public Extension type;
        public string path;
        public FileInfo( Extension _type, string _path )
        {
            type = _type;
            path = _path;
        }
    }

    //private void Start()
    //{
    //    //info = new DirectoryInfo( Application.streamingAssetsPath + "/Sounds/Lobby/Background" );
    //    //foreach ( var dir in info.GetDirectories() )
    //    //{
    //    //    foreach ( var file in dir.GetFiles( "*.mp3" ) )
    //    //    {
    //    //        Load( file.FullName );
    //    //        Play( Path.GetFileNameWithoutExtension( file.FullName ) );
    //    //    }
    //    //}
    //}

    // preview, timing data parsing
    public SoundInfo Read( FileInfo _info )
    {
        SoundInfo sound = null;

        switch ( _info.type )
        {
            case Extension.Osu:
            {
                sound = OsuExtension( _info.path );
                break;
            }
            case Extension.Bms:
            {
                break;
            }
            case Extension.Custom:
            {
                break;
            }
            default: break;
        }

        return sound;
    }

    private SoundInfo OsuExtension( string _path )
    {
        string line;
        StreamReader reader = new StreamReader( _path );
        SoundInfo sound = new SoundInfo();

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

                sound.preview.audio = Path.GetFileNameWithoutExtension( arr[ 0 ].Substring( 14 ).Trim() );
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
                sound.preview.img = img[ 2 ].Trim().Replace( "\"", string.Empty );
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
                    sound.timings.Add( new SoundInfo.TimingInfo( float.Parse( arr[ 0 ] ), float.Parse( arr[ 1 ] ) ) );
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

    private void BmsExtension( string _path )
    {

    }

    private void CustomExtention( string _path )
    {

    }
}
