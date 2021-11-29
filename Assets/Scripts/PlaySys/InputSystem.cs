using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputSystem : MonoBehaviour
{
    public Queue<Note> notes = new Queue<Note>();
    private Note curNote;

    private float diff = 0f;
    private bool isCheckComplate = true;

    public KeyAction key;
    private int keyIndex;

    private void Awake()
    {
        keyIndex = ( int )key;

        transform.position = new Vector3( GlobalSetting.NoteStartPos + ( GlobalSetting.NoteWidth * keyIndex ) +
                                        ( GlobalSetting.NoteBlank * keyIndex ) + GlobalSetting.NoteBlank, 
                                          transform.parent.transform.position.y, 0f );

        //key = KeySetting.Keys[( KeyAction )keyIndex];
        //StartCoroutine( Process() );
        //StartCoroutine( CheckOutLine() );
    }   

    IEnumerator Process()
    {
        yield return new WaitUntil( () => !isCheckComplate );

        if ( Input.GetKeyDown( KeySetting.Keys[key] ) )
        {
            if ( diff < 150f )
            {
                GameManager.Combo++;
                InGame.nPool.Despawn( curNote );
                //InGame.cPool.Despawn( curColNote );
                isCheckComplate = true;
            }
        }
        StartCoroutine( Process() );
    }

    IEnumerator CheckOutLine()
    {
        yield return new WaitUntil( () => !isCheckComplate );

        if ( curNote.timing - NowPlaying.Playback < 0 && diff > 150f )
            //if ( diff < 150f )
        {
            //GameManager.Combo = 0;
            InGame.nPool.Despawn( curNote );
            //InGame.cPool.Despawn( curColNote );
            isCheckComplate = true;
        }

        StartCoroutine( CheckOutLine() );
    }

    private void Update()
    {
        if ( isCheckComplate && notes.Count >= 1 )
        {
            curNote = notes.Dequeue();
            isCheckComplate = false;
        }

        if ( isCheckComplate ) return;

        diff = Mathf.Abs( NowPlaying.PlaybackChanged - curNote.calcTiming );


        if ( Input.GetKeyDown( KeySetting.Keys[key] ) )
        {
            if ( diff < 150f )
            {
                GameManager.Combo++;
                curNote.gameObject.SetActive( false );
                InGame.nPool.Despawn( curNote );
                //InGame.cPool.Despawn( curColNote );
                isCheckComplate = true;
                return;
            }
        }

        if ( curNote.calcTiming - NowPlaying.PlaybackChanged < 0 && diff > 150f )
        {
            //GameManager.Combo = 0;
            InGame.nPool.Despawn( curNote );
            //InGame.cPool.Despawn( curColNote );
            isCheckComplate = true;
            return;
        }
    }
}
