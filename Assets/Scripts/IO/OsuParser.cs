using System;
using System.IO;

public class OsuParser : Parser
{
    public OsuParser( string _path ) : base( _path ) { }

    public override Song PreRead()
    {
        try
        {
            // [General] ~ [Editor]
            while ( ReadLine() != "[Metadata]" )
            {
                if ( Contains( "AudioFilename" ) ) song.AudioPath = Path.Combine( directory, SplitAndTrim( ':' ) );
                if ( Contains( "PreviewTime" ) ) song.PreviewTime = int.Parse( SplitAndTrim( ':' ) );
            }
            
            // [Metadata] ~ [Difficulty]
            while ( ReadLine() != "[Events]" )
            {
                if ( Contains( "Title" ) )   song.Title   = SplitAndTrim( ':' );
                if ( Contains( "Artist" ) )  song.Artist  = SplitAndTrim( ':' );
                if ( Contains( "Creator" ) ) song.Creator = SplitAndTrim( ':' );
                if ( Contains( "Version" ) ) song.Version = SplitAndTrim( ':' );
            }
            
            // [Events]
            while ( ReadLine() != "[TimingPoints]" )
            {
                if ( Contains( ".avi" ) || Contains( ".mp4" ) || Contains( ".mpg" ) )
                {
                    song.VideoPath = Path.Combine( directory, SplitAndTrim( '"' ) );
                    song.HasVideo  = true;
            
                    FileInfo videoInfo = new FileInfo( song.VideoPath );
                    if ( !videoInfo.Exists ) song.HasVideo = false;
                }
            
                if ( Contains( ".jpg" ) || Contains( ".png" ) )
                {
                    song.ImagePath = Path.Combine( directory, SplitAndTrim( '"' ) );
            
                    FileInfo imageInfo = new FileInfo( song.ImagePath );
                    if ( !imageInfo.Exists ) song.ImagePath = GlobalSetting.DefaultImagePath;
                }
            }
        }
        catch ( Exception _error )
        {
            UnityEngine.Debug.Log( _error.Message );
            Dispose();
        }

        return song;
    }

    public override Chart PostRead()
    {
        //string line;
        //StreamReader reader = new StreamReader( path );
        //MetaData data = new MetaData();

        //while ( ( line = reader.ReadLine() ) != null )
        //{
        //    if ( line.Contains( "[General]" ) )
        //    {
        //        List<string> arr = new List<string>();
        //        for ( int index = 0; index < 3; ++index )
        //        {
        //            if ( string.IsNullOrEmpty( line ) || line.Contains( "[Metadata]" ) )
        //            {
        //                break;
        //            }
        //            arr.Add( line = reader.ReadLine() );
        //        }

        //        data.audioName = Path.GetFileName( arr[0].Substring( 14 ).Trim() );
        //        data.audioPath = Path.GetDirectoryName( _path ) + "\\" + data.audioName;
        //        data.previewTime = int.Parse( arr[2].Substring( 12 ).Trim() );
        //    }

        //    if ( line.Contains( "[Metadata]" ) )
        //    {
        //        List<string> arr = new List<string>();
        //        for ( int index = 0; index < 6; ++index )
        //        {
        //            if ( string.IsNullOrEmpty( line ) || line.Contains( "[Events]" ) )
        //            {
        //                break;
        //            }
        //            arr.Add( line = reader.ReadLine() );
        //        }

        //        data.title = arr[0].Substring( 6 ).Trim();
        //        data.artist = arr[2].Substring( 7 ).Trim();
        //        data.creator = arr[4].Substring( 8 ).Trim();
        //        data.version = arr[5].Substring( 8 ).Trim();
        //    }

        //    if ( line.Contains( "[Events]" ) )
        //    {
        //        List<string> arr = new List<string>();
        //        for ( int index = 0; index < 4; ++index )
        //        {
        //            if ( string.IsNullOrEmpty( line ) || line.Contains( "[TimingPoints]" ) )
        //            {
        //                break;
        //            }
        //            arr.Add( line = reader.ReadLine() );
        //        }

        //        string[] img = arr[1].Split( ',' );
        //        data.imgName = img[2].Trim().Replace( "\"", string.Empty );
        //        data.imgPath = Path.GetDirectoryName( _path ) + "\\" + data.imgName;
        //    }

        //    if ( line.Contains( "[TimingPoints]" ) )
        //    {
        //        double prevBPM = 0d;
        //        bool isFirst = true;
        //        while ( !( string.IsNullOrEmpty( line = reader.ReadLine() ) || line.Contains( "[Colours]" ) || line.Contains( "[HitObjects]" ) ) )
        //        {
        //            string[] arr = line.Split( ',' );

        //            bool isUninherited = StringToBoolean( arr[6] );
        //            float changeTime = float.Parse( arr[0] );
        //            double beatLength = Mathf.Abs( float.Parse( arr[1] ) );
        //            double BPM = 1d / beatLength * 60000d;

        //            if ( isUninherited ) prevBPM = BPM;
        //            else BPM = ( prevBPM * 100d ) / beatLength;

        //            if ( isFirst )
        //            {
        //                data.timings.Add( new Timings( -10000, ( float )BPM ) );
        //                isFirst = false;
        //            }
        //            data.timings.Add( new Timings( changeTime, ( float )BPM ) );
        //        }
        //    }

        //    if ( line.Contains( "[HitObjects]" ) )
        //    {
        //        while ( !string.IsNullOrEmpty( line = reader.ReadLine() ) )
        //        {
        //            string[] arr = line.Split( ',' );
        //            string[] LNTiming = arr[5].Split( ':' );
        //            data.notes.Add( new Notes( int.Parse( arr[0] ), float.Parse( arr[2] ), int.Parse( arr[3] ), int.Parse( LNTiming[0] ) ) );
        //        }
        //    }
        //}
        //reader.Close();

        //int idx = data.audioName.IndexOf( "-" );
        //if ( idx >= 0 )
        //{
        //    string src = data.audioName;
        //    data.audioName = data.audioName.Replace( "-", "" );
        //    File.Move( Path.GetDirectoryName( _path ) + "\\" + src, Path.GetDirectoryName( _path ) + "\\" + data.audioName );

        //    string[] lines = File.ReadAllLines( _path );
        //    var pos = Array.FindIndex( lines, row => row.Contains( "AudioFilename:" ) );
        //    if ( pos > 0 )
        //    {
        //        lines[pos] = string.Format( "AudioFilename:{0}", data.audioName );
        //        File.WriteAllLines( _path, lines );
        //    }
        //}
        return chart;
    }
}
