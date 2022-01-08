using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSystem : MonoBehaviour
{
    private NoteSystem noteSystem;
    private Queue<NoteRenderer> notes = new Queue<NoteRenderer>();
    private NoteRenderer currentNote;

    private Queue<NoteRenderer> sliderMissQueue = new Queue<NoteRenderer>();

    public GAME_KEY_ACTION key;
    private int keyIndex;

    private bool isHolding = false;
    private float playback;

    public void Enqueue( NoteRenderer _note ) => notes.Enqueue( _note );

    private void Awake()
    {
        keyIndex = ( int )key;

        noteSystem = GetComponent<NoteSystem>();

        transform.position = new Vector3( GlobalSetting.NoteStartPos + ( GlobalSetting.NoteWidth * keyIndex ) +
                                        ( GlobalSetting.NoteBlank * keyIndex ) + GlobalSetting.NoteBlank, 
                                          transform.parent.transform.position.y, 0f );

        StartCoroutine( NoteSelect() );
    }

    private IEnumerator NoteSelect()
    {
        while ( currentNote == null )
        {
            if ( notes.Count > 0 )
            {
                currentNote = notes.Dequeue();
            }

            yield return null;
        }
    }

    private void JudgeEnd()
    {
        noteSystem.Despawn( currentNote );
        currentNote = null;
        
        StartCoroutine( NoteSelect() );
    }

    private void NoteJudge()
    {
        float diff = currentNote.Time - InGame.Playback;
        float diffAbs = Mathf.Abs( currentNote.Time - InGame.Playback );
        if ( Input.GetKeyDown( GlobalKeySetting.Inst.Keys[key] ) )
        {
            if ( Judge( diffAbs ) )
                 JudgeEnd();
        }
        else
        {
            // 마지막 판정까지 안눌렀을 때 ( Miss )
            if ( diff < -( 22 + 35 + 28 ) )
                 JudgeEnd();
        }
    }

    private void SliderJudge()
    {
        float startDiff    = currentNote.Time - InGame.Playback;
        float startDiffAbs = Mathf.Abs( startDiff );

        float endDiff    = currentNote.SliderTime - InGame.Playback;
        float endDiffAbs = Mathf.Abs( endDiff );

        if ( !isHolding && Input.GetKeyDown( GlobalKeySetting.Inst.Keys[key] ) )
        {
            if ( Judge( startDiffAbs ) )
            {
                isHolding = true;
                currentNote.isHolding = true;
            }
        }
        else if ( isHolding && Input.GetKey( GlobalKeySetting.Inst.Keys[key] ) )
        {
            playback += Time.deltaTime;
            if ( playback > .15f )
            {
                GameManager.Combo++;
                playback = 0f;
            }
        }
        else if ( isHolding && Input.GetKeyUp( GlobalKeySetting.Inst.Keys[key] ) )
        {
            if ( Judge( endDiffAbs ) )
            {
                isHolding = false;
                JudgeEnd();
            }
            else
            {
                currentNote.SetColor( Color.gray );
                sliderMissQueue.Enqueue( currentNote );
                isHolding = false;
                currentNote.isHolding = false;

                currentNote = null;
                StartCoroutine( NoteSelect() );
                return;
            }
        }

        // 마지막 판정까지 안눌렀을 때 ( Miss )
        if ( endDiff < -( 22 + 35 + 28 ) )
        {
            JudgeEnd();
            isHolding = false;
        }
    }

    private bool Judge( float _diff )
    {
        // Kool 22 Cool 35 Good 28
        if ( _diff <= 22 )
        {
            GameManager.Combo++;
            GameManager.Kool++;
            return true;
        }
        else if ( _diff > 22 && _diff <= 22 + 35 )
        {
            GameManager.Combo++;
            GameManager.Cool++;
            return true;
        }
        else if ( _diff > 22 + 35 && _diff <= 22 + 35 + 28 )
        {
            GameManager.Combo++;
            GameManager.Good++;
            return true;
        }

        return false;
    }

    private void Update()
    {
        if ( currentNote == null ) return;

        if ( currentNote.IsSlider ) SliderJudge();
        else                        NoteJudge();

        if ( sliderMissQueue.Count > 0 )
        {
            var slider = sliderMissQueue.Peek();
            float endDiff = slider.SliderTime - InGame.Playback;
            if ( endDiff < -( 22 + 35 + 28 ) )
            {
                noteSystem.Despawn( slider );
                sliderMissQueue.Dequeue();
            }
        }

        //if ( currentNote.IsSlider )
        //{
        //    endDiff    = currentNote.SliderTime - InGame.Playback;
        //    endDiffAbs = Mathf.Abs( endDiff );

        //    if ( Input.GetKeyDown( GlobalKeySetting.Inst.Keys[key] ) )
        //    {
        //        if ( startDiff < 150f )
        //        {
        //            currentNote.isHolding = true;
        //            isHolding = true;
        //            //GameManager.Combo++;
        //        }
        //    }
        //    else if ( isHolding && Input.GetKey( GlobalKeySetting.Inst.Keys[key] ) )
        //    {
        //        //GameManager.Combo++;
        //    }
        //    else if ( isHolding && Input.GetKeyUp( GlobalKeySetting.Inst.Keys[key] ) )
        //    {
        //        if ( endDiff > 150f )
        //        {
        //            // miss
        //            currentNote.GetComponent<SpriteRenderer>().color = Color.gray;
        //        }
        //        else if ( endDiffAbs < 150f )
        //        {
        //            //GameManager.Combo++;
        //            isHolding = false;
        //            currentNote.isHolding = false;
        //            noteSystem.Despawn( currentNote );
        //        }
        //    }

        //    if ( endDiff < -150f )
        //    {
        //        noteSystem.Despawn( currentNote );
        //        isHolding = false;
        //        currentNote.isHolding = false;
        //    }
        //}
        //else
        //{
        //    if ( startDiff < -150f )
        //    {
        //        noteSystem.Despawn( currentNote );
        //    }
        //    else if ( startDiffAbs < 150f )
        //    {
        //        if ( Input.GetKeyDown( GlobalKeySetting.Inst.Keys[key] ) )
        //        {
        //            noteSystem.Despawn( currentNote );
        //        }
        //    }
        //}
    }
}
