using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using TMPro;
using UnityEngine;

#pragma warning disable CS0162

public class PreviewNoteSystem : MonoBehaviour
{
    [SerializeField] private FreeStyle          scene;
    [SerializeField] private SoundPitchOption   pitchOption;
    [SerializeField] private PreviewNoteModelSO noteModelSO;
    [SerializeField] private NoteModelOption    noteModelOption;

    private Dictionary<NoteModel, ObjectPool<PreviewNoteRenderer>> pools = new();
    private ObjectPool<PreviewNoteRenderer> currentPool;

    private static Song song = new ();
    private Note  curData;
    private int   spawnIndex;
    private float startNotePos;
    private int startNoteIndex;

    private List<Note>   notes   = new ();
    private List<Timing> timings = new ();
    private int timingIndex;
    private int startTimingIndex;

    private const double DelayTime = ( 1d / 60d ) * 1000d;

    [SerializeField] TextMeshProUGUI bpmText;
    private int    bpmIndex;
    private double bpmTime;
    private double prevBPM;
    private Timing curTiming;

    public  static int Playback => FreeStyle.Playback + song.audioOffset;
    public  static double Distance { get; private set; }
    private static double DistanceCache;
    private static double StartDistanceCache;

    public  static float NoteWidth;
    public  static float NoteHeight;
    private static float NoteMultiplier;

    private void Awake()
    {
        scene.OnLoadAsync += Parse;
        scene.OnStart     += Restart;
        scene.OnRestart   += Restart;
        pitchOption.OnUpdatePitch += UpdatePitch;
        noteModelOption.OnChangedNoteModel += ChangedNoteModel;

        ChangedNoteModel( GameSetting.CurrentNoteModel );
    }

    private void OnDestroy()
    {
        scene.OnLoadAsync -= Parse;
        scene.OnStart     -= Restart;
        scene.OnRestart   -= Restart;
        pitchOption.OnUpdatePitch          -= UpdatePitch;
        noteModelOption.OnChangedNoteModel -= ChangedNoteModel;
    }

    private void ChangedNoteModel( NoteModel _type )
    {
        if ( !pools.TryGetValue( _type, out currentPool ) )
        {
            pools.Add( _type, new ObjectPool<PreviewNoteRenderer>( noteModelSO.GetModel( _type ), transform, 5 ) );
            currentPool = pools[_type];
        }
    }

    private async UniTask Parse( Song _song, CancellationToken _token )
    {
        song = _song;
        NoteMultiplier = song.keyCount == 7 ? .55f :
                         song.keyCount == 6 ? .625f : .775f;

        NoteWidth  = 110.5f * NoteMultiplier;
        NoteHeight = 63f    * NoteMultiplier;

        startNotePos = -( ( NoteWidth * ( song.keyCount - 1 ) ) + ( GameSetting.NoteBlank * ( song.keyCount + 1 ) ) ) * .5f;

        timings.Clear();
        timings.AddRange( DataStorage.Inst.Timings );

        notes.Clear();
        notes.AddRange( DataStorage.Inst.Notes );

        double previewTime = song.previewTime + song.audioOffset - 50;
        for ( startNoteIndex = 0; startNoteIndex < notes.Count; startNoteIndex++ )
        {
            Note note = notes[startNoteIndex];
            if ( note.time > previewTime || ( note.time < previewTime && note.endTime > previewTime ) )
            {   
                spawnIndex = startNoteIndex;
                break;
            }
        }

        StartDistanceCache = 0d;
        for ( int i = 0; i < timings.Count; i++ )
        {
            double time = timings[i].time;
            double bpm  = timings[i].bpm / song.mainBPM;

            if ( i + 1 < timings.Count && timings[i + 1].time < previewTime )
            {
                StartDistanceCache += bpm * ( timings[i + 1].time - time );
                continue;
            }

            startTimingIndex = i;
            break;
        }

        await UniTask.CompletedTask;
    }

    private void Restart( in Song _song )
    {
        currentPool.AllDespawn();
        
        spawnIndex     = startNoteIndex;

        timingIndex    = startTimingIndex;
        Distance       = StartDistanceCache;
        DistanceCache  = StartDistanceCache;

        bpmIndex       = timingIndex;
        curTiming      = timings[timingIndex];
        bpmTime        = curTiming.time;
        bpmText.SetText( "{0}", Mathf.RoundToInt( ( float )( curTiming.bpm * GameSetting.SoundPitch ) ) );

        curData = notes[spawnIndex];
        curData.distance = GetDistance( curData.time );
        if ( curData.isSlider )
             curData.endDistance = GetDistance( curData.endTime );
    }

    private void UpdatePitch( float _pitch )
    {
        bpmText.SetText( "{0}", Mathf.RoundToInt( ( float )( curTiming.bpm * _pitch ) ) );
    }

    private void Update()
    {
        for ( int i = timingIndex; i < timings.Count; i++ )
        {
            double time = timings[i].time;
            double bpm  = timings[i].bpm / song.mainBPM;

            if ( i + 1 < timings.Count && timings[i + 1].time < Playback )
            {
                timingIndex   += 1;
                DistanceCache += bpm * ( timings[i + 1].time - time );
                continue;
                //break;
            }

            Distance = DistanceCache + ( bpm * ( Playback - time ) );
            break;
        }

        SpawnNotes( Distance );

        if ( bpmIndex < timings.Count && bpmTime < Playback )
        {
            Timing current = timings[bpmIndex];
            if ( prevBPM != current.bpm )
                 bpmText.SetText( "{0}", Mathf.RoundToInt( ( float )( current.bpm * GameSetting.SoundPitch ) ) );
            
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
    }

    private void SpawnNotes( double _distance )
    {
        while ( spawnIndex < notes.Count && curData.distance <= _distance + GameSetting.MinDistance )
        {
            PreviewNoteRenderer note = currentPool.Spawn();
            
            Color color  = Color.white;
            int keyCount = Browser.Inst.SelectedSong.keyCount;
            if      ( keyCount == 4 ) color = curData.lane == 1 || curData.lane == 2 ? new Color( 0.2078432f, 0.7843138f, 1f, 1f ) : Color.white;
            else if ( keyCount == 6 ) color = curData.lane == 1 || curData.lane == 4 ? new Color( 0.2078432f, 0.7843138f, 1f, 1f ) : Color.white;
            else if ( keyCount == 7 ) color = curData.lane == 1 || curData.lane == 5 ? new Color( 0.2078432f, 0.7843138f, 1f, 1f ) :
                                              curData.lane == 3                      ? new Color( 1f, 0.8274511f, 0.2117647f, 1f ) : Color.white;

            note.SetInfo( curData, startNotePos, color );
            if ( ++spawnIndex < notes.Count )
            {
                curData = notes[spawnIndex];
                curData.distance = GetDistance( curData.time );
                if ( curData.isSlider )
                     curData.endDistance = GetDistance( curData.endTime );
            }
        }
    }

    public double GetDistance( double _time )
    {
        double result = 0d;
        for ( int i = 0; i < timings.Count; i++ )
        {
            double time = timings[i].time;
            double bpm  = timings[i].bpm / song.mainBPM;

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
