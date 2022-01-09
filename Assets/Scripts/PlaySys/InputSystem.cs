using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSystem : MonoBehaviour
{
    private NoteSystem noteSystem;
    private Judgement judgement;
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
        judgement  = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();

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
                //currentNote.SetColor( Color.green );
            }

            yield return null;
        }
    }

    private void SelectNextNote()
    {
        currentNote.gameObject.SetActive( false );
        noteSystem.Despawn( currentNote );
        currentNote = null;
        
        StartCoroutine( NoteSelect() );
    }

    private void CheckNote()
    {
        float startDiff = currentNote.Time - NowPlaying.Playback;
        if ( Input.GetKeyDown( GlobalKeySetting.Inst.Keys[key] ) )
        {
            if ( judgement.IsCalculated( startDiff ) )
                 SelectNextNote();
        }
        else
        {
            // 마지막 판정까지 안눌렀을 때 ( Miss )
            if ( judgement.IsMiss( startDiff ) )
                 SelectNextNote();
        }
    }

    private void CheckSlider()
    {
        float startDiff = currentNote.Time - NowPlaying.Playback;
        float endDiff   = currentNote.SliderTime - NowPlaying.Playback;
        if ( !isHolding && Input.GetKeyDown( GlobalKeySetting.Inst.Keys[key] ) )
        {
            if ( judgement.IsCalculated( startDiff ) )
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
                //GameManager.Combo++;
                playback = 0f;
            }
        }
        else if ( isHolding && Input.GetKeyUp( GlobalKeySetting.Inst.Keys[key] ) )
        {
            if ( !judgement.IsCalculated( endDiff ) )
                 currentNote.SetColor( Color.gray );
         
            sliderMissQueue.Enqueue( currentNote );
            isHolding = false;
            currentNote.isHolding = false;

            currentNote = null;
            StartCoroutine( NoteSelect() );
            return;
        }
        
        // 마지막 판정까지 안눌렀을 때 ( Miss )
        if ( !isHolding && judgement.IsMiss( endDiff ) )
             SelectNextNote();
    }

    private void Update()
    {
        if ( currentNote == null ) return;

        if ( currentNote.IsSlider ) CheckSlider();
        else                        CheckNote();

        if ( sliderMissQueue.Count > 0 )
        {
            var slider = sliderMissQueue.Peek();
            float endDiff = ( slider.SliderTime - NowPlaying.Playback );
            if ( judgement.IsMiss( endDiff ) )
            {
                noteSystem.Despawn( slider );
                sliderMissQueue.Dequeue();
            }
        }
    }
}
