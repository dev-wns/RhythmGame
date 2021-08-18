using System.Collections;
using System.Collections.Generic;

// Sound Data
public class Sound
{
    #region Structures
    public struct Note
    {
        public int x, y;        // position
        public int timing;      // hit timing
        public int type;        // 1 = normal note, 128 = long note
        public int length_LN;   // long note length
        public string hitSound; // hit sound name

        public Note( int _x, int _y, int _timing, int _type, int _length_LN, string _hitSound )
        {
            x = _x;
            y = _y;
            timing = _timing;
            type = _type;
            length_LN = _length_LN;
            hitSound = _hitSound;
        }
    }

    public struct Timing
    {
        public int time;          // time when bpm changes
        public float bpm;         // 1 / beat length * 1000 ( ms ) * 60 ( minute )
        private float beatLength;

        public Timing( int _time, float _beatLength )
        {
            time = _time;
            beatLength = _beatLength;
            bpm = 1f / beatLength * 1000f * 60f;
        }
    }

    public struct Preview
    {
        public int time; // preview start time
        public string audio;
        public string img;
        public string title;
        public string artist;

        public Preview( string _audio, string _img, string _title, string _artist, int _time )
        {
            audio = _audio;
            img = _img;
            title = _title;
            artist = _artist;
            time = _time;
        }
    }
    #endregion

    public List<Note> notes;
    public List<Timing> timings;
    public Preview preview;
}
