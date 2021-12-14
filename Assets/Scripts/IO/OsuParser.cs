using System;
using System.IO;
using System.Collections.Generic;

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
                song.timingCount++;
            }

            while( ReadLineEndOfStream() )
            {
                string[] splitDatas = line.Split( ',' );
                if ( splitDatas.Length != 6 ) continue;

                song.totalTime = int.Parse( splitDatas[2] );
                int note       = int.Parse( splitDatas[3] );

                if ( note == 128 ) song.sliderCount++;
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

    public override Chart PostRead( Song _song )
    {
        chart.timings = new List<Timing>();
        chart.timings.Capacity = _song.noteCount +_song.sliderCount;

        chart.notes = new List<Note>();
        chart.notes.Capacity = _song.timingCount;

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

            double beatLength = Math.Abs( float.Parse( splitDatas[1] ) );
            double BPM = 1d / beatLength * 60000d;

            if ( isUninherited ) prevBPM = BPM;
            else BPM = ( prevBPM * 100d ) / beatLength; // 상속된 bpm은 부모 bpm의 백분율 값을 가진다.

            chart.timings.Add( new Timing( float.Parse( splitDatas[0] ), ( float )BPM ) );
        }
        chart.medianBpm = GetMedianBpm( chart.timings );

        while ( ReadLineEndOfStream() )
        {
            string[] splitDatas = line.Split( ',' );
            if ( splitDatas.Length != 6 ) continue;

            bool isSlider;
            float sliderTime = 0;
            int type = int.Parse( splitDatas[3] );
            if ( type == 128 )
            {
                isSlider = true;
                string[] splitSliderData = splitDatas[5].Split( ':' );
                sliderTime = int.Parse( splitSliderData[0] );
            }
            else isSlider = false;

            float time = int.Parse( splitDatas[2] );
            chart.notes.Add( new Note( int.Parse( splitDatas[0] ), time, InGame.GetChangedTime( time, chart ), 
                                                  sliderTime, InGame.GetChangedTime( sliderTime, chart ), isSlider ) );
        }
        return chart;
    }
}
