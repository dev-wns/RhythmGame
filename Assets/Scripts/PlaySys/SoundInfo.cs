using System.Collections;
using System.Collections.Generic;

// Sound Data
public class SoundInfo
{
    #region Structures
    public struct NoteInfo
    {
        public int x, y;        // position
        public int timing;      // hit timing
        public int type;        // 1 = normal note, 128 = long note
        public int length_LN;   // long note length
        public string hitSound; // hit sound name

        public NoteInfo( int _x, int _y, int _timing, int _type, int _length_LN, string _hitSound )
        {
            x = _x;
            y = _y;
            timing = _timing;
            type = _type;
            length_LN = _length_LN;
            hitSound = _hitSound;
        }
    }

    public struct TimingInfo
    {
        public float time;        // bpm changes time
        public float bpm;         // 1 / beat length * 1000 ( ms ) * 60 ( minute )
        private float beatLength;

        public TimingInfo( float _time, float _beatLength )
        {
            time = _time;
            beatLength = _beatLength;
            bpm = 1f / beatLength * 1000f * 60f;
        }
    }

    public struct PreviewInfo
    {
        public int time; // preview start time
        public string audio;
        public string img;
        public string title;
        public string artist;

        public PreviewInfo( string _audio, string _img, string _title, string _artist, int _time )
        {
            audio = _audio;
            img = _img;
            title = _title;
            artist = _artist;
            time = _time;
        }
    }
    #endregion

    public List<NoteInfo> notes     = new List<NoteInfo>();
    public List<TimingInfo> timings = new List<TimingInfo>();
    public PreviewInfo preview;
}
