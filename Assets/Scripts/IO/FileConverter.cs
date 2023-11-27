using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public struct Song
{
    public int UID;

    public string filePath;
    public string imagePath;
    public string audioPath;
    public int    audioOffset;
    public string videoPath;
    public int    videoOffset;

    public bool hasVideo;
    public bool hasSprite;
    public bool hasKeySound;    // ��Ʈ Ű���� �ϳ��� ���� �� ( ������� ��� X )
    public bool isOnlyKeySound; // Ű�����θ� �̷���� �뷡 ( ������ǰ� ��Ʈ ���� Ű������ �̷���� )

    public string title;
    public string artist;
    public string creator;
    public string version;
    public string source;

    public int totalTime;
    public int previewTime;

    public int keyCount;
    public int noteCount;
    public int sliderCount;
    public int delNoteCount;
    public int delSliderCount;

    public int    minBpm;
    public int    maxBpm;
    public double mainBPM;
}

public struct Timing
{
    public double time;
    public double bpm;
    public double beatLength;
    public int isUninherited;

    public Timing( double _time, double _bpm )
    {
        time = _time;
        bpm = _bpm;
        isUninherited = 0;
        beatLength = 0d;
    }

    public Timing( double _time, double _bpm, double _beatLength, int _isUninherited )
    {
        time = _time;
        beatLength = _beatLength;
        bpm = _bpm;
        isUninherited = _isUninherited;
    }
}

public struct Note
{
    public int lane;
    public double time;
    public double sliderTime;
    public bool isSlider;
    public double noteDistance;
    public double sliderDistance;
    public KeySound keySound;

    public Note( int _lane, double _time, double _sliderTime, KeySound _sound )
    {
        lane = _lane;
        time = _time;
        sliderTime = _sliderTime;
        noteDistance = 0d;
        sliderDistance = 0d;
        isSlider = sliderTime > 0d ? true : false;
        keySound = _sound;
    }
}

public struct KeySound
{
    public string name;
    public float volume;
    public double time;

    public KeySound( double _time, string _name, float _volume )
    {
        time = _time;
        volume = _volume < .1f ? 100f : _volume;
        name = _name;
    }

