using TMPro;
using UnityEngine;

public class PreviewNoteSystem : MonoBehaviour
{
    private ObjectPool<PreviewNoteRenderer> notePool;
    public PreviewNoteRenderer notePrefab;

    private int noteSpawnIndex;
    private Note curData;

    public FreeStyleMainScroll scroll;
    private Chart chart;

    private int timingIndex;
    private double mainBPM;

    private const double DelayTime = 1d / 60d;

    public TextMeshProUGUI bpmText;
    private int bpmIndex;
    private double bpmTime;
    private Timing curTiming;

    public SoundPitchOption pitchOption;

    public static double Playback => FreeStyleMainScroll.Playback + ( NowPlaying.CurrentSong.audioOffset * .001f ) - .05f;
    public static double Distance { get; private set; }
    private static double DistanceCache;
    private float noteStartPos;
    private double soundOffset;

    public static float NoteWidth;
    public static float NoteHeight;
    public static float Weight => ( GameSetting.ScrollSpeed * .775f ) * 350f;
    public static float MinDistance => 1200f / Weight;
    private static float NoteMultiplier;

    private void Awake()
    {
        scroll.OnSelectSong += Parse;
        scroll.OnSoundRestart += Restart;
        pitchOption.OnPitchUpdate += OnPitchUpdate;

        notePool ??= new ObjectPool<PreviewNoteRenderer>( notePrefab, transform, 5 );
    }

    private void Restart( Song _song )
    {
        notePool.AllDespawn();
        noteSpawnIndex = 0;
        timingIndex = 0;
        Distance = 0d;
        DistanceCache = 0d;

        for ( int i = 0; i < chart.notes.Count; i++ )
        {
            if ( chart.notes[i].time < Playback )
            {
                noteSpawnIndex++;
            }
            else
            {
                curData = chart.notes[noteSpawnIndex];
                curData.noteDistance = GetDistance( curData.time );
                if ( curData.isSlider )
                    curData.sliderDistance = GetDistance( curData.sliderTime );

                break;
            }
        }

        var timings = chart.timings;
        for ( int i = 0; i + 1 < timings.Count; i++ )
        {
            double time = timings[i].time;
            double bpm  = timings[i].bpm / mainBPM;

            if ( timings[i + 1].time < Playback )
            {
                timingIndex++;
                DistanceCache += ( bpm * ( timings[i + 1].time - time ) );
            }
            else
            {
                timingIndex = i;
                break;
            }
        }

        bpmIndex = timingIndex;
        curTiming = timings[bpmIndex];
        bpmTime = curTiming.time;
        bpmText.text = $"{Mathf.RoundToInt( ( float )( curTiming.bpm * GameSetting.CurrentPitch ) )}";
    }

    private void Parse( Song _song )
    {
        NoteMultiplier = _song.keyCount == 7 ? .55f :
                         _song.keyCount == 6 ? .625f : .775f;

        NoteWidth = 110.5f * NoteMultiplier;
        NoteHeight = 63f * NoteMultiplier;

        noteStartPos = -( ( NoteWidth * ( _song.keyCount - 1 ) ) + ( GameSetting.NoteBlank * ( _song.keyCount + 1 ) ) ) * .5f;

        mainBPM = _song.mainBPM;
        using ( FileParser parser = new FileParser() )
        {
            if ( !parser.TryPreviewParse( _song.filePath, out chart ) )
                Debug.LogWarning( $"Parsing failed  Current Chart : {_song.title}" );
        }

        Restart( _song );
    }

    private void OnPitchUpdate( float _pitch )
    {
        bpmText.text = $"{Mathf.RoundToInt( ( float )( curTiming.bpm * _pitch ) )}";
    }

    private void Update()
    {
        var timings = chart.timings;
        for ( int i = timingIndex; i < timings.Count; i++ )
        {
            double time = timings[i].time;
            double bpm  = timings[i].bpm / mainBPM;

            if ( Playback < time )
                break;

            if ( i + 1 < timings.Count && timings[i + 1].time < Playback )
            {
                timingIndex++;
                DistanceCache += ( bpm * ( timings[i + 1].time - time ) );
                Distance = DistanceCache;
                break;
            }

            Distance = DistanceCache + ( bpm * ( Playback - time ) );
        }

        if ( bpmIndex < timings.Count && bpmTime < Playback )
        {
            bpmText.text = $"{Mathf.RoundToInt( ( float )( timings[bpmIndex].bpm * GameSetting.CurrentPitch ) )}";

            if ( ++bpmIndex < timings.Count )
            {
                curTiming = timings[bpmIndex];
                bpmTime = bpmIndex + 1 < timings.Count && Global.Math.Abs( timings[bpmIndex + 1].time - curTiming.time ) > DelayTime ?
                          curTiming.time + DelayTime : curTiming.time;
            }
        }

        SpawnNotes( Distance );
    }

    private void SpawnNotes( double _distance )
    {
        while ( noteSpawnIndex < chart.notes.Count && curData.noteDistance <= _distance + MinDistance )
        {
            PreviewNoteRenderer note = notePool.Spawn();
            Color color = Color.white;
            if ( NowPlaying.OriginKeyCount == 4 )
            {
                color = curData.lane == 1 || curData.lane == 2 ? new Color( 0.2078432f, 0.7843138f, 1f, 1f ) : Color.white;
            }
            else if ( NowPlaying.OriginKeyCount == 6 )
            {
                color = curData.lane == 1 || curData.lane == 4 ? new Color( 0.2078432f, 0.7843138f, 1f, 1f ) : Color.white;
            }
            else if ( NowPlaying.OriginKeyCount == 7 )
            {
                color = curData.lane == 1 || curData.lane == 5 ? new Color( 0.2078432f, 0.7843138f, 1f, 1f ) :
                                             curData.lane == 3 ? new Color( 1f, 0.8274511f, 0.2117647f, 1f ) : Color.white;
            }

            note.SetInfo( curData, noteStartPos, color );
            if ( ++noteSpawnIndex < chart.notes.Count )
            {
                curData = chart.notes[noteSpawnIndex];
                curData.noteDistance = GetDistance( curData.time );
                if ( curData.isSlider )
                    curData.sliderDistance = GetDistance( curData.sliderTime );
            }
        }
    }

    public double GetDistance( double _time )
    {
        double result = 0d;
        var timings = chart.timings;
        for ( int i = 0; i < timings.Count; i++ )
        {
            double time = timings[i].time;
            double bpm  = timings[i].bpm / mainBPM;

            // 구간별 타이밍에 대한 거리 추가
            if ( i + 1 < timings.Count && timings[i + 1].time < _time )
            {
                result += bpm * ( timings[i + 1].time - time );
                continue;
            }

            // 마지막 타이밍에 대한 거리 추가
            result += bpm * ( _time - time );
            break;
        }
        return result;
    }
}
