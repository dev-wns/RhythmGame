using System.Collections;
using System.Collections.Generic;

public class Song
{
    #region structures
    public struct Notes
    {
        public int x, y;        // position
        public int timing;      // hit timing
        public int type;        // 1 = normal note, 128 = long note
        public int length_LN;   // long note length
        public string hitSound; // hit sound name

        public Notes( int _x, int _y, int _timing, int _type, int _length_LN, string _hitSound )
        {
            x = _x;
            y = _y;
            timing = _timing;
            type = _type;
            length_LN = _length_LN;
            hitSound = _hitSound;
        }
    }

    public struct Timings
    {
        public float time;        // bpm changes time
        public float bpm;         // 1 / beat length * 1000 ( ms ) * 60 ( minute )
        private float beatLength;

        public Timings( float _time, float _beatLength )
        {
            time = _time;
            beatLength = _beatLength;
            bpm = 1f / beatLength * 1000f * 60f;
        }
    }

    public struct PreviewData
    {
        public int time; // preview start time
        public string name; // sound name
        public string img;
        public string title;
        public string artist;
        public string path;

        public PreviewData( string _name, string _img, string _title, string _artist, int _time, string _path )
        {
            name = _name;
            img = _img;
            title = _title;
            artist = _artist;
            time = _time;
            path = _path;
        }
    }
    #endregion

    public List<Notes> notes = new List<Notes>();
    public List<Timings> timings = new List<Timings>();
    public PreviewData preview;
}