    public KeySound( Note _note )
    {
        name   = _note.keySound.name;
        volume = _note.keySound.volume < .1f ? 100f : _note.keySound.volume;
        time   = _note.time;
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
        type  = _type;
        start = _start;
        end   = _end;
        name  = _name;
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
    private List<Timing> uninheritedTimings = new List<Timing>();
    private List<Timing> timings            = new List<Timing>();
    private List<Note> notes                = new List<Note>();
    private List<KeySound> samples          = new List<KeySound>();
    private List<SpriteSample> sprites      = new List<SpriteSample>();

    private readonly string[] virtualAudioName = { "preview.wav", "preview.mp3", "preview.ogg" };
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
            int[] deleteKeys = //( _bitCount == 7 ) ? new int[] { 3, }   :
                               ( _bitCount == 8 ) ? new int[] { 0, } : //{ 0, 4 } :
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
            px   = _px;
            lane = -1;
        }
    }
    private class AccumulateTiming
    {
        public double time;
        public double bpm;

        public AccumulateTiming( double _time, double _bpm )
        {
            time = _time;
            bpm  = _bpm;
        }
    }

    public void Load( string _path )
    {
        if ( !File.Exists( Path.ChangeExtension( _path, "wns" ) ) )
             Convert( _path );
    }

    private void Convert( string _path ) 
    {
        try 
        {
            Song song = new Song();
            OpenFile( _path );
#region General

            // [General] ~ [Editor]
            int mode = 0;
            while ( ReadLine() != "[Metadata]" )
            {
                if ( Contains( "AudioFilename:" ) ) song.audioPath   = Split( ':' );
                if ( Contains( "PreviewTime:" ) )   song.previewTime = int.Parse( Split( ':' ) );
                if ( Contains( "Mode:" ) )          mode             = int.Parse( Split( ':' ) );
            }

            // �ǹ��� ��尡 �ƴϸ� ���� ����.
            if ( mode != 3 ) return;
            // [Metadata] ~ [Difficulty]
            while ( ReadLine() != "[Difficulty]" )
            {
                if ( Contains( "Title:" )  && !Contains( "TitleUnicode:" ) )  song.title   = Replace( "Title:",   string.Empty );
                if ( Contains( "Artist:" ) && !Contains( "ArtistUnicode:" ) ) song.artist  = Replace( "Artist:",  string.Empty );
                if ( Contains( "Source:"  ) )                                 song.source  = Replace( "Source:",  string.Empty );
                if ( Contains( "Creator:" ) )                                 song.creator = Replace( "Creator:", string.Empty );
                if ( Contains( "Version:" ) )                                 song.version = Replace( "Version:", string.Empty );
            }

            // [Metadata] ~ [Difficulty]
            while ( ReadLine() != "[Events]" )
            {
                if ( Contains( "CircleSize:" ) ) song.keyCount = int.Parse( Split( ':' ) );
            }

            // Ű�������� ����Ǵ� �뷡�� ������ ������ ��κ� ����.
            // preview.wav�� ���� ���α׷��� ���� �������� �����̴�.
            if ( song.audioPath == null || song.audioPath == string.Empty || song.audioPath == "virtual" )
            {
                for ( int i = 0; i < virtualAudioName.Length; i++ )
                {
                    if ( File.Exists( Path.Combine( dir, virtualAudioName[i] ) ) )
                    {
                        song.audioPath = virtualAudioName[i];
                        song.isOnlyKeySound = true;
                        break;
                    }
                }
            }

            // [Events]
            samples?.Clear();
            sprites?.Clear();

            var directory = Path.GetDirectoryName( _path );
            while ( ReadLine() != "[TimingPoints]" )
            {
                // Image
                if ( Contains( "0,0," ) && !( Contains( ".mp3" ) || Contains( ".wav" ) || Contains( ".ogg" ) || Contains( "Video," ) || Contains( "Sprite," ) ) )
                     song.imagePath = Split( '"' );

                // Video
                if ( Contains( "Video," ) )
                {
                    var splitData = line.Split( ',' );
                    var path = splitData[2].Split( '"' )[1].Trim();
                    if ( Path.GetExtension( path ) != ".mpg" )
                    {
                        song.videoPath = path;
                        song.videoOffset = int.Parse( splitData[1] );
                        song.hasVideo = File.Exists( Path.Combine( directory, song.videoPath ) );
                    }
                }

                // Sprite
                if ( Contains( "Sprite," ) )
                {
                    string[] splitSprite = line.Split( ',' );
                    string name = Split( '"' );
                    var type = splitSprite[1].Contains( "Background" ) ? SpriteType.Background : splitSprite[1].Contains( "Foreground" ) ? SpriteType.Foreground : SpriteType.None;
                    string[] splitTime = ReadLine().Split( ',' );
                    sprites.Add( new SpriteSample( type, float.Parse( splitTime[2] ), float.Parse( splitTime[3] ), name ) );
                }

                // HitSound
                // <filepath> is the same concept as with sprites, only referring to the .wav, .mp3, or .ogg file.
                if ( Contains( ".mp3" ) || Contains( ".wav" ) || Contains( ".ogg" ) )
                {
                    string[] split = line.Split( ',' );
                    string name    = Split( '"' );
                    samples.Add( new KeySound( float.Parse( split[1] ), name, float.Parse( split[4] ) ) );
                }
            }

            sprites.Sort( delegate ( SpriteSample _A, SpriteSample _B )
            {
                if ( _A.start > _B.start )      return 1;
                else if ( _A.start < _B.start ) return -1;
                else                            return 0;
            } );

            if ( sprites.Count > 0 )
                 song.hasSprite = true;
             
#endregion
            timings?.Clear();
            uninheritedTimings?.Clear();

            double uninheritedBeat = 0d;
            song.minBpm = int.MaxValue;

            #region Timing
            while ( ReadLine() != "[HitObjects]" )
            {
                string[] splitDatas = line.Split( ',' );
                if ( splitDatas.Length != 8 ) continue;

                double time          = double.Parse( splitDatas[0] );
                double beatLengthAbs = Math.Abs( double.Parse( splitDatas[1] ) );
                int isUninherited    = int.Parse( splitDatas[6] );

                if ( isUninherited == 1 )
                {
                    uninheritedBeat = beatLengthAbs;
                    uninheritedTimings.Add( new Timing( time, 1d / beatLengthAbs * 60000d, beatLengthAbs, isUninherited ) );
                }
                else
                {
                    beatLengthAbs = uninheritedBeat * ( beatLengthAbs * .01d );
                }

                double BPM = 1d / beatLengthAbs * 60000d;
                if ( song.minBpm > BPM ) song.minBpm = Mathf.RoundToInt( ( float )BPM );
                if ( song.maxBpm < BPM ) song.maxBpm = Mathf.RoundToInt( ( float )BPM );

                timings.Add( new Timing( time, BPM, beatLengthAbs, isUninherited ) );
            }
            #endregion

#region Note
            notes?.Clear();
            bool isCheckKeySoundOnce = false;
            DeleteKey deleteKey = new DeleteKey( song.keyCount );
            while ( ReadLineEndOfStream() ) 
            {
                if ( line == null ) continue;

                string[] splitDatas = line.Split( ',' );
                string[] objParams = splitDatas[5].Split( ':' );
                double noteTime    = double.Parse( splitDatas[2] );
                double sliderTime  = 0d;
                if ( int.Parse( splitDatas[3] ) == 2 << 6 )
                     sliderTime = double.Parse( objParams[0] );

                // ���ŵ� ��Ʈ�� Ű���� �ڵ����� ����Ǵ� ��������� ����Ѵ�.
                int originLane    = Mathf.FloorToInt( int.Parse( splitDatas[0] ) * song.keyCount / 512 );
                int finalLane     = deleteKey.FinalKey( originLane );
                KeySound keySound = new KeySound( noteTime, objParams[objParams.Length - 1], 
                                                  float.Parse( objParams[objParams.Length - 2] ) );
                if ( deleteKey[originLane] )
                {
                    samples.Add( keySound );
                }
                else
                {
                    if ( sliderTime > 0d ) {
                        song.totalTime = song.totalTime >= sliderTime ? song.totalTime : ( int )sliderTime;
                        song.sliderCount++;

                        if ( finalLane == 3 )
                             song.delSliderCount++;
                    }
                    else {
                        song.totalTime = song.totalTime >= noteTime ? song.totalTime : ( int )noteTime;
                        song.noteCount++;

                        if ( finalLane == 3 )
                             song.delNoteCount++;
                    }

                    notes.Add( new Note( finalLane, noteTime, sliderTime, keySound ) );

                    if ( !isCheckKeySoundOnce )
                    {
                        if ( File.Exists( Path.Combine( directory, keySound.name ) ) )
                        {
                            song.hasKeySound    = true;
                            isCheckKeySoundOnce = true;
                        }
                    }
                }
            }

            // BMS2Osu�� ���� ������ Pixel�� �������� ���ĵǾ� �ֱ� ������ �ð� ������ �ٽ� �������ش�.
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
            song.mainBPM = GetMainBPM();

            Write( in song );
            Dispose();
        }
        catch ( System.Exception _error ) 
        {
            Dispose();
            #if UNITY_EDITOR
            // ���� ��ġ ã��
            System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace( _error, true );
            Debug.LogWarning( $"{trace.GetFrame( 0 ).GetFileLineNumber()} {_error.Message}  {Path.GetFileName( path )}" );
            #endif
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
                    writer.WriteLine( $"ImagePath: {_song.imagePath}" );
                    writer.WriteLine( $"AudioPath: {_song.audioPath}" );
                    writer.WriteLine( $"VideoPath: {_song.videoPath}" );
                    writer.WriteLine( $"VideoOffset: {_song.videoOffset}" );

                    writer.WriteLine( $"Title: {_song.title}" );
                    writer.WriteLine( $"Artist: {_song.artist}" );
                    writer.WriteLine( $"Source: {_song.source}" );
                    writer.WriteLine( $"Creator: {_song.creator}" );
                    writer.WriteLine( $"Version: {_song.version}" );

                    writer.WriteLine( $"PreviewTime: {_song.previewTime}" );
                    writer.WriteLine( $"TotalTime: {_song.totalTime}" );

                    writer.WriteLine( $"KeyCount: {_song.keyCount}" );
                    writer.WriteLine( $"NumNote: {_song.noteCount}" );
                    writer.WriteLine( $"NumSlider: {_song.sliderCount}" );
                    writer.WriteLine( $"NumDelNote: {_song.delNoteCount}" );
                    writer.WriteLine( $"NumDelSlider: {_song.delSliderCount}" );

                    writer.WriteLine( $"MinBPM: {_song.minBpm}" );
                    writer.WriteLine( $"MaxBPM: {_song.maxBpm}" );
                    writer.WriteLine( $"MainBPM: {_song.mainBPM}" );

                    writer.WriteLine( $"DataExist: {( _song.isOnlyKeySound ? 1 : 0 )}:" +
                                                 $"{( _song.hasKeySound    ? 1 : 0 )}:" +
                                                 $"{( _song.hasVideo       ? 1 : 0 )}:" +
                                                 $"{( _song.hasSprite      ? 1 : 0 )}" );

                    StringBuilder text = new StringBuilder();
                    writer.WriteLine( "[Timings]" );
                    for ( int i = 0; i < timings.Count; i++ )
                    {
                        text.Clear();
                        text.Append( timings[i].time ).Append( "," );
                        text.Append( timings[i].beatLength ).Append( "," );
                        text.Append( timings[i].isUninherited );

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

        // ���� ���� ���� ( �׳� ���� ������ ��� )
        // if ( File.Exists( path ) )
        // {
        //     Debug.Log( $"File Delete : {path}" );
        //     File.Delete( path );
        // }
    }

    private double GetMainBPM()
    {
        List<AccumulateTiming> accumulateTimings = new List<AccumulateTiming>();
        // ��ӵ������� BPM���� ����Ѵ�.
        for ( int i = 0; i < uninheritedTimings.Count; i++ )
        {
            // ������ ������ ������ ��Ʈ �ð����� �����Ѵ�.
            double nextTime = ( i + 1 < uninheritedTimings.Count ) ? uninheritedTimings[i + 1].time : notes.Last().time;

            // Ÿ�̹� ���ӽð� ����
            bool isFind = false;
            for ( int j = 0; j < accumulateTimings.Count; j++ )
            {
                double diff = Math.Round( accumulateTimings[j].bpm - uninheritedTimings[i].bpm );
                if ( Math.Abs( diff ) < double.Epsilon )
                {
                    isFind = true;
                    accumulateTimings[j].time += nextTime - uninheritedTimings[i].time;
                    break;
                }
            }

            // �񱳵� ���ο� Ÿ�̹� �߰�
            if ( !isFind )
                 accumulateTimings.Add( new AccumulateTiming( nextTime - uninheritedTimings[i].time, uninheritedTimings[i].bpm ) );
        }

        if ( accumulateTimings.Count == 0 )
             throw new Exception( "The MainBPM was not found." );

        // ���� ���� ���ӵǴ� BPM�� ù��° ��Ұ� �ǵ��� �������� ����
        accumulateTimings.Sort( delegate ( AccumulateTiming _left, AccumulateTiming _right )
        {
            if      ( _left.time < _right.time ) return 1;
            else if ( _left.time > _right.time ) return -1;
            else                                 return 0;
        } );

        return accumulateTimings[0].bpm;
    }
}
