using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSystem : MonoBehaviour
{
    private InGame scene;

    // 60bpm은 분당 1/4박자 60개, 스크롤 속도가 1일때 한박자(1/4) 시간은 1초
    private ObjectPool<NoteRenderer> nPool;
    public NoteRenderer nPrefab;

    private List<Note> notes = new List<Note>();
    private int currentIndex;

    private InputSystem inputSystem;

    private void Awake()
    {
        scene   = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        nPool   = new ObjectPool<NoteRenderer>( nPrefab, 10 );
        inputSystem = GetComponent<InputSystem>();

        scene.OnGameStart += () => StartCoroutine( Process() );
    }

    public void AddNote( Note _note ) => notes.Add( _note );

    public void Despawn( NoteRenderer _note ) => nPool.Despawn( _note );

    private IEnumerator Process()
    {
        while ( currentIndex < notes.Count )
        {
            Note curNote = notes[currentIndex];
            yield return new WaitUntil( () => curNote.calcTime <= NowPlaying.PlaybackChanged + GameSetting.PreLoadTime );

            NoteRenderer note = nPool.Spawn();
            note.SetInfo( curNote );
            inputSystem.Enqueue( note );
            currentIndex++;
        }
    } 
}
