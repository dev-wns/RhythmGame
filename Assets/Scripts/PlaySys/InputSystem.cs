using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputSystem : MonoBehaviour
{
    public Queue<Note> notes = new Queue<Note>();
    private Note curNote;

    private float startDiff = 0f, startDiffAbs = 0f;
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

        startDiff = curNote.calcTiming - NowPlaying.PlaybackChanged;
        startDiffAbs = Mathf.Abs( startDiff );

        //if ( curNote.IsLN )
        //{
        //    isHolding = false;
        //    float endDiff = Mathf.Abs( NowPlaying.PlaybackChanged - curNote.calcEndTiming );
        //    if ( Input.GetKeyDown( KeySetting.Keys[key] ) )
        //    {
        //        if ( startDiff < 150f )
        //        {
        //            isHolding = true;
        //            // 판정
        //        }
        //    }
        //    else if ( isHolding && Input.GetKey( KeySetting.Keys[key] ) )
        //    {
        //        curNote.transform.localScale = new Vector3( GlobalSetting.NoteWidth, Mathf.Abs( ( curNote.calcEndTiming * NowPlaying.Weight ) - ( GlobalSetting.JudgeLine * NowPlaying.Weight ) ), 1f );
        //    }
        //    else if ( isHolding && Input.GetKeyUp( KeySetting.Keys[key] ) )
        //    {
        //        if ( curNote.calcEndTiming - NowPlaying.PlaybackChanged < 0 && endDiff > 150f )
        //        {

        //        }
        //        else if ( endDiff < 150f )
        //        {
        //            // 판정
        //            isHolding = false;
        //            isCheckComplate = true;
        //            curNote.gameObject.SetActive( false );
        //            InGame.nPool.Despawn( curNote );
        //        }
        //    }
        //}
        //else
        //{

            
        if ( startDiffAbs < 150f )
        {
            if ( Input.GetKeyDown( KeySetting.Keys[key] ) )
            {
                GameManager.Combo++;
                curNote.gameObject.SetActive( false );
                InGame.nPool.Despawn( curNote );
                //InGame.cPool.Despawn( curColNote );
                isCheckComplate = true;
            }
        }

        if ( !curNote.IsLN && startDiff < -150f)
        {
            //GameManager.Combo = 0;
            InGame.nPool.Despawn( curNote );
            curNote = null;
            //InGame.cPool.Despawn( curColNote );
            isCheckComplate = true;
        }
        //}
    }
}
