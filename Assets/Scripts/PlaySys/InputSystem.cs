using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputSystem : MonoBehaviour
{
    public Queue<Note> notes = new Queue<Note>();
    private Note curNote;

    private float startDiff = 0f, startDiffAbs = 0f;
    private float endDiff = 0f, endDiffAbs = 0f;
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

        startDiff = curNote.timing - NowPlaying.Playback;
        startDiffAbs = Mathf.Abs( startDiff );

        if ( curNote.IsLN )
        {
            endDiff    = curNote.endTiming - NowPlaying.Playback;
            endDiffAbs = Mathf.Abs( endDiff );

            if ( Input.GetKeyDown( KeySetting.Keys[key] ) )
            {
                if ( startDiff < 150f )
                {
                    curNote.isHolding = true;
                    isHolding = true;
                    //GameManager.Combo++;
                }
            }
            else if ( isHolding && Input.GetKey( KeySetting.Keys[key] ) )
            {
                //GameManager.Combo++;
            }
            else if ( isHolding && Input.GetKeyUp( KeySetting.Keys[key] ) )
            {
                if ( endDiff > 150f )
                {
                    // miss
                    curNote.GetComponent<SpriteRenderer>().color = Color.gray;
                }
                else if ( endDiffAbs < 150f )
                {
                    //GameManager.Combo++;
                    isHolding = false;
                    curNote.isHolding = false;
                    isCheckComplate = true;
                    curNote.gameObject.SetActive( false );
                    InGame.nPool.Despawn( curNote );
                }
            }

            if ( endDiff < -150f )
            {
                InGame.nPool.Despawn( curNote );
                isHolding = false;
                curNote.isHolding = false;
                isCheckComplate = true;
            }
        }
        else
        {
            if ( startDiff < -150f )
            {
                //GameManager.Combo = 0;
                InGame.nPool.Despawn( curNote );
                //InGame.cPool.Despawn( curColNote );
                isCheckComplate = true;
            }
            else if ( startDiffAbs < 150f )
            {
                if ( Input.GetKeyDown( KeySetting.Keys[key] ) )
                {
                    //GameManager.Combo++;
                    curNote.gameObject.SetActive( false );
                    InGame.nPool.Despawn( curNote );
                    //InGame.cPool.Despawn( curColNote );
                    isCheckComplate = true;
                }
            }
        }
    }
}
