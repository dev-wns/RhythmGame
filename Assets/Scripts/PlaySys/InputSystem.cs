using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputSystem : MonoBehaviour
{
    public Queue<NoteRenderer> notes = new Queue<NoteRenderer>();
    private NoteRenderer curNote;

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

        startDiff = curNote.Time - InGame.Playback;
        startDiffAbs = Mathf.Abs( startDiff );

        if ( curNote.IsSlider )
        {
            endDiff    = curNote.SliderTime - InGame.Playback;
            endDiffAbs = Mathf.Abs( endDiff );

            if ( Input.GetKeyDown( GlobalKeySetting.Inst.Keys[key] ) )
            {
                if ( startDiff < 150f )
                {
                    curNote.isHolding = true;
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
                    curNote.GetComponent<SpriteRenderer>().color = Color.gray;
                }
                else if ( endDiffAbs < 150f )
                {
                    //GameManager.Combo++;
                    isHolding = false;
                    curNote.isHolding = false;
                    isCheckComplate = true;
                    curNote.Destroy();
                    curNote.gameObject.SetActive( false );
                }
            }

            if ( endDiff < -150f )
            {
                curNote.Destroy();
                curNote.gameObject.SetActive( false );
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
                curNote.Destroy();
                curNote.gameObject.SetActive( false );
                //InGame.cPool.Despawn( curColNote );
                isCheckComplate = true;
            }
            else if ( startDiffAbs < 150f )
            {
                if ( Input.GetKeyDown( GlobalKeySetting.Inst.Keys[key] ) )
                {
                    //GameManager.Combo++;
                    curNote.Destroy();
                    curNote.gameObject.SetActive( false );
                    //InGame.cPool.Despawn( curColNote );
                    isCheckComplate = true;
                }
            }
        }
    }
}
