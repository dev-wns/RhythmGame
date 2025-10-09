using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

#pragma warning disable CS0162

public class PreviewNoteSystem : MonoBehaviour
{
    private ObjectPool<PreviewNoteRenderer> notePool;
    public PreviewNoteRenderer notePrefab;

    private int spawnIndex;
    private Note curData;

    public FreeStyleMainScroll scroll;
    private Chart chart;

    private int timingIndex;
    private double mainBPM;

    private const double DelayTime = ( 1d / 60d ) * 1000d;

    public TextMeshProUGUI bpmText;
    private int bpmIndex;
    private double bpmTime;
    private double prevBPM;
    private Timing curTiming;
    public static Timing FirstTiming;

    public static double Playback => FreeStyleMainScroll.Playback + NowPlaying.CurrentSong.audioOffset + GameSetting.SoundOffset;
    public static double Distance { get; private set; }
    private static double DistanceCache;
    private float noteStartPos;

    public static float NoteWidth;
    public static float NoteHeight;
    public static float Weight => GameSetting.Weight * .775f;
    public static float MinDistance => 1200f / Weight;
    private static float NoteMultiplier;

    private void Awake()
    {
        scroll.OnSelectSong += Parse;
        scroll.OnSoundRestart += Restart;
        AudioManager.OnUpdatePitch += UpdatePitch;

        notePool ??= new ObjectPool<PreviewNoteRenderer>( notePrefab, transform, 5 );
    }

    private void OnDestroy()
    {
        AudioManager.OnUpdatePitch -= UpdatePitch;
    }

    private void Restart( Song _song )
    {
        notePool.AllDespawn();
        spawnIndex     = 0;
        timingIndex    = 0;
        Distance       = 0d;
        DistanceCache  = 0d;

        var notes = chart.notes;
        for ( spawnIndex = 0; spawnIndex < notes.Count; spawnIndex++ )
        {
            if ( notes[spawnIndex].time > Playback )
            {
                curData = notes[spawnIndex];
                curData.distance = GetDistance( curData.time );
                if ( curData.isSlider )
                     curData.endDistance = GetDistance( curData.endTime );

                break;
            }
        }

        var timings = chart.timings;
        for ( timingIndex = 0; timingIndex + 1 < timings.Count; timingIndex++ )
        {
            double time = timings[timingIndex].time;
            double bpm  = timings[timingIndex].bpm / mainBPM;

            if ( timings[timingIndex + 1].time > Playback )
                 break;

            DistanceCache += bpm * ( timings[timingIndex + 1].time - time );
        }

        bpmIndex     = timingIndex;
        curTiming    = timings[bpmIndex];
        bpmTime      = curTiming.time;
        bpmText.text = $"{Mathf.RoundToInt( ( float )( curTiming.bpm * GameSetting.CurrentPitch ) )}";
    }

    private void Parse( Song _song )
    {
        NoteMultiplier = _song.keyCount == 7 ? .55f :
                         _song.keyCount == 6 ? .625f : .775f;

        NoteWidth  = 110.5f * NoteMultiplier;
        NoteHeight = 63f * NoteMultiplier;

        noteStartPos = -( ( NoteWidth * ( _song.keyCount - 1 ) ) + ( GameSetting.NoteBlank * ( _song.keyCount + 1 ) ) ) * .5f;

        mainBPM = _song.mainBPM;
        using ( FileParser parser = new FileParser() )
        {
            if ( !parser.TryPreviewParse( _song.filePath, out chart ) )
                 Debug.LogWarning( $"Parsing failed  Current Chart : {_song.title}" );

            FirstTiming = chart.timings[0];
        }

        Restart( _song );
    }

    private void UpdatePitch( float _pitch )
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

            if ( i + 1 < timings.Count && timings[i + 1].time < Playback )
            {
                timingIndex += 1;
                DistanceCache += bpm * ( timings[i + 1].time - time );
                break;
            }

            Distance = DistanceCache + ( bpm * ( Playback - time ) );
            break; 
        }


        if ( bpmIndex < timings.Count && bpmTime < Playback )
        {
            Timing current = timings[bpmIndex];
            if ( prevBPM != current.bpm )
                 bpmText.text = $"{Mathf.RoundToInt( ( float ) ( current.bpm * GameSetting.CurrentPitch ) )}";
            
            prevBPM = current.bpm;

            // ´ÙÀ½ BPM
            if ( bpmIndex + 1 < timings.Count )
            {
                current = timings[++bpmIndex];
                bool needDelay = false;
                if ( bpmIndex + 1 < timings.Count )
                { 
                    Timing next = timings[bpmIndex + 1];
                    if ( Global.Math.Abs( next.time - current.time ) > DelayTime )
                         needDelay = true;
                }

                bpmTime = needDelay ? current.time + DelayTime : current.time;
            }
        }

        SpawnNotes( Distance );
    }

    private void SpawnNotes( double _distance )
    {
        while ( spawnIndex < chart.notes.Count && curData.distance <= _distance + MinDistance )
        {
            PreviewNoteRenderer note = notePool.Spawn();
            
            Color color  = Color.white;
            int keyCount = NowPlaying.CurrentSong.keyCount;
            if      ( keyCount == 4 ) color = curData.lane == 1 || curData.lane == 2 ? new Color( 0.2078432f, 0.7843138f, 1f, 1f ) : Color.white;
            else if ( keyCount == 6 ) color = curData.lane == 1 || curData.lane == 4 ? new Color( 0.2078432f, 0.7843138f, 1f, 1f ) : Color.white;
            else if ( keyCount == 7 ) color = curData.lane == 1 || curData.lane == 5 ? new Color( 0.2078432f, 0.7843138f, 1f, 1f ) :
                                              curData.lane == 3                      ? new Color( 1f, 0.8274511f, 0.2117647f, 1f ) : Color.white;

            note.SetInfo( curData, noteStartPos, color );
            if ( ++spawnIndex < chart.notes.Count )
            {
                curData = chart.notes[spawnIndex];
                curData.distance = GetDistance( curData.time );
                if ( curData.isSlider )
                     curData.endDistance = GetDistance( curData.endTime );
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

            if ( i + 1 < timings.Count && timings[i + 1].time < _time )
            {
                result += bpm * ( timings[i + 1].time - time );
                continue;
            }

            result += bpm * ( _time - time );
            break;
        }
        return result;
    }
}
