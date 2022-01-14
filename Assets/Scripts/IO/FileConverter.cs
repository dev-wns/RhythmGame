using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;

public struct Song
{
    public string filePath;
    public string audioPath;
    public string imagePath;
    public string videoPath;
    public bool   hasVideo;

    public string title;
    public string artist;
    public string creator;
    public string version;

    public int previewTime;
    public int totalTime;

    public int noteCount;
    public int sliderCount;
    public int timingCount;
    public int minBpm;
    public int maxBpm;
    public int medianBpm;
}

public class Timing
{
    public float time;
    public float bpm;

    public Timing() { }
    public Timing( float _time, float _bpm )
    {
        time = _time;
        bpm = _bpm;
    }
}

public struct Note
{
    public int line;
    public float time;
    public float sliderTime;
    public bool isSlider;
    public float calcTime;
    public float calcSliderTime;

    public Note( int _line, float _time, float _calcTime, float _sliderTime, float _calcSliderTime, bool _isSlider )
    {
        line = _line;
        time = _time;
        calcTime = _calcTime;
        sliderTime = _sliderTime;
        calcSliderTime = _calcSliderTime;
        isSlider = _isSlider;
    }
}

public struct Chart
{
    public List<Timing> timings;
    public List<Note> notes;
}

public class FileConverter : FileReader
{
    public void ReLoad()
    {
        string[] osuFiles = GetFilesInSubDirectories( GameSetting.SoundDirectoryPath, "*.osu" );
        for ( int i = 0; i < osuFiles.Length; i++ )
        {
            Convert( osuFiles[i] );
        }
    }

    private void Convert( string _path )
    {
        Song song   = new Song();
        Chart chart = new Chart();

        try { OpenFile( _path ); }
        catch ( Exception _error )
        {
            Debug.LogError( _error.Message );
            Dispose();
            return;
        }

        try
        {
            // [General] ~ [Editor]
            while ( ReadLine() != "[Metadata]" )
            {
                if ( Contains( "AudioFilename" ) ) song.audioPath   = SplitAndTrim( ':' );
                if ( Contains( "PreviewTime" ) )   song.previewTime = int.Parse( SplitAndTrim( ':' ) );
            }

            // [Metadata] ~ [Difficulty]
            while ( ReadLine() != "[Events]" )
            {
                if ( Contains( "Title" ) && !Contains( "TitleUnicode:" ) )
                     song.title   = SplitAndTrim( ':' );

                if ( Contains( "Artist" ) && !Contains( "ArtistUnicode" ) )  
                     song.artist  = SplitAndTrim( ':' );

                if ( Contains( "Creator" ) ) song.creator = SplitAndTrim( ':' );
                if ( Contains( "Version" ) ) song.version = SplitAndTrim( ':' );
            }

            // [Events]

            var directory = Path.GetDirectoryName( _path );

            while ( ReadLine() != "[TimingPoints]" )
            {
                if ( Contains( ".avi" ) || Contains( ".mp4" ) || Contains( ".mpg" ) )
                {
                    song.videoPath = SplitAndTrim( '"' );
                    song.hasVideo  = true;

                    if ( File.Exists( Path.Combine( directory, song.videoPath ) ) ) song.hasVideo = true;
                    else                                                            song.hasVideo = false;
                }

                if ( Contains( ".jpg" ) || Contains( ".png" ) )
                {
                    song.imagePath = SplitAndTrim( '"' );

                    if ( !File.Exists( Path.Combine( directory, song.imagePath ) ) ) 
                         song.imagePath = GameSetting.DefaultImagePath;
                }
            }

            chart.timings?.Clear();
            chart.timings ??= new List<Timing>();
            chart.timings.Capacity = song.noteCount + song.sliderCount;

            chart.notes?.Clear();
            chart.notes ??= new List<Note>();
            chart.notes.Capacity = song.timingCount;

            // [TimingPoints]
            double prevBPM = 0d;
            song.timingCount = 0;
            while ( ReadLine() != "[HitObjects]" )
            {
                string[] splitDatas = line.Split( ',' );
                if ( splitDatas.Length != 8 ) continue;

                bool isUninherited;
                
                int uninherited = int.Parse( splitDatas[6] );
                if ( uninherited == 0 ) isUninherited = false;
                else isUninherited = true;

                double beatLength = Math.Abs( float.Parse( splitDatas[1] ) );
                double BPM = 1d / beatLength * 60000d;

                if ( isUninherited ) prevBPM = BPM;
                else BPM = ( prevBPM * 100d ) / beatLength; // 상속된 bpm은 부모 bpm의 백분율 값을 가진다.

                if ( song.minBpm > BPM || song.minBpm == 0 ) song.minBpm = ( int )BPM;
                if ( song.maxBpm < BPM ) song.maxBpm = ( int )BPM;

                float time = float.Parse( splitDatas[0] );
                if ( song.timingCount == 0 ) time = -5000f;

                song.timingCount++;

                chart.timings.Add( new Timing( time, ( float )BPM ) );
            }
            song.medianBpm = ( int )GetMedianBpm( chart.timings );

            // [HitObjects]
            song.noteCount = 0;
            song.sliderCount = 0;
            while ( ReadLineEndOfStream() )
            {
                string[] splitDatas = line.Split( ',' );
                if ( splitDatas.Length != 6 ) continue;

                song.totalTime = int.Parse( splitDatas[2] );
                
                int note       = int.Parse( splitDatas[3] );
                if ( note == 128 ) song.sliderCount++;
                else song.noteCount++;

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
                int lane = Mathf.FloorToInt( int.Parse( splitDatas[0] ) * 6f / 512f );
                chart.notes.Add( new Note( lane, time, GetChangedTime( time, chart ),
                                                 sliderTime, GetChangedTime( sliderTime, chart ), isSlider ) );
            }

            Write( song, chart );
        }
        catch ( Exception _error )
        {
            if ( !Directory.Exists( GameSetting.FailedPath ) )
                  Directory.CreateDirectory( GameSetting.FailedPath );

            if ( File.Exists( path ) )
            {
                File.Move( path, GameSetting.FailedPath );
                Debug.LogWarning( $"File Move Failed Directory : {path}" );
            }

            Dispose();
            Debug.LogError( $"{_error}, {path}" );
        }
    }

