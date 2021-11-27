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

    public int keyIndex;

    private void Awake()
    {
        transform.position = new Vector3( GlobalSetting.NoteStartPos + ( GlobalSetting.NoteWidth * keyIndex ) +
                                        ( GlobalSetting.NoteBlank * keyIndex ) + GlobalSetting.NoteBlank, 
                                          transform.parent.transform.position.y, 0f );

        //StartCoroutine( Process() );
        //StartCoroutine( CheckOutLine() );
    }

    IEnumerator Process()
    {
        yield return new WaitUntil( () => !isCheckComplate );

        if ( Input.GetKeyDown( KeySetting.Keys[( KeyAction )keyIndex] ) )
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

        //if ( curNote.timing - InGame.__time < 0 && diff > 150f )
        if ( diff < 150f )
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

        diff = Mathf.Abs( InGame.__time - curNote.originTiming );

        if ( Input.GetKeyDown( KeySetting.Keys[( KeyAction )keyIndex] ) )
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

        //if ( diff < 150f )
        if ( curNote.timing - InGame.__time < 0 && diff > 150f )
        {
            //GameManager.Combo = 0;
            InGame.nPool.Despawn( curNote );
            //InGame.cPool.Despawn( curColNote );
            isCheckComplate = true;
            return;
        }
    }
}
