using System.Collections.Generic;
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
    public int sliderCount;
    public int timingCount;
    public int minBpm;
    public int maxBpm;
    public int keyCount;
}
public class Timing
{
    public float time;
    public float bpm;

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

    public Note( int _x, float _time, float _calcTime, float _sliderTime, float _calcSliderTime, bool _isSlider )
    {
        line = Mathf.FloorToInt( _x * 6f / 512f );
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
    public List<Note>   notes;
    public float medianBpm;
}

public abstract class Parser : FileReader
{
    public bool IsComplete { get; protected set; } = true;
    protected Song song;
    protected Chart chart;

    protected Parser( string _path ) : base( _path ) { song.filePath = path; }

    public abstract Song PreRead();

    public abstract Chart PostRead( Song _song );
    
    protected float GetMedianBpm( List<Timing> timings )
    {
        List<Timing> medianCalc = new List<Timing>();
        medianCalc.Add( new Timing( 0f, timings[0].bpm ) );
        for ( int i = 1; i < timings.Count; i++ )
        {
            float prevTime = timings[i - 1].time;
            float prevBpm = timings[i - 1].bpm;

            for ( int j = 0; j < medianCalc.Count; j++ )
            {
                if ( Mathf.Abs( medianCalc[j].bpm - prevBpm ) < .1f )
                {
                    medianCalc[j].time += timings[i].time - prevTime;
                    break;
                }

                if ( medianCalc.Count - 1 == j && timings[i].bpm >= 30f )
                {
                    medianCalc.Add( new Timing( timings[i].time - prevTime, timings[i].bpm ) );
                    break;
                }
            }
        }

        medianCalc.Sort( delegate ( Timing A, Timing B )
        {
            if ( A.time >= B.time ) return -1;
            else return 1;
        } );

        //return 1f / medianCalc[0].bpm * 60000f;
        return medianCalc[0].bpm;
    }
}