    private void Write( Song _song, Chart _chart )
    {
        try
        {
            string fileName = Path.GetFileNameWithoutExtension( path ) + ".wns";
            string filePath = Path.Combine( Path.GetDirectoryName( path ), fileName );

            using ( var stream = new FileStream( filePath, FileMode.Create ) )
            {
                using ( var writer = new StreamWriter( stream ) )
                {
                    writer.WriteLine( "[General]" );
                    writer.WriteLine( "AudioPath: " + _song.audioPath );
                    writer.WriteLine( "ImagePath: " + _song.imagePath );
                    writer.WriteLine( "VideoPath: " + _song.videoPath );

                    writer.WriteLine( "Title: "   + _song.title );
                    writer.WriteLine( "Artist: "  + _song.artist );
                    writer.WriteLine( "Creator: " + _song.creator );
                    writer.WriteLine( "Version: " + _song.version );

                    writer.WriteLine( "PreviewTime: " + _song.previewTime );
                    writer.WriteLine( "TotalTime: "   + _song.totalTime );

                    writer.WriteLine( "NumNote: "   + _song.noteCount );
                    writer.WriteLine( "NumSlider: " + _song.sliderCount );
                    writer.WriteLine( "NumTiming: " + _song.timingCount );

                    writer.WriteLine( "MinBPM: "    + _song.minBpm );
                    writer.WriteLine( "MaxBPM: "    + _song.maxBpm );
                    writer.WriteLine( "MedianBPM: " + _song.medianBpm );

                    StringBuilder text = new StringBuilder();
                    writer.WriteLine( "[Timings]" );
                    for ( int i = 0; i < _chart.timings.Count; i++ )
                    {
                        text.Clear();
                        text.Append( _chart.timings[i].time ).Append( "," );
                        text.Append( _chart.timings[i].bpm );

                        writer.WriteLine( text );
                    }

                    writer.WriteLine( "[Notes]" );
                    for ( int i = 0; i < _chart.notes.Count; i++ )
                    {
                        text.Clear();
                        text.Append( _chart.notes[i].line ).Append( "," );
                        text.Append( _chart.notes[i].time ).Append( "," );
                        text.Append( _chart.notes[i].sliderTime ).Append( "," );
                        text.Append( _chart.notes[i].isSlider ).Append( "," );
                        text.Append( _chart.notes[i].calcTime ).Append( "," );
                        text.Append( _chart.notes[i].calcSliderTime );

                        writer.WriteLine( text );
                    }
                }
            }
        }
        catch ( Exception _error )
        {
            Dispose();
            Debug.LogError( _error.Message );
        }

        // 원본 파일 삭제
        if ( File.Exists( path ) )
        {
            //Debug.Log( $"File Delete : {path}" );
            //File.Delete( path );
        }
    }

    private float GetMedianBpm( List<Timing> timings )
    {
        List<Timing> medianCalc = new List<Timing>();
        medianCalc.Add( new Timing( 0f, timings[0].bpm ) );
        for ( int i = 1; i < timings.Count; i++ )
        {
            float prevTime = timings[i - 1].time;
            float prevBpm = timings[i - 1].bpm;

            bool isFind = false;
            for ( int j = 0; j < medianCalc.Count; j++ )
            {
                if ( Mathf.Abs( medianCalc[j].bpm - prevBpm ) < .1f )
                {
                    isFind = true;
                    medianCalc[j].time += timings[i].time - prevTime;
                }
            }

            if ( !isFind ) medianCalc.Add( new Timing( timings[i].time - prevTime, prevBpm ) );
        }

        medianCalc.Sort( delegate ( Timing A, Timing B )
        {
            if ( A.time < B.time ) return 1;
            else if ( A.time > B.time ) return -1;
            else return 0;
        } );

        //return 1f / medianCalc[0].bpm * 60000f;
        return medianCalc[0].bpm;
    }

    private float GetChangedTime( float _time, Chart chart ) // BPM 변화에 따른 시간 계산
    {
        double newTime = _time;
        double prevBpm = 0d;
        for ( int i = 0; i < chart.timings.Count; i++ )
        {
            double time = chart.timings[i].time;
            double bpm = chart.timings[i].bpm;

            if ( time > _time ) break;
            //bpm = bpm / chart.medianBpm;
            newTime += ( bpm - prevBpm ) * ( _time - time );
            prevBpm = bpm;
        }
        return ( float )newTime;
    }
}
