using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSystem : MonoBehaviour
{
    public Lane lane;
    private InGame scene;
    private Judgement judge;

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
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();

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
        playback = 0f;
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
        var startType = judge.GetJudgeType( startDiff );

        if ( _isInputDown )
        {
            if ( startType != JudgeType.None && startType != JudgeType.Miss )
            {
                judge.OnJudgement( startType );
                SelectNextNote();
            }
        }

        // 마지막 판정까지 안눌렀을 때 ( Miss )
        if ( startType == JudgeType.Miss )
        {
            judge.OnJudgement( JudgeType.Miss );
            SelectNextNote();
        }        
    }

    private void CheckSlider( bool _isInputDown, bool _isInputHold, bool _isInputUp )
    {
        float startDiff = currentNote.Time       - NowPlaying.Playback;
        float endDiff   = currentNote.SliderTime - NowPlaying.Playback;

        var startType = judge.GetJudgeType( startDiff );
        var endType   = judge.GetJudgeType( endDiff );


        if ( !isHolding && _isInputDown )
        {
            if ( startType != JudgeType.None && startType != JudgeType.Miss )
            {
                isHolding = true;
                currentNote.isHolding = true;
                judge.OnJudgement( startType );
            }
        }
        else if ( isHolding && _isInputHold )
        {
            if ( endType == JudgeType.Miss ) 
            {
                judge.OnJudgement( JudgeType.Miss );

                SelectNextNote();
                return;
            }

            playback += Time.deltaTime;
            if ( playback > .1f )
            {
                judge.OnJudgement( JudgeType.None );
                playback = 0f;
            }
        }
        else if ( isHolding && _isInputUp )
        {
            if ( endType != JudgeType.None && endType != JudgeType.Miss )
            {
                // 판정 범위 안에서 키 뗏을 때
                judge.OnJudgement( endType );

                SelectNextNote();
            }
            else if ( endType == JudgeType.None )
            {
                // 판정 범위 밖에서 키 뗏을 때
                judge.OnJudgement( JudgeType.Miss );

                currentNote.SetBodyFail();
                sliderMissQueue.Enqueue( currentNote );
                SelectNextNote( false );
            }
            return;
        }

        // 롱노트 시작부분 처리 못했을 때
        if ( !isHolding && startType == JudgeType.Miss ) 
        {
            judge.OnJudgement( JudgeType.Miss );

            currentNote.SetBodyFail();
            sliderMissQueue.Enqueue( currentNote );
            SelectNextNote( false );
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
    }

    private void LateUpdate()
    {
        if ( sliderMissQueue.Count > 0 )
        {
            var slider = sliderMissQueue.Peek();
            float endDiff = slider.SliderTime - NowPlaying.Playback;
            if ( judge.GetJudgeType( endDiff ) == JudgeType.Miss )
            {
                slider.Despawn();
                sliderMissQueue.Dequeue();
            }
        }
    }
}
