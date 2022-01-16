using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSystem : MonoBehaviour
{
    public Lane lane;
    private InGame scene;
    private Judgement judgement;

    private Queue<NoteRenderer> notes = new Queue<NoteRenderer>();
    private NoteRenderer currentNote;

    private Queue<NoteRenderer> sliderMissQueue = new Queue<NoteRenderer>();
    public delegate void DelInputEvent( bool _isKeyDown );
    public event DelInputEvent OnInputEvent;

    private GameKeyAction key;

    private bool isHolding = false;
    private float playback;

    public void Enqueue( NoteRenderer _note ) => notes.Enqueue( _note );

    private void Awake()
    {
        scene      = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        judgement  = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();

        scene.OnGameStart += Initialize;
    }

    private void Initialize()
    {
        key = ( GameKeyAction )lane.Key;
        StartCoroutine( NoteSelect() );
    }

    private IEnumerator NoteSelect()
    {
        yield return new WaitUntil( () => currentNote == null && notes.Count > 0 );
        currentNote ??= notes.Dequeue();
    }

    private void SelectNextNote( bool _isDespawn = true )
    {
        isHolding = false;
        currentNote.isHolding = false;

        if ( _isDespawn )
            currentNote.Despawn();

        if ( notes.Count > 0 )
             currentNote = notes.Dequeue();
        else
        {
            currentNote = null;
            StartCoroutine( NoteSelect() );
        }
    }

    private void CheckNote( bool _isInputDown )
    {
        float startDiff = currentNote.Time - NowPlaying.Playback;
        if ( _isInputDown )
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

    private void CheckSlider( bool _isInputDown, bool _isInputHold, bool _isInputUp )
    {
        float startDiff = currentNote.Time - NowPlaying.Playback;
        float endDiff   = currentNote.SliderTime - NowPlaying.Playback;
        if ( !isHolding && _isInputDown )
        {
            if ( judgement.IsCalculated( startDiff ) )
            {
                isHolding = true;
                currentNote.isHolding = true;
            }
        }
        else if ( isHolding && _isInputHold )
        {
            if ( judgement.IsMiss( endDiff ) )
            {
                SelectNextNote();
                return;
            }

            playback += Time.deltaTime;
            if ( playback > .25f )
            {
                //GameManager.Combo++;
                playback = 0f;
            }
        }
        else if ( isHolding && _isInputUp )
        {
            if ( judgement.IsCalculated( endDiff ) )
            {
                isHolding = false;
                currentNote.isHolding = false;
                SelectNextNote();
            }
            else if ( judgement.IsMiss( endDiff ) )
            {
                sliderMissQueue.Enqueue( currentNote );
                SelectNextNote( false );
            }
            return;
        }

        // 마지막 판정까지 안눌렀을 때 ( Miss )
        if ( judgement.IsMiss( endDiff ) )
        {
            isHolding = false;
            currentNote.isHolding = false;
            SelectNextNote();
        }
    }

    private void Update()
    {
        bool isInputDown = Input.GetKeyDown( GameSetting.Inst.Keys[key] );
        bool isInputHold = Input.GetKey( GameSetting.Inst.Keys[key] );
        bool isInputUp   = Input.GetKeyUp( GameSetting.Inst.Keys[key] );

        if ( isInputDown )    OnInputEvent?.Invoke( true );
        else if ( isInputUp ) OnInputEvent?.Invoke( false );

        if ( currentNote == null ) 
             return;

        if ( currentNote.IsSlider ) CheckSlider( isInputDown, isInputHold, isInputUp );
        else                        CheckNote( isInputDown );

        if ( sliderMissQueue.Count > 0 )
        {
            var slider = sliderMissQueue.Peek();
            float endDiff = ( slider.SliderTime - NowPlaying.Playback );
            if ( judgement.IsMiss( endDiff ) )
            {
                slider.Despawn();
                sliderMissQueue.Dequeue();
            }
        }
    }
}
