public struct Song
{
    public string AudioPath { get; set; }
    public string ImagePath { get; set; }
    public string VideoPath { get; set; }
    public bool HasVideo { get; set; }

    public string Title { get; set; }
    public string Artist { get; set; }
    public string Creator { get; set; }
    public string Version { get; set; }

    public int PreviewTime { get; set; }

    public Song( Song _song )
    {
        AudioPath = _song.AudioPath;
        ImagePath = _song.ImagePath;
        VideoPath = _song.VideoPath;
        HasVideo = _song.HasVideo;
        Title = _song.Title;
        Artist = _song.Artist;
        Creator = _song.Creator;
        Version = _song.Version;
        PreviewTime = _song.PreviewTime;
    }
}

public struct Chart
{
    public int NoteCount { get; set; }
    public int LongNoteCount { get; set; }
    public int MinBPM { get; set; }
    public int MaxBPM { get; set; }
    public int KeyCount { get; set; }
}

public abstract class Parser : FileReader
{
    protected Song song;
    protected Chart chart;

    protected Parser( string _path ) : base( _path ) { }

    public abstract Song PreRead();
    public abstract Chart PostRead();
}