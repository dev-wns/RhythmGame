using System;
using System.IO;

public class OsuParser : Parser
{
    public OsuParser( string _path ) : base( _path ) { song.type = ParseType.Osu; }

    public override Song PreRead()
    {
        try
        {
            // [General] ~ [Editor]
            while ( ReadLine() != "[Metadata]" )
            {
                if ( Contains( "AudioFilename" ) ) song.audioPath   = Path.Combine( directory, SplitAndTrim( ':' ) );
                if ( Contains( "PreviewTime" ) )   song.previewTime = int.Parse( SplitAndTrim( ':' ) );
            }
            
            // [Metadata] ~ [Difficulty]
            while ( ReadLine() != "[Events]" )
            {
                if ( Contains( "Title" ) )   song.title   = SplitAndTrim( ':' );
                if ( Contains( "Artist" ) )  song.artist  = SplitAndTrim( ':' );
                if ( Contains( "Creator" ) ) song.creator = SplitAndTrim( ':' );
                if ( Contains( "Version" ) ) song.version = SplitAndTrim( ':' );
            }
            
            // [Events]
            while ( ReadLine() != "[TimingPoints]" )
            {
                if ( Contains( ".avi" ) || Contains( ".mp4" ) || Contains( ".mpg" ) )
                {
                    song.videoPath = Path.Combine( directory, SplitAndTrim( '"' ) );
                    song.hasVideo  = true;
            
                    FileInfo videoInfo = new FileInfo( song.videoPath );
                    if ( !videoInfo.Exists ) song.hasVideo = false;
                }
            
                if ( Contains( ".jpg" ) || Contains( ".png" ) )
                {
                    song.imagePath = Path.Combine( directory, SplitAndTrim( '"' ) );
            
                    FileInfo imageInfo = new FileInfo( song.imagePath );
                    if ( !imageInfo.Exists ) song.imagePath = GlobalSetting.DefaultImagePath;
                }
            }

            // [TimingPoints]
            double prevBPM = 0d;
            while ( ReadLine() != "[HitObjects]" )
            { 
                string[] splitDatas = line.Split( ',' );
                if ( splitDatas.Length != 8 ) continue;

                bool isUninherited;
                int uninherited = int.Parse( splitDatas[6] );
                if ( uninherited == 0 ) isUninherited = false;
                else                    isUninherited = true;

                double beatLength    = Math.Abs( float.Parse( splitDatas[1] ) );
                double BPM           = 1d / beatLength * 60000d;

                if ( isUninherited ) prevBPM = BPM;
                else                 BPM = ( prevBPM * 100d ) / beatLength; // 상속된 bpm은 부모 bpm의 백분율 값을 가진다.

                if ( song.minBpm > BPM || song.minBpm == 0 ) song.minBpm = ( int )BPM;
                if ( song.maxBpm < BPM )                     song.maxBpm = ( int )BPM;
            }

            while( ReadLineEndOfStream() )
            {
                string[] splitDatas = line.Split( ',' );
                if ( splitDatas.Length != 6 ) continue;

                song.totalTime = int.Parse( splitDatas[2] );
                int note       = int.Parse( splitDatas[3] );

                if ( note == 128 ) song.longNoteCount++;
                else               song.noteCount++;

            }
        }
        catch ( Exception _error )
        {
            UnityEngine.Debug.Log( _error.Message );
            IsComplete = false;
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
