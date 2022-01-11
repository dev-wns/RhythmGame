using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSystem : MonoBehaviour
{
    private InGame scene;

    public int lane;
    private ObjectPool<NoteRenderer> nPool;
    public NoteRenderer nPrefab;

    private List<Note> notes = new List<Note>();
    private int currentIndex;

    private InputSystem inputSystem;

    private void Awake()
    {
        scene   = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        nPool   = new ObjectPool<NoteRenderer>( nPrefab, 10 );

        scene.OnGameStart += Initialize;
    }

    public void AddNote( Note _note ) => notes.Add( _note );

    public bool HasCompareNote( Note _note )
    {
        if ( notes.Count == 0 ) return false;

        return notes[notes.Count - 1].time == _note.time ? true : false;
    }

    public void Despawn( NoteRenderer _note ) => nPool.Despawn( _note );

    private void Initialize()
    {
        inputSystem = GameObject.FindGameObjectWithTag( "Systems" ).GetComponentsInChildren<InputSystem>()[lane];
        StartCoroutine( Process() );
    }

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
            note.SetInfo( lane, curNote );
            note.system = this;
            inputSystem.Enqueue( note );
            currentIndex++;
        }
    } 
}
