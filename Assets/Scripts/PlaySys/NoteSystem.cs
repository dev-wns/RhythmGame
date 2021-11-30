using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct NoteData
{
    public float time; // hit timing
    public float calcTime;
    public float LNEndTime;
    public int line;
    public int type; // 128 = Long Note or Default Note

    public NoteData( float _time, float _calcTime, float _LNEndTime, int _type, int _line )
    {
        time = _time; calcTime = _calcTime; LNEndTime = _LNEndTime; type = _type; line = _line;
    }
}

public class NoteSystem : MonoBehaviour
{
    public Queue<NoteData> datas;
    private NoteData curData;
    private InputSystem ISystem;

    private void Awake()
    {
        ISystem = GetComponent<InputSystem>();
        datas = new Queue<NoteData>();
        InGame.SystemsInitialized += Initialized;
    }

    private void Initialized()
    {
        if ( datas.Count == 0 )
        {
            Debug.Log( "Note System Initialize Fail " );
            return;
        }

        StartCoroutine( Process() );
    }

    private IEnumerator Process()
    {
        while ( datas.Count > 0 )
        {
            curData = datas.Dequeue();
            float timing = curData.calcTime;
            yield return new WaitUntil( () => timing <= NowPlaying.PlaybackChanged + NowPlaying.PreLoadTime && NowPlaying.IsPlaying );

            Note note = InGame.nPool.Spawn();
            note.Initialized( curData );
            ISystem.notes.Enqueue( note );
        }
    } 
}
