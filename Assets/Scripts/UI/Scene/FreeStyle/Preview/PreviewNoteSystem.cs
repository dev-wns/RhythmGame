using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
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

    public  static double Distance { get; private set; }
    private static double DistanceCache;
    private double playback;
    private double startTime;
    private float noteStartPos;
    private float previewTime;
    private double soundOffset;

    private Coroutine cor;

    public static float NoteWidth  ;//=> GameSetting.NoteWidth * NoteMultiplier;
    public static float NoteHeight ;//=> GameSetting.NoteHeight * NoteMultiplier;
    private static float NoteMultiplier;

    private void Awake()
    {
        scroll.OnSelectSong += Parse;

        notePool ??= new ObjectPool<PreviewNoteRenderer>( notePrefab, transform, 5 );
    }

    private void Parse( Song _song )
    {
        notePool.AllDespawn();
        noteSpawnIndex = 0;
        timingIndex    = 0;
        Distance       = 0d;
        DistanceCache  = 0d;

        previewTime    = _song.previewTime * .001f;
        startTime      = Time.realtimeSinceStartupAsDouble;
        playback       = previewTime + ( Time.realtimeSinceStartupAsDouble - startTime );

        NoteMultiplier = _song.keyCount == 7 ? .55f  : 
                         _song.keyCount == 6 ? .625f : .775f;

        NoteWidth = GameSetting.NoteWidth * NoteMultiplier;
        NoteHeight= GameSetting.NoteHeight * NoteMultiplier;

        noteStartPos   = -( ( NoteWidth * ( _song.keyCount - 1 ) ) + ( GameSetting.NoteBlank * ( _song.keyCount + 1 ) ) ) * .5f;

        mainBPM = _song.mainBPM * GameSetting.CurrentPitch;
        using ( FileParser parser = new FileParser() )
        {
            if ( !parser.TryPreviewParse( _song.filePath, out chart ) )
                 Debug.LogWarning( $"Parsing failed  Current Chart : {_song.title}" );
        }

        for ( int i = 0; i < chart.notes.Count; i++ )
        {
            if ( chart.notes[i].time < playback )
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

        if ( cor is not null )
        {
            StopCoroutine( cor );
            cor = null;
        }
        cor = StartCoroutine( UpdateTime() );
    }

    private IEnumerator UpdateTime()
    {
        while ( true )
        {
            playback = previewTime + ( Time.realtimeSinceStartupAsDouble - startTime ) + ( NowPlaying.CurrentSong.audioOffset * .001f );
            
            var timings = chart.timings;
            for ( int i = timingIndex; i < timings.Count; i++ )
            {
                double time = timings[i].time;
                double bpm  = timings[i].bpm / mainBPM;

                if ( playback < time )
                     break;

                if ( i + 1 < timings.Count && timings[i + 1].time < playback )
                {
                    timingIndex++;
                    DistanceCache += bpm * ( timings[i + 1].time - time );
                    Distance = DistanceCache;
                    continue;
                }

                Distance = DistanceCache + ( bpm * ( playback - time ) );
            }

            SpawnNotes( Distance );
            yield return null;
        }
    }
    
    private void SpawnNotes( double _distance )
    {
        while ( noteSpawnIndex < chart.notes.Count && curData.noteDistance <= _distance + GameSetting.MinDistance )
        {
            PreviewNoteRenderer note = notePool.Spawn();
            Color color = Color.white;
            if ( NowPlaying.KeyCount == 4 )
            {
                color = curData.lane == 1 || curData.lane == 2 ? new Color( 0.2078432f, 0.7843138f, 1f, 1f ) : Color.white;
            }
            else if ( NowPlaying.KeyCount == 6 )
            {
                color = curData.lane == 1 || curData.lane == 4 ? new Color( 0.2078432f, 0.7843138f, 1f, 1f ) : Color.white;
            }
            else if ( NowPlaying.KeyCount == 7 )
            {
                color = curData.lane == 1 || curData.lane == 5 ? new Color( 0.2078432f, 0.7843138f, 1f, 1f ) :
                                             curData.lane == 3 ? new Color( 1f, 0.8274511f, 0.2117647f, 1f ) : Color.white;
            }

            note.SetInfo( curData, noteStartPos, color );

            if ( ++noteSpawnIndex < chart.notes.Count )
            {
                curData = chart.notes[noteSpawnIndex];
                curData.noteDistance   = GetDistance( curData.time );
                if( curData.isSlider )
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
