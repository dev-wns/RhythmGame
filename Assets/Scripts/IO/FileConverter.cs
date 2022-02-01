using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public double medianBpm;
}

public struct Timing : IEquatable<Timing>
{
    public double time;
    public double bpm;
    public double beatLength;

    public override bool Equals( object _obj )
    {
        Debug.Log( "boxing" );
        return base.Equals( _obj );
    }
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public bool Equals( Timing _other )
    {
        return ( time == _other.time ) && ( bpm == _other.bpm );
    }

    public static bool operator == ( in Timing _left, in Timing _right )
    {
        return _left.Equals( _right );
    }

    public static bool operator != ( in Timing _left, in Timing _right )
    {
        return !( _left == _right );
    }

    public Timing( Timing _timing )
    {
        time = _timing.time;
        beatLength = _timing.beatLength;
        bpm = _timing.bpm;
    }
    public Timing( double _time, double _bpm )
    {
        time = _time;
        bpm = _bpm;
        beatLength = 0d;
    }
    public Timing( double _time, double _bpm, double _beatLength )
    {
        time = _time;
        beatLength = _beatLength;
        bpm = _bpm;
    }
}

public struct Note
{
    public int lane;
    public double time;
    public double sliderTime;
    public bool isSlider;
    public double calcTime;
    public double calcSliderTime;
    public KeySound keySound;

    public Note( int _lane, double _time, double _sliderTime, KeySound _keySound )
    {
        lane = _lane;
        time = _time;
        sliderTime = _sliderTime;
        calcTime = 0f;
        calcSliderTime = 0f;
        isSlider = false;
        keySound = _keySound;
    }
}

public struct KeySound
{
    public float volume;
    public string name;

    public KeySound( float _volume, string _name )
    {
        volume = _volume;
        name = _name;
    }
}

public struct KeySample
{
    public double time;
    public KeySound sound;
  
    public KeySample( double _time, string _name, float _volume )
    {
        time = _time;
        sound.name = _name;
        sound.volume = _volume;
    }
}

public struct Chart
{
    public ReadOnlyCollection<Timing> timings;
    public ReadOnlyCollection<Note> notes;
    public ReadOnlyCollection<KeySample> samples;
    public ReadOnlyCollection<string> keySoundNames;
}

public class FileConverter : FileReader
{
    private List<Timing> timings = new List<Timing>();
    private List<Note> notes = new List<Note>();
    private List<KeySample> samples = new List<KeySample>();
    private List<string> keySoundNames = new List<string>();

    private readonly string virtualAudioName = "preview.wav";

    private class CalcMedianTiming
    {
        public double time, bpm;

