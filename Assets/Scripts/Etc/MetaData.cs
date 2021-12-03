using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Timings
{
    public float changeTime;
    public float bpm;

    public Timings( float _changeTime, float _bpm )
    {
        changeTime = _changeTime;
        bpm = _bpm;
    }
}

public struct Notes
{
    public int line;
    public float hitTiming;
    public int type;
    public int lengthLN;
    public Notes( int _x, float _hitTiming, int _type, int _lengthLN )
    {
        line = Mathf.FloorToInt( _x * 6f / 512f );
        hitTiming = _hitTiming;
        type = _type;
        lengthLN = _lengthLN;
    }
}

public class MetaData
{ 
    public string title, artist, creator, version;
    public string audioName, imgName;
    public string audioPath, imgPath;
    public int previewTime;

    public List<Notes> notes = new List<Notes>();
    public List<Timings> timings = new List<Timings>();

    public Sprite background;
    //public AudioClip clip;
    //public FMOD.Sound sound;
}
