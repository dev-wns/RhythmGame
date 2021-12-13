using System.Collections;
using UnityEngine;
using System.IO;


public enum ParseType { Osu, Bms };
public struct Song
{
    public ParseType type;
    public string filePath;
    public string audioPath;
    public string imagePath;
    public string videoPath;
    public bool hasVideo;

    public string title;
    public string artist;
    public string creator;
    public string version;

    public int previewTime;
    public int totalTime;

    public int noteCount;
    public int longNoteCount;
    public int minBpm;
    public int maxBpm;
    public int keyCount;
}

public struct Chart
{

}

public abstract class Parser : FileReader
{
    public bool IsComplete { get; protected set; } = true;
    protected Song song;
    protected Chart chart;

    protected Parser( string _path ) : base( _path ) { }

    public abstract Song PreRead();

    public abstract Chart PostRead();
}