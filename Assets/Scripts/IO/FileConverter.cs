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
    public int    videoOffset;

    public string title;
    public string artist;
    public string creator;
    public string version;

    public int previewTime;
    public int totalTime;
    public bool isOnlyKeySound;

    public int noteCount;
    public int sliderCount;
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

    public Note( int _lane, double _time, double _sliderTime, KeySound _sound )
    {
        lane = _lane;
        time = _time;
        sliderTime = _sliderTime;
        calcTime = 0d;
        calcSliderTime = 0d;
        isSlider = sliderTime > 0d ? true : false;
        keySound = _sound;
    }
}

public struct KeySound
{
    public string name;
    public float volume;
    public double time;
    public FMOD.Sound sound;
    public bool hasSound;
  
    public KeySound( double _time, string _name, float _volume )
    {
        time = _time;
        volume = _volume < .1f ? 100f : _volume;
        name = _name;
        sound = new FMOD.Sound();
        hasSound = false;
    }

    public KeySound( Note _note )
    {
        name = _note.keySound.name;
        volume = _note.keySound.volume < .1f ? 100f : _note.keySound.volume;
        time = _note.keySound.time;
        sound = new FMOD.Sound();
        hasSound = false;
    }
}

public enum SpriteType { None, Background, Foreground }
public struct SpriteSample
{
    public SpriteType type;
    public string name;
    public double start, end;

    public SpriteSample( SpriteType _type, double _start, double _end, string _name )
    {
        type = _type;
        start = _start;
        end = _end;
        name = _name;
    }
}

public struct Chart
{
    public ReadOnlyCollection<Timing> timings;
    public ReadOnlyCollection<Note> notes;
    public ReadOnlyCollection<KeySound> samples;
    public ReadOnlyCollection<SpriteSample> sprites;
}


public class FileConverter : FileReader
{
    private List<Timing> timings       = new List<Timing>();
    private List<Note> notes           = new List<Note>();
    private List<KeySound> samples     = new List<KeySound>();
    private List<SpriteSample> sprites = new List<SpriteSample>();

    private readonly string virtualAudioName = "preview.wav";

    private class IntegerComparer : IComparer<int>
    {
        int IComparer<int>.Compare( int _left, int _right )
        {
            if ( _left > _right )      return 1;
            else if ( _left < _right ) return -1;
            else                       return 0;
        }
    }

    private class DeleteKey
    {
        public BitArray bits { get; private set; }
        public Dictionary<int/* lane */, int /* final key */> keys = new Dictionary<int, int>();
        
        public int FinalKey( int _lane ) => keys[_lane];

        public DeleteKey( int _bitCount )
        {
            bits = new BitArray( _bitCount );
            int[] deleteKeys = ( _bitCount == 7 ) ? new int[] { 6, }   :
                               ( _bitCount == 8 ) ? new int[] { 0, 7 } :
                                                    new int[] { -1, };

            int finalLane = -1;
            for ( int i = 0; i < bits.Length; i++ )
            {
                for ( int j = 0; j < deleteKeys.Length; j++ )
                {
                    if ( i == deleteKeys[j] )
                    {
                        bits[i] = true;
                        break;
                    }
                }

                keys.Add( i, bits[i] ? -1 : ++finalLane );
            }
        }

        public bool this[int _key] => bits[_key];
    }

    private class LaneData : IEquatable<LaneData>
    {
        public int px;
        public int lane;

        public override bool Equals( object obj )
        {
            return base.Equals( obj );
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool Equals( LaneData _data )
        {
            return px == _data.px;
        }

        public LaneData( int _px )
        {
            px = _px;
            lane = -1;
        }
    }
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
            int mode = 0;
            while ( ReadLine() != "[Metadata]" )
            {
                if ( Contains( "AudioFilename" ) ) song.audioPath   = Split( ':' );
                if ( Contains( "PreviewTime" ) )   song.previewTime = int.Parse( Split( ':' ) );
                if ( Contains( "Mode" ) )          mode             = int.Parse( Split( ':' ) );
            }
            // 건반형 모드가 아니면 읽지 않음.
            if ( mode != 3 ) return;

            // [Metadata] ~ [Difficulty]
            while ( ReadLine() != "[Difficulty]" )
            {
                if ( Contains( "Title" )  && !Contains( "TitleUnicode" ) )  song.title   = Replace( "Title:",   string.Empty );
                if ( Contains( "Artist" ) && !Contains( "ArtistUnicode" ) ) song.artist  = Replace( "Artist:",  string.Empty );
                if ( Contains( "Creator" ) )                                song.creator = Replace( "Creator:", string.Empty );
                if ( Contains( "Version" ) )                                song.version = Replace( "Version:", string.Empty );
            }

