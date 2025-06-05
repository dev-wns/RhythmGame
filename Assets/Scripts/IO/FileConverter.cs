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
    // 파싱 단계에서 갱신
    public int index;
    public string filePath;
    public string directory;

    public string imagePath;
    public string audioPath;
    public string videoPath;

    // 파일 내 데이터
    public string title;
    public string artist;
    public string creator;
    public string version;
    public string source;

    public string imageName;
    public string audioName;
    public string videoName;
    public int    audioOffset;
    public int    videoOffset;
    public int    volume;

    public int totalTime;
    public int previewTime;

    public bool hasVideo;
    public bool hasSprite;
    public bool hasKeySound;    // 노트 키음이 하나라도 있을 때 ( 배경음은 상관 X )
    public bool isOnlyKeySound; // 키음으로만 이루어진 노래 ( 배경음악과 노트 전부 키음으로 이루어짐 )

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
    public int      lane;
    public double   time;
    public double   sliderTime;
    public double   noteDistance;
    public double   sliderDistance;
    public bool     isSlider;
    public KeySound keySound;

    public Note( int _lane, double _time, double _sliderTime, KeySound _sound )
    {
        lane           = _lane;
        time           = _time;
        sliderTime     = _sliderTime;
        noteDistance   = 0d;
        sliderDistance = 0d;
        isSlider       = sliderTime > 0d ? true : false;
        keySound       = _sound;
    }
}

public struct KeySound
{
    public double time;
    public string name;
    public float  volume;

    public KeySound( double _time, string _name, float _volume )
    {
        time   = _time;
        name   = _name;
        volume = _volume < .1f ? 100f : _volume;
    }

    public KeySound( Note _note )
    {
        time   = _note.time;
        name   = _note.keySound.name;
        volume = _note.keySound.volume < .1f ? 100f : _note.keySound.volume;
    }
}

public enum SpriteType { None, Background, Foreground }
public struct SpriteSample
{
    public SpriteType type;
    public string name;
    public double start, end;

    public SpriteSample( string _name )
    {
        type  = SpriteType.Background;
        name  = _name;
        start = 0d;
        end   = 0d;
    }

    public SpriteSample( SpriteType _type, double _start, double _end, string _name )
    {
        type  = _type;
        name  = _name;
        start = _start;
        end   = _end;
    }
}

public struct Chart
{
    public ReadOnlyCollection<Timing>       timings;
    public ReadOnlyCollection<Note>         notes;
    public ReadOnlyCollection<KeySound>     samples;
    public ReadOnlyCollection<SpriteSample> sprites;
}

public class FileConverter : FileReader
{
    [Header( "Datas" )]
    private Song               song               = new Song();
    private List<Note>         notes              = new List<Note>();
    private List<Timing>       timings            = new List<Timing>();
    private List<KeySound>     samples            = new List<KeySound>();
    private List<SpriteSample> sprites            = new List<SpriteSample>();
    private List<Timing>       uninheritedTimings = new List<Timing>();

    [Header( "Write" )]
    private bool isAppended = false;
    private StringBuilder writeString = new StringBuilder();

