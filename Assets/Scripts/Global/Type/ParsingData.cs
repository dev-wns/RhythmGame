
public enum BGA : byte { None, Video, Sprite, Image, }
public enum SpriteType { None, Background, Foreground, }

public delegate void SongHandler( in Song _song );
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
    public float  volume;

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
        time          = _time;
        bpm           = _bpm;
        isUninherited = 0;
        beatLength    = 0d;
    }

    public Timing( double _time, double _bpm, double _beatLength, int _isUninherited )
    {
        time          = _time;
        bpm           = _bpm;
        beatLength    = _beatLength;
        isUninherited = _isUninherited;
    }
}

public struct Note
{
    public int      lane;
    public double   time;
    public double   endTime;
    public double   distance;
    public double   endDistance;
    public bool     isSlider;
    public KeySound keySound;

    public Note( int _lane, double _time, double _endTime, KeySound _sound )
    {
        lane        = _lane;
        time        = _time;
        endTime     = _endTime;
        distance    = 0d;
        endDistance = 0d;
        isSlider    = endTime > 0d;
        keySound    = _sound;
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
        volume = _volume < float.Epsilon ? 100f : _volume;
    }

    public KeySound( string _name )
    {
        time   = 0d;
        name   = _name;
        volume = 100f;
    }
}

public struct SpriteSample
{
    public SpriteType type;
    public string name;
    public double start;
    public double end;

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