            // [Metadata] ~ [Difficulty]
            int keyCount = 0;
            while ( ReadLine() != "[Events]" )
            {
                if ( Contains( "CircleSize" ) ) keyCount = int.Parse( Split( ':' ) );
            }

            // 키음만으로 재생되는 노래는 프리뷰 음악이 대부분 없다.
            // preview.wav는 따로 프로그램을 통해 만들어놓은 파일이다.
            if ( song.audioPath == null || song.audioPath == string.Empty )
            {
                if ( File.Exists( Path.Combine( dir, virtualAudioName ) ) )
                {
                    song.audioPath = virtualAudioName;
                    song.isOnlyKeySound = true;
                }
            }

            // [Events]
            samples?.Clear();
            sprites?.Clear();

            var directory = Path.GetDirectoryName( _path );
            while ( ReadLine() != "[TimingPoints]" )
            {
                if ( Contains( ".avi" ) || Contains( ".mp4" ) )
                {
                    var splitData = line.Split( ',' );
                    song.videoOffset = int.Parse( splitData[1] );
                    song.videoPath   = splitData[2].Split( '"' )[1].Trim();
                    song.hasVideo    = File.Exists( Path.Combine( directory, song.videoPath ) ) ? true : false;
                }

                if ( ( Contains( ".jpg" ) || Contains( ".png" ) || Contains( ".bmp" ) ) && ( !Contains( "Sprite," ) ) )
                {
                    song.imagePath = Split( '"' );
                }

                if ( Contains( "Sprite," ) )
                {
                    string[] splitSprite = line.Split( ',' );
                    string name = Split( '"' );
                    var type = splitSprite[1].Contains( "Background" ) ? SpriteType.Background : splitSprite[1].Contains( "Foreground" ) ? SpriteType.Foreground : SpriteType.None;
                    string[] splitTime = ReadLine().Split( ',' );
                    sprites.Add( new SpriteSample( type, float.Parse( splitTime[2] ), float.Parse( splitTime[3] ), name ) );
                }

                if ( Contains( "Sample," ) )
                {
                    string[] split = line.Split( ',' );
                    string name    = Split( '"' );
                    samples.Add( new KeySound( float.Parse( split[1] ), name, float.Parse( split[4] ) ) );
                }
            }
#endregion

#region Timings Parsing
            timings?.Clear();

            // [TimingPoints]
            double uninheritedBeat = 0d;
            Timing prevTiming = new Timing();
            song.minBpm = int.MaxValue;
            while ( ReadLine() != "[HitObjects]" )
            {
                string[] splitDatas = line.Split( ',' );
                if ( splitDatas.Length != 8 )
                    continue;

                // 상속된 BeatLength는 음수이기 때문에 절대값 변환 후 계산한다.
                double beatLength = Globals.Abs( double.Parse( splitDatas[1] ) );

                // 상속된 bpm은 부모 bpm의 역백분율 값을 가진다. ( 100 = 1배, 50 = 2배 ... )
                if ( int.Parse( splitDatas[6] ) == 1 ) uninheritedBeat = beatLength;
                else                                   beatLength = uninheritedBeat * ( beatLength * .01d );

                double BPM = 1d / beatLength * 60000d;
                if ( song.minBpm > BPM ) song.minBpm = Mathf.RoundToInt( ( float )BPM );
                if ( song.maxBpm < BPM ) song.maxBpm = Mathf.RoundToInt( ( float )BPM );

                double time = double.Parse( splitDatas[0] );
                Timing timing = new Timing( time, BPM, beatLength );
                if ( prevTiming.bpm != timing.bpm )
                {
                    timings.Add( timing );
                    prevTiming = timing;
                }

                //string[] splitDatas = line.Split( ',' );
                //if ( splitDatas.Length != 8 ) continue;

                //// 상속된 BeatLength는 음수이기 때문에 절대값 변환 후 계산한다.
                //double beatLength = Globals.Abs( double.Parse( splitDatas[1] ) );
                //double BPM = 1d / beatLength * 60000d;

                //// 상속된 bpm은 부모 bpm의 역백분율 값을 가진다. ( 100 = 1배, 50 = 2배 ... )
                //bool isUninherited = int.Parse( splitDatas[6] ) == 1;
                //if ( isUninherited ) uninheritedBpm = BPM;
                //else                 BPM = ( uninheritedBpm * 100d ) / beatLength;

                //if ( song.minBpm >= BPM || song.minBpm == 0 ) song.minBpm = Mathf.RoundToInt( ( float )BPM );
                //if ( song.maxBpm <= BPM )                     song.maxBpm = Mathf.RoundToInt( ( float )BPM );

                //double time = double.Parse( splitDatas[0] );
                //Timing timing = new Timing( time, BPM, 60000d / BPM );
                //if ( prevTiming.bpm != timing.bpm )
                //{
                //    timings.Add( timing );
                //    prevTiming = timing;
                //}
            }
#endregion

#region Notes Parsing
            // [HitObjects]
            notes?.Clear();
            DeleteKey deleteKey = new DeleteKey( keyCount );

