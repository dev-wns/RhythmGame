using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputSystem : MonoBehaviour
{
    public Queue<Note> notes = new Queue<Note>();
    private Note curNote;

    private float startDiff = 0f;
    private float endDiff = 0f;
    private bool isCheckComplate = true;

    public KeyAction key;
    private int keyIndex;
    bool isHolding = false;

    private void Awake()
    {
        keyIndex = ( int )key;

        transform.position = new Vector3( GlobalSetting.NoteStartPos + ( GlobalSetting.NoteWidth * keyIndex ) +
                                        ( GlobalSetting.NoteBlank * keyIndex ) + GlobalSetting.NoteBlank, 
                                          transform.parent.transform.position.y, 0f );
    }   

    private void Update()
    {
        if ( isCheckComplate && notes.Count >= 1 )
        {
            curNote = notes.Dequeue();
            isCheckComplate = false;
        }

        if ( isCheckComplate ) return;

        startDiff = Mathf.Abs( NowPlaying.PlaybackChanged - curNote.calcTiming );
    
        if ( Input.GetKeyDown( KeySetting.Keys[key] ) )
        {
            if ( startDiff < 150f )
            {
                GameManager.Combo++;
                curNote.gameObject.SetActive( false );
                InGame.nPool.Despawn( curNote );
                //InGame.cPool.Despawn( curColNote );
                isCheckComplate = true;
                return;
            }
        }
    
        if ( curNote.calcTiming - NowPlaying.PlaybackChanged < 0 && startDiff > 150f )
        {
            //GameManager.Combo = 0;
            InGame.nPool.Despawn( curNote );
            //InGame.cPool.Despawn( curColNote );
            isCheckComplate = true;
            return;
        }
    }
}