    private readonly string[] virtualAudioName = { "preview.wav", "preview2.mp3", "preview.mp3", "preview.ogg" };
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
            px = _px;
            lane = -1;
        }
    }
    private class Accumulate
    {
        public double time;
        public double bpm;

        public Accumulate( double _time, double _bpm )
        {
            time = _time;
            bpm  = _bpm;
        }
    }

    protected void Convert( string _path )
    {
        try
        {
            song        = new Song(); 
            song.volume = 100;

            OpenFile( _path );

            #region General
            // [General] ~ [Editor]
            int mode = 0;
            while ( ReadLine() != "[Metadata]" )
            {
                if ( Contains( "AudioFilename:" ) ) song.audioName   = Split( ':' );
                if ( Contains( "PreviewTime:" ) )   song.previewTime = int.Parse( Split( ':' ) );
                if ( Contains( "Mode:" ) )          mode             = int.Parse( Split( ':' ) );
            }

            // 건반형 모드가 아니면 읽지 않음.
            if ( mode != 3 ) 
                 return;

            // [Metadata] ~ [Difficulty]
            while ( ReadLine() != "[Difficulty]" )
            {
                if ( Contains( "Title:"   ) && !Contains( "TitleUnicode:"  ) ) song.title   = Replace( "Title:",   string.Empty );
                if ( Contains( "Artist:"  ) && !Contains( "ArtistUnicode:" ) ) song.artist  = Replace( "Artist:",  string.Empty );
                if ( Contains( "Source:"  ) )                                  song.source  = Replace( "Source:",  string.Empty );
                if ( Contains( "Creator:" ) )                                  song.creator = Replace( "Creator:", string.Empty );
                if ( Contains( "Version:" ) )                                  song.version = Replace( "Version:", string.Empty );
            }

            // [Metadata] ~ [Difficulty]
            while ( ReadLine() != "[Events]" )
            {
                if ( Contains( "CircleSize:" ) ) song.keyCount = int.Parse( Split( ':' ) );
            }

            // 키음만으로 재생되는 노래는 프리뷰 음악이 대부분 없다.
            // preview.wav는 따로 프로그램을 통해 만들어놓은 파일이다.
            if ( song.audioName == null || song.audioName == string.Empty || song.audioName == "virtual" )
            {
                for ( int i = 0; i < virtualAudioName.Length; i++ )
                {
                    if ( File.Exists( Path.Combine( directory, virtualAudioName[i] ) ) )
                    {
                        song.audioName      = virtualAudioName[i];
                        song.isOnlyKeySound = true;

                        break;
                    }
                }
            }

            // [Events]
            samples?.Clear();
            sprites?.Clear();

            while ( ReadLine() != "[TimingPoints]" )
            {
                // Image
                if ( Contains( "0,0," ) && !( Contains( ".mp3" ) || Contains( ".wav" ) || Contains( ".ogg" ) || Contains( "Video," ) || Contains( "Sprite," ) ) )
                    song.imageName = Split( '"' );

                // Video
                if ( Contains( "Video," ) )
                {
                    var splitData = line.Split( ',' );
                    var name = splitData[2].Split( '"' )[1].Trim();
                    if ( Path.GetExtension( path ) != ".mpg" )
                    {
                        song.videoName   = name;
                        song.videoOffset = int.Parse( splitData[1] );
                        song.hasVideo    = File.Exists( Path.Combine( directory, name ) );
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
                if      ( _A.start > _B.start ) return 1;
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

                // 제거된 노트의 키음은 자동으로 재생되는 배경음으로 재생한다.
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
                    if ( sliderTime > 0d )
                    {
                        song.totalTime = song.totalTime >= sliderTime ? song.totalTime : ( int )sliderTime;
                        song.sliderCount++;

                        if ( finalLane == 3 )
                            song.delSliderCount++;
                    }
                    else
                    {
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
                            song.hasKeySound = true;
                            isCheckKeySoundOnce = true;
                        }
                    }
                }
            }

            // BMS2Osu로 뽑은 파일은 Pixel값 기준으로 정렬되어 있기 때문에 시간 순으로 다시 정렬해준다.
            samples.Sort( delegate ( KeySound _A, KeySound _B )
            {
                if      ( _A.time > _B.time ) return 1;
                else if ( _A.time < _B.time ) return -1;
                else                          return 0;
            } );
            notes.Sort( delegate ( Note _A, Note _B )
            {
                if      ( _A.time > _B.time ) return 1;
                else if ( _A.time < _B.time ) return -1;
                else                          return 0;
            } );

            #endregion
            song.mainBPM = GetMainBPM();

            Write();
            Dispose();
        }
        catch ( System.Exception _error )
        {
            Dispose();
            #if UNITY_EDITOR
            // 에러 위치 찾기
            System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace( _error, true );
            Debug.LogWarning( $"{trace.GetFrame( 0 ).GetFileLineNumber()} {_error.Message}  {Path.GetFileName( path )}" );
            #endif
        }
    }

    private void Write()
    {
        try
        {
            string fileName = $"{Path.GetFileNameWithoutExtension( path )}.wns";
            string filePath = @$"\\?\{Path.Combine( directory, fileName )}";

            using ( var stream = new FileStream( filePath, FileMode.Create ) )
            {
                using ( var writer = new StreamWriter( stream ) )
                {
                    writer.WriteLine( "[General]" );
                    writer.WriteLine( $"Title: {song.title}" );
                    writer.WriteLine( $"Artist: {song.artist}" );
                    writer.WriteLine( $"Source: {song.source}" );
                    writer.WriteLine( $"Creator: {song.creator}" );
                    writer.WriteLine( $"Version: {song.version}" );

                    writer.WriteLine( $"AudioOffset: {song.audioOffset}" );
                    writer.WriteLine( $"VideoOffset: {song.videoOffset}" );
                    writer.WriteLine( $"PreviewTime: {song.previewTime}" );
                    writer.WriteLine( $"Volume: {song.volume}" );

                    writer.WriteLine( $"ImageName: {song.imageName}" );
                    writer.WriteLine( $"AudioName: {song.audioName}" );
                    writer.WriteLine( $"VideoName: {song.videoName}" );

                    writer.WriteLine( $"TotalTime: {song.totalTime}" );
                    writer.WriteLine( $"Notes: {song.keyCount}:" +
                                             $"{song.noteCount}:" +
                                             $"{song.sliderCount}:" +
                                             $"{song.delNoteCount}:" +
                                             $"{song.delSliderCount}" );

                    writer.WriteLine( $"BPM: {song.minBpm}:" +
                                           $"{song.maxBpm}:" +
                                           $"{song.mainBPM}" );

                    writer.WriteLine( $"DataExist: {( song.isOnlyKeySound ? 1 : 0 )}:" +
                                                 $"{( song.hasKeySound    ? 1 : 0 )}:" +
                                                 $"{( song.hasVideo       ? 1 : 0 )}:" +
                                                 $"{( song.hasSprite      ? 1 : 0 )}" );

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

        // 원본 파일 삭제 ( 그냥 원본 놔둘지 고민 )
        // if ( File.Exists( path ) )
        // {
        //     Debug.Log( $"File Delete : {path}" );
        //     File.Delete( path );
        // }
    }

    private double GetMainBPM()
    {
        List<Accumulate> accumulate = new List<Accumulate>();
        // 상속되지않은 BPM으로 계산한다.
        for ( int i = 0; i < uninheritedTimings.Count; i++ )
        {
            // 끝나는 지점은 마지막 노트 시간으로 지정한다.
            double nextTime = ( i + 1 < uninheritedTimings.Count ) ? uninheritedTimings[i + 1].time : notes.Last().time;

            // 타이밍 지속시간 누적
            bool isFind = false;
            for ( int j = 0; j < accumulate.Count; j++ )
            {
                double diff = Math.Round( accumulate[j].bpm - uninheritedTimings[i].bpm );
                if ( Math.Abs( diff ) < double.Epsilon )
                {
                    isFind = true;
                    accumulate[j].time += nextTime - uninheritedTimings[i].time;
                    break;
                }
            }

            // 비교될 새로운 타이밍 추가
            if ( !isFind )
                accumulate.Add( new Accumulate( nextTime - uninheritedTimings[i].time, uninheritedTimings[i].bpm ) );
        }

        if ( accumulate.Count == 0 )
            throw new Exception( "The MainBPM was not found." );

        // 가장 오래 지속되는 BPM이 첫번째 요소가 되도록 내림차순 정렬
        accumulate.Sort( delegate ( Accumulate _left, Accumulate _right )
        {
            if      ( _left.time < _right.time ) return 1;
            else if ( _left.time > _right.time ) return -1;
            else                                 return 0;
        } );

        return accumulate[0].bpm;
    }

    public void ReWrite( in Song _song )
    {
        try
        {
            OpenFile( _song.filePath );

            while ( ReadLineEndOfStream() )
            {
                isAppended = false;

                if ( Contains( "AudioOffset:" ) ) AppendString( $"AudioOffset: {_song.audioOffset}" );
                if ( Contains( "VideoOffset:" ) ) AppendString( $"VideoOffset: {_song.videoOffset}" );
                if ( Contains( "PreviewTime:" ) ) AppendString( $"PreviewTime: {_song.previewTime}" );
                if ( Contains( "Volume:"      ) ) AppendString( $"Volume: {_song.volume}" );

                
                //if ( Contains( "ImagePath:" ) ) AppendString( $"ImageName: {Path.GetFileName( _song.imageName )}" );
                //if ( Contains( "AudioPath:" ) ) AppendString( $"AudioName: {Path.GetFileName( _song.audioName )}" );
                //if ( Contains( "VideoPath:" ) ) AppendString( $"VideoName: {Path.GetFileName( _song.videoName )}" );

                if ( !isAppended ) 
                      AppendString( line );
            }

            Dispose();

            string fileName = $"{Path.GetFileNameWithoutExtension( _song.filePath )}.wns";
            string filePath = @$"\\?\{Path.Combine( directory, fileName )}";
            using ( var stream = new FileStream( filePath, FileMode.Create ) )
            {
                using ( var writer = new StreamWriter( stream ) )
                {
                    writer.Write( writeString );
                }
            }
        }
        catch ( System.Exception _error )
        {
            Dispose();
            #if UNITY_EDITOR
            // 에러 위치 찾기
            System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace( _error, true );
            Debug.LogWarning( $"{trace.GetFrame( 0 ).GetFileLineNumber()} {_error.Message}  {Path.GetFileName( path )}" );
            #endif
        }
    }

    public void AppendString( in string _string )
    {
        writeString.Append( _string );
        writeString.Append( "\n" );
        isAppended = true;
    }
}