            while ( ReadLineEndOfStream() )
            {
                string[] splitDatas = line.Split( ',' );
                string[] objParams = splitDatas[5].Split( ':' );
                
                double noteTime    = double.Parse( splitDatas[2] );
                double sliderTime  = 0d;
                if ( int.Parse( splitDatas[3] ) == 2 << 6 )
                     sliderTime = double.Parse( objParams[0] );

                // 잘린 노트의 키음은 자동으로 재생되는 KeySample로 만들어 준다.
                int originLane    = Mathf.FloorToInt( int.Parse( splitDatas[0] ) * keyCount / 512 );
                int finalLane     = deleteKey.FinalKey( originLane );
                KeySound keySound = new KeySound( noteTime, objParams[objParams.Length - 1], float.Parse( objParams[objParams.Length - 2] ) );

                if ( deleteKey[originLane] )
                {
                    samples.Add( keySound );
                }
                else
                {
                    if ( sliderTime > 0d )
                    {
                        song.totalTime = song.totalTime >= sliderTime ? song.totalTime : ( int )sliderTime;
                        song.sliderCount++;
                    }
                    else
                    {
                        song.totalTime = song.totalTime >= noteTime ? song.totalTime : ( int )noteTime;
                        song.noteCount++;
                    }

                    notes.Add( new Note( finalLane, noteTime, sliderTime, keySound ) );
                }
            }

            // BMS2Osu로 뽑은 파일은 Pixel값 기준으로 정렬되어 있기 때문에 시간 순으로 다시 정렬해준다.
            samples.Sort( delegate ( KeySound _A, KeySound _B )
            {
                if ( _A.time > _B.time )      return 1;
                else if ( _A.time < _B.time ) return -1;
                else                          return 0;
            } );

            notes.Sort( delegate ( Note _A, Note _B )
            {
                if ( _A.time > _B.time )      return 1;
                else if ( _A.time < _B.time ) return -1;
                else                          return 0;
            } );
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
            Debug.LogError( $"{_error} : {_error.Message}" );
        }
    }

    public int GetLineNumber( Exception ex )
    {
        var lineNumber = 0;
        const string lineSearch = ":line ";
        var index = ex.StackTrace.LastIndexOf( lineSearch );
        if ( index != -1 )
        {
            var lineNumberText = ex.StackTrace.Substring( index + lineSearch.Length );
            if ( int.TryParse( lineNumberText, out lineNumber ) )
            {
            }
        }
        return lineNumber;
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
                    writer.WriteLine( $"VideoOffset: {_song.videoOffset}" );

                    writer.WriteLine( $"Title: {_song.title}" );
                    writer.WriteLine( $"Artist: {_song.artist}" );
                    writer.WriteLine( $"Creator: {_song.creator}" );
                    writer.WriteLine( $"Version: {_song.version}" );

                    writer.WriteLine( $"PreviewTime: {_song.previewTime}" );
                    writer.WriteLine( $"TotalTime: {_song.totalTime}" );

                    writer.WriteLine( $"NumNote: {_song.noteCount}" );
                    writer.WriteLine( $"NumSlider: {_song.sliderCount}" );

                    writer.WriteLine( $"MinBPM: {_song.minBpm}" );
                    writer.WriteLine( $"MaxBPM: {_song.maxBpm}" );
                    writer.WriteLine( $"Median: {_song.medianBpm}" );
                    writer.WriteLine( $"Virtual: {( _song.isOnlyKeySound ? 1 : 0 )}" );

                    StringBuilder text = new StringBuilder();
                    writer.WriteLine( "[Timings]" );
                    for ( int i = 0; i < timings.Count; i++ )
                    {
                        text.Clear();
                        text.Append( timings[i].time ).Append( "," );
                        text.Append( timings[i].beatLength );

                        writer.WriteLine( text );
                    }

                    writer.WriteLine( "[Sprites]" );
                    for ( int i = 0; i < sprites.Count; i++ )
                    {
                        text.Clear();
                        text.Append( ( int )sprites[i].type ).Append( "," );
                        text.Append( sprites[i].start ).Append( "," );
                        text.Append( sprites[i].end ).Append( "," );
                        text.Append( sprites[i].name );

                        writer.WriteLine( text );
                    }

                    writer.WriteLine( "[Samples]" );
                    for ( int i = 0; i < samples.Count; i++ )
                    {
                        text.Clear();
                        text.Append( samples[i].time ).Append( "," );
                        text.Append( samples[i].volume ).Append( "," );
                        text.Append( samples[i].name );

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

        // 오름차순 정렬 ( 가장 오래 유지되는 BPM이 첫번째요소가 되도록 )
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
