using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputSystem : MonoBehaviour
{
    public Queue<NoteRenderer> notes = new Queue<NoteRenderer>();
    private NoteRenderer currentNote;

    private float startDiff = 0f, startDiffAbs = 0f;
    private float endDiff = 0f, endDiffAbs = 0f;
    private bool isCheckComplate = true;

    public GAME_KEY_ACTION key;
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
            currentNote = notes.Dequeue();
            isCheckComplate = false;
        }

        if ( isCheckComplate ) return;

        startDiff = currentNote.Time - InGame.Playback;
        startDiffAbs = Mathf.Abs( startDiff );

        if ( currentNote.IsSlider )
        {
            endDiff    = currentNote.SliderTime - InGame.Playback;
            endDiffAbs = Mathf.Abs( endDiff );

            if ( Input.GetKeyDown( GlobalKeySetting.Inst.Keys[key] ) )
            {
                if ( startDiff < 150f )
                {
                    currentNote.isHolding = true;
                    isHolding = true;
                    //GameManager.Combo++;
                }
            }
            else if ( isHolding && Input.GetKey( GlobalKeySetting.Inst.Keys[key] ) )
            {
                //GameManager.Combo++;
            }
            else if ( isHolding && Input.GetKeyUp( GlobalKeySetting.Inst.Keys[key] ) )
            {
                if ( endDiff > 150f )
                {
                    // miss
                    currentNote.GetComponent<SpriteRenderer>().color = Color.gray;
                }
                else if ( endDiffAbs < 150f )
                {
                    //GameManager.Combo++;
                    isHolding = false;
                    currentNote.isHolding = false;
                    isCheckComplate = true;
                    currentNote.Destroy();
                    currentNote.gameObject.SetActive( false );
                }
            }

            if ( endDiff < -150f )
            {
                currentNote.Destroy();
                currentNote.gameObject.SetActive( false );
                isHolding = false;
                currentNote.isHolding = false;
                isCheckComplate = true;
            }
        }
        else
        {
            if ( startDiff < -150f )
            {
                //GameManager.Combo = 0;
                currentNote.Destroy();
                currentNote.gameObject.SetActive( false );
                //InGame.cPool.Despawn( curColNote );
                isCheckComplate = true;
            }
            else if ( startDiffAbs < 150f )
            {
                if ( Input.GetKeyDown( GlobalKeySetting.Inst.Keys[key] ) )
                {
                    //GameManager.Combo++;
                    currentNote.Destroy();
                    currentNote.gameObject.SetActive( false );
                    //InGame.cPool.Despawn( curColNote );
                    isCheckComplate = true;
                }
            }
        }
    }
}
