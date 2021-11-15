using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotePool : Singleton<NotePool>
{
    // pool
    private Queue<Note> notes = new Queue<Note>();
    public Note notePrefab;
    public Transform noteParent;
    private int defaultCapacity = 200;

    private void InstantiateNote( int _capacity )
    {
        for ( int idx = 0; idx < _capacity; idx++ )
        {
            notes.Enqueue( Instantiate( notePrefab, noteParent ) );
        }
    }

    public void Enqueue( Note _obj )
    {
        _obj.gameObject.SetActive( false );
        notes.Enqueue( _obj );
    }

    public Note Dequeue()
    {
        if ( notes.Count <= 0 )
        {
            InstantiateNote( defaultCapacity );
        }

        Note obj = notes.Dequeue();
        obj.gameObject.SetActive( true );

        return obj;
    }
}