        public CalcMedianTiming( Timing _timing )
        {
            time = _timing.time;
            bpm = _timing.bpm;
        }
        public CalcMedianTiming( double _time, double _bpm )
        {
            time = _time;
            bpm = _bpm;
        }
    }

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
        try
        {
            Song song = new Song();

            OpenFile( _path );

            #region General Data Parsing
            // [General] ~ [Editor]
            while ( ReadLine() != "[Metadata]" )
            {
                if ( Contains( "AudioFilename" ) ) song.audioPath   = SplitAndTrim( ':' );
                if ( Contains( "PreviewTime" ) )   song.previewTime = int.Parse( SplitAndTrim( ':' ) );
            }

            // [Metadata] ~ [Difficulty]
            while ( ReadLine() != "[Events]" )
            {
                if ( Contains( "Title" )  && !Contains( "TitleUnicode:" ) ) song.title   = SplitAndTrim( ':' );
                if ( Contains( "Artist" ) && !Contains( "ArtistUnicode" ) ) song.artist  = SplitAndTrim( ':' );
                if ( Contains( "Creator" ) )                                song.creator = SplitAndTrim( ':' );
                if ( Contains( "Version" ) )                                song.version = SplitAndTrim( ':' );
            }

            // 키음만으로 재생되는 노래는 프리뷰 음악이 대부분 없다.
            // preview.wav는 따로 프로그램을 통해 만들어놓은 파일이다.
            if ( song.audioPath == null || song.audioPath == string.Empty )
            {
                if ( File.Exists( Path.Combine( dir, virtualAudioName ) ) )
                {
                    song.audioPath = virtualAudioName;
                }
            }

            // [Events]
            samples?.Clear();
            samples ??= new List<KeySample>();

            var directory = Path.GetDirectoryName( _path );
            while ( ReadLine() != "[TimingPoints]" )
            {
                if ( Contains( ".avi" ) || Contains( ".mp4" ) || Contains( ".mpg" ) )
                {
                    song.videoPath = SplitAndTrim( '"' );
                    song.hasVideo = File.Exists( Path.Combine( directory, song.videoPath ) ) ? true : false;
                }

                if ( Contains( ".jpg" ) || Contains( ".png" ) )
                {
                    song.imagePath = SplitAndTrim( '"' );
                }

                if ( Contains( "Sample," ) )
                {
                    string[] split = line.Split( ',' );
                    string name    = SplitAndTrim( '"' );
                    samples.Add( new KeySample( float.Parse( split[1] ), name, float.Parse( split[4] ) ) );
                }
            }

            //

            samples.Sort( delegate ( KeySample _A, KeySample _B )
            {
                if ( _A.time > _B.time ) return 1;
                else if ( _A.time < _B.time ) return -1;
                else return 0;
            });

            #endregion

            #region Timings Parsing
            timings?.Clear();
            timings ??= new List<Timing>();

            // [TimingPoints]
            double uninheritedBpm = 0d;
            Timing prevTiming = new Timing();
            while ( ReadLine() != "[HitObjects]" )
            {
                string[] splitDatas = line.Split( ',' );
                if ( splitDatas.Length != 8 ) continue;

                double beatLength = Globals.Abs( double.Parse( splitDatas[1] ) );
                double BPM = 1d / beatLength * 60000d;

                // 상속된 bpm은 부모 bpm의 백분율 값을 가진다.
                bool isUninherited = int.Parse( splitDatas[6] ) == 0 ? false : true;
                if ( isUninherited ) uninheritedBpm = BPM;
                else                 BPM = ( uninheritedBpm * 100d ) / beatLength;

                if ( song.minBpm >= BPM || song.minBpm == 0 ) song.minBpm = Mathf.RoundToInt( ( float )BPM );
                if ( song.maxBpm <= BPM )                     song.maxBpm = Mathf.RoundToInt( ( float )BPM );

                double time = double.Parse( splitDatas[0] );
                Timing timing = new Timing( time, BPM, 60000d / BPM );
                if ( prevTiming.bpm != timing.bpm )
                {
                    timings.Add( timing );
                    prevTiming = timing;
                }

                song.timingCount++;
            }

            if ( timings.Count == 0 )
                throw new Exception( "Timing Convert Error" );

            #endregion

            #region Notes Parsing
            notes?.Clear();
            notes ??= new List<Note>();

            keySoundNames.Clear();
            keySoundNames ??= new List<string>();

            // [HitObjects]
            while ( ReadLineEndOfStream() )
            {
                string[] splitDatas = line.Split( ',' );
                if ( splitDatas.Length != 6 ) continue;

                double noteTime = double.Parse( splitDatas[2] );
                double sliderTime = 0d;

                string[] objParams = splitDatas[5].Split( ':' );
                bool isSlider = int.Parse( splitDatas[3] ) == 128 ? true : false;
                if ( isSlider )
                {
                    sliderTime = double.Parse( objParams[0] );
                    
                    song.sliderCount++;
                    song.totalTime = song.totalTime >= sliderTime ? song.totalTime : ( int )sliderTime;
                }
                else
                {
                    song.noteCount++;
                    song.totalTime = song.totalTime >= noteTime ? song.totalTime : ( int )noteTime;
                }

                KeySound keySound = new KeySound( float.Parse( objParams[objParams.Length - 2] ), objParams[objParams.Length - 1] );
                int lane = Mathf.FloorToInt( int.Parse( splitDatas[0] ) * 6 / 512 );
                notes.Add( new Note( lane, noteTime, sliderTime, keySound ) );

                if ( keySound.name != string.Empty )
                {
                    if ( !keySoundNames.Contains( keySound.name ) )
                         keySoundNames.Add( keySound.name );
                }
            }

            if ( notes.Count == 0 )
                 throw new Exception( "Note Convert Error" );

            #endregion

            song.medianBpm = GetMedianBpm();
            timings[0] = new Timing( -5000d, timings[0].bpm, timings[0].beatLength );

            Write( in song );
        }
        catch ( Exception _error )
        {
            //if ( !Directory.Exists( GameSetting.FailedPath ) )
            //      Directory.CreateDirectory( GameSetting.FailedPath );

            //if ( File.Exists( path ) )
            //{
            //    File.Move( path, GameSetting.FailedPath );
            //    Debug.LogWarning( $"File Move Failed Directory : {path}" );
            //}

            Dispose();
            Debug.LogError( $"{_error}, {path}" );
        }
    }

    private void Write( in Song _song )
    {
        try
        {
            string fileName = $"{Path.GetFileNameWithoutExtension( path )}.wns";
            string filePath = @$"\\?\{Path.Combine( Path.GetDirectoryName( path ), fileName )}";

            using ( var stream = new FileStream( filePath, FileMode.Create ) )
            {
                using ( var writer = new StreamWriter( stream ) )
                {
                    writer.WriteLine( "[General]" );
                    writer.WriteLine( $"AudioPath: {_song.audioPath}" );
                    writer.WriteLine( $"ImagePath: {_song.imagePath}" );
                    writer.WriteLine( $"VideoPath: {_song.videoPath}" );

                    writer.WriteLine( $"Title: {_song.title}" );
                    writer.WriteLine( $"Artist: {_song.artist}" );
                    writer.WriteLine( $"Creator: {_song.creator}" );
                    writer.WriteLine( $"Version: {_song.version}" );

                    writer.WriteLine( $"PreviewTime: {_song.previewTime}" );
                    writer.WriteLine( $"TotalTime: {_song.totalTime}" );

                    writer.WriteLine( $"NumNote: {_song.noteCount}" );
                    writer.WriteLine( $"NumSlider: {_song.sliderCount}" );
                    writer.WriteLine( $"NumTiming: {_song.timingCount}" );

                    writer.WriteLine( $"MinBPM: {_song.minBpm}" );
                    writer.WriteLine( $"MaxBPM: {_song.maxBpm}" );
                    writer.WriteLine( $"Median: {_song.medianBpm}" );

                    StringBuilder text = new StringBuilder();
                    writer.WriteLine( "[Timings]" );
                    for ( int i = 0; i < timings.Count; i++ )
                    {
                        text.Clear();
                        text.Append( timings[i].time ).Append( "," );
                        text.Append( timings[i].beatLength );

                        writer.WriteLine( text );
                    }

                    writer.WriteLine( "[Samples]" );
                    for ( int i = 0; i < samples.Count; i++ )
                    {
                        text.Clear();
                        text.Append( samples[i].time ).Append( "," );
                        text.Append( samples[i].sound.volume ).Append( "," );
                        text.Append( samples[i].sound.name );

                        writer.WriteLine( text );
                    }

                    writer.WriteLine( "[KeySounds]" );
                    for ( int i = 0; i < keySoundNames.Count; i++ )
                    {
                        text.Clear();
                        text.Append( keySoundNames[i] );

                        writer.WriteLine( text );
                    }

                    writer.WriteLine( "[Notes]" );
                    for ( int i = 0; i < notes.Count; i++ )
                    {
                        text.Clear();
                        text.Append( notes[i].lane ).Append( "," );
                        text.Append( notes[i].time ).Append( "," );
                        text.Append( notes[i].sliderTime ).Append( "," );

                        text.Append( notes[i].keySound.volume ).Append( ":" );
                        text.Append( notes[i].keySound.name );

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

    private double GetMedianBpm()
    {        
        // 값 전부 복사해서 계산해도 되지만 타이밍도 몇천개 있을 수도 있다.
        // 첫번째, 마지막 타이밍만 수정한 후 계산 끝나면 돌려놓자.
        var firstTimingCached = new Timing( timings[0] );

        // 노트 기준으로 길이 측정 하기위해 첫번째, 마지막 타이밍 수정
        // 마지막 Timing Bpm은 마지막 노트 시간까지의 길이로 계산한다.
        timings[0] = new Timing( notes[0].time, timings[0].bpm );
        timings.Add( new Timing( notes[notes.Count - 1].time, timings[timings.Count - 1].bpm ) );
        
        List<CalcMedianTiming> medianCalc = new List<CalcMedianTiming>();
        medianCalc.Add( new CalcMedianTiming( timings[0] ) );
        for ( int i = 1; i < timings.Count; i++ )
        {
            double prevTime = timings[i - 1].time;
            double prevBpm  = timings[i - 1].bpm;

            bool isFind = false;
            for ( int j = 0; j < medianCalc.Count; j++ )
            {
                if ( Globals.Abs( medianCalc[j].bpm - prevBpm ) < .1f )
                {
                    isFind = true;
                    medianCalc[j].time += timings[i].time - prevTime;
                }
            }

            if ( !isFind ) medianCalc.Add( new CalcMedianTiming( timings[i].time - prevTime, prevBpm ) );
        }

        // 내림차순 정렬
        medianCalc.Sort( delegate ( CalcMedianTiming A, CalcMedianTiming B )
        {
            if ( A.time < B.time )      return 1;
            else if ( A.time > B.time ) return -1;
            else                        return 0;
        } );

        // 계산 끝난 후 값 돌려놓기.
        timings[0] = firstTimingCached;
        timings.RemoveAt( timings.Count - 1 );

        return medianCalc[0].bpm;
    }
}
