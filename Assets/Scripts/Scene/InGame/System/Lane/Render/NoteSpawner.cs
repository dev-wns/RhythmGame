using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    [Header( "Lane" )]
    private int lane;

    [Header( "Prefab" )]
    public NoteRenderer note1; /* Lane 0,2,3,5 */
    public NoteRenderer note2; /* Lane 1,4 */
    public NoteRenderer noteMedian;

    [Header( "Note" )]
    private ObjectPool<NoteRenderer> pool;
    private Queue<NoteRenderer> notes        = new (); // 현재 스폰된 노트 리스트
    private Queue<NoteRenderer> lnMissQueue  = new ();
    private Queue<NoteRenderer> lnEarlyQueue = new ();
    private int spawnIndex;

    private bool isPrevHolding; // pause 전에 hold 중이였는지

    public  NoteRenderer CurrentNote { get; private set; } // 가장 먼저 스폰된 노트
    private ReadOnlyCollection<Note> datas;

    public void Initialize( Lane _lane )
    {
        lane = _lane.Key;

        NoteRenderer note = note1;
        if (      NowPlaying.KeyCount == 4 ) note = lane == 1 || lane == 2 ? note2 : note1;
        else if ( NowPlaying.KeyCount == 6 ) note = lane == 1 || lane == 4 ? note2 : note1;
        else if ( NowPlaying.KeyCount == 7 ) note = lane == 1 || lane == 5 ? note2 : lane == 3 ? noteMedian : note1;
        pool ??= new ObjectPool<NoteRenderer>( note, 5 );
    }

    public void SetNotes( ReadOnlyCollection<Note> _datas ) => datas = _datas;

    public void Pause( bool _isPause )
    {
        if ( CurrentNote == null )
             return;

        if ( _isPause )
        {
            isPrevHolding = CurrentNote.IsKeyDown;
            if ( isPrevHolding )
                 CurrentNote.IsKeyDown = false;
        }
        else
        {
            if ( isPrevHolding )
                 CurrentNote.IsKeyDown = true;
        }
    }

    public void Clear()
    {
        while ( lnMissQueue.Count > 0 )
        {
            var slider = lnMissQueue.Dequeue();
            slider.Despawn();
        }

        while ( lnEarlyQueue.Count > 0 )
        {
            var slider = lnEarlyQueue.Dequeue();
            slider.Despawn();
        }
        pool.AllDespawn();
        notes.Clear();

        isPrevHolding = false;
        spawnIndex    = 0;
        CurrentNote   = null;
    }

    public void SpawnNotes( double _playback )
    {
        if ( spawnIndex < datas.Count )
        {
            Note current = datas[spawnIndex];
            if ( current.distance <= NowPlaying.Distance + GameSetting.MinDistance )
            {
                NoteRenderer note = pool.Spawn();
                note.SetInfo( lane, in current );
                notes.Enqueue( note );

                spawnIndex++;
            }
        }

        if ( CurrentNote == null && notes.Count > 0 )
             CurrentNote = notes.Dequeue();
    }

    private void LateUpdate()
    {
        // 일찍 처리된 롱노트 판정선에 닿을 때 디스폰
        if ( lnEarlyQueue.TryPeek( out NoteRenderer lnEarly ) )
        {
            if ( lnEarly.EndTime < NowPlaying.Playback )
            {
                lnEarly.Despawn();
                lnEarlyQueue.Dequeue();
            }
        }

        // 미스처리된 롱노트 화면 밖에서 디스폰 
        if ( lnMissQueue.TryPeek( out NoteRenderer lnMiss ) )
        {
            if ( lnMiss.TailPos < -( ( Global.Screen.Height * .5f ) + 100f ) )
            {
                lnMiss.Despawn();
                lnMissQueue.Dequeue();
            }
        }
    }

    public void Despawn()
    {
        if ( CurrentNote == null )
        {
            Debug.LogError( "Cannot be Despawned( CurrentNote is null )" );
            return;
        }
        pool.Despawn( CurrentNote );
        CurrentNote = null;
    }

    public void AddMissQueue()
    {
        if ( CurrentNote == null )
        {
            Debug.LogError( "Cannot add to MissQueue( CurrentNote is null )" );
            return;
        }
        lnMissQueue.Enqueue( CurrentNote );
        CurrentNote = null;
    }

    public void AddEarlyQueue()
    {
        if ( CurrentNote == null )
        {
            Debug.LogError( "Cannot add to EarlyQueue( CurrentNote is null )" );
            return;
        }
        lnEarlyQueue.Enqueue( CurrentNote );
        CurrentNote = null;
    }
}