using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NoteType { None, Default, Slider }

public class NoteSystem : MonoBehaviour
{
    public InGame CurrentScene { get; private set; }
    private Lane lane;

    private ObjectPool<NoteRenderer> nPool;
    public NoteRenderer note1 /* Lane 0,2,3,5 */, note2 /* Lane 1,4 */, noteMedian;

    private List<Note> notes = new List<Note>();
    private Note curNote;
    private int curIndex;

    private void Awake()
    {
        lane = GetComponent<Lane>();
        lane.OnLaneInitialize += ( int _key ) =>
        {
            NoteRenderer skin = note1;
            if ( NowPlaying.CurrentSong.keyCount == 4 )
            {
                skin = _key == 1 || _key == 2 ? note2 : note1;
            }
            else if ( NowPlaying.CurrentSong.keyCount == 6 )
            {
                skin = _key == 1 || _key == 4 ? note2 : note1;
            }
            else if ( NowPlaying.CurrentSong.keyCount == 7 )
            {
                skin = _key == 1 || _key == 5 ? note2 : 
                                    _key == 3 ? noteMedian : note1;
            }
            nPool ??= new ObjectPool<NoteRenderer>( skin, 5 );
        };

        CurrentScene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        CurrentScene.OnGameStart    += () => StartCoroutine( Process() );
        CurrentScene.OnReLoad       += ReLoad;
    }

    private void ReLoad()
    {
        StopAllCoroutines();
        curIndex = 0;
        curNote = notes[curIndex];
        lane.InputSys.SetSound( curNote.keySound );
    }

    public void AddNote( in Note _note ) => notes.Add( _note );

    public void Despawn( NoteRenderer _note ) => nPool.Despawn( _note );

    private IEnumerator Process()
    {
        if ( notes.Count > 0 )
        {
            curNote = notes[curIndex];
            lane.InputSys.SetSound( curNote.keySound );
        }

        WaitUntil waitNextNote = new WaitUntil( () => curNote.calcTime <= NowPlaying.PlaybackInBPM + GameSetting.PreLoadTime );
        while ( curIndex < notes.Count )
        {
            yield return waitNextNote;
         
            NoteRenderer note = nPool.Spawn();
            note.SetInfo( lane.Key, this, in curNote );

            lane.InputSys.AddNote( note );

            if ( ++curIndex < notes.Count )
                 curNote = notes[curIndex];
        }
    }
}
