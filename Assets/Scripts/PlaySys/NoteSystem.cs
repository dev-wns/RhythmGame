using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSystem : MonoBehaviour
{
    public InGame CurrentScene { get; private set; }
    public Lane lane;

    private ObjectPool<NoteRenderer> nPool;
    public NoteRenderer nPrefab;

    private List<Note> notes = new List<Note>();
    private int currentIndex;

    private void Awake()
    {
        CurrentScene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        nPool = new ObjectPool<NoteRenderer>( nPrefab, 10, false );

        CurrentScene.OnGameStart += () => StartCoroutine( Process() );
    }

    public void AddNote( Note _note ) => notes.Add( _note );

    public void Despawn( NoteRenderer _note ) => nPool.Despawn( _note );
    private IEnumerator Process()
    {
        float slidertime = 0f;
        float notetime = 0f;
        while ( currentIndex < notes.Count )
        {
            Note curNote = notes[currentIndex];
            yield return new WaitUntil( () => curNote.calcTime <= NowPlaying.PlaybackChanged + GameSetting.PreLoadTime );

            if ( curNote.isSlider ) slidertime = curNote.sliderTime;

            if ( currentIndex + 1 < notes.Count &&
                 ( slidertime > notes[currentIndex + 1].time ||
                   notetime == notes[currentIndex + 1].time ) )
            { 
                Debug.Log( "overlab " );
            }

            NoteRenderer note = nPool.Spawn();
            note.SetInfo( lane.Key, this, curNote );
            
            lane.InputSys.Enqueue( note );
            currentIndex++;
        }
    } 
}
