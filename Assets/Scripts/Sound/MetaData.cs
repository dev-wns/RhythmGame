using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetaData
{ 
    public struct Timings
    {
        public float changeTime;
        public float beatLength;
        public double bpm;
        public bool isUninherited;

        public Timings ( float _changeTime, float _beatLength, bool _isUninherited )
        {
            changeTime = _changeTime;
            beatLength = _beatLength;
            bpm = 1f / _beatLength * 1000f * 60f;
            isUninherited = _isUninherited;
        }
    }

    public struct Notes
    {
        public int x, y;
        public uint hitTiming;
        public int type;
        public int lengthLN;
        public Notes( int _x, int _y, uint _hitTiming, int _type, int _lengthLN )
        {
            x = _x;
            y = _y;
            hitTiming = _hitTiming;
            type = _type;
            lengthLN = _lengthLN;
        }
    }

    public string title, artist, creator, version;
    public string audioName, imgName;
    public string audioPath, imgPath;
    public int previewTime;

    public List<Notes> notes = new List<Notes>();
    public List<Timings> timings = new List<Timings>();
}
