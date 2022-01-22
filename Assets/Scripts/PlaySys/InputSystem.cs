using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSystem : MonoBehaviour
{
    private Lane lane;
    private InGame scene;
    private Judgement judge;

    private Queue<NoteRenderer> notes = new Queue<NoteRenderer>();
    private NoteRenderer currentNote;

    private Queue<NoteRenderer> sliderMissQueue = new Queue<NoteRenderer>();
    public event Action<bool> OnInputEvent;
    public event Action OnHitNote;

    private GameKeyAction key;

    private bool isHolding = false;
    private float playback;

    private Coroutine waitNoteCoroutine;

    private Action NoteProcessAction;

    public void Enqueue( NoteRenderer _note ) => notes.Enqueue( _note );

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnGameStart += () => waitNoteCoroutine = StartCoroutine( NoteSelect() );

        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();

        lane  = GetComponent<Lane>(); 
        lane.OnLaneInitialize += _key => key = ( GameKeyAction )_key;

        if ( GameSetting.CurrentGameMode.HasFlag( GameMode.AutoPlay ) )
        {
            NoteProcessAction = () =>
            {
                if ( currentNote.IsSlider ) AutoCheckSlider();
                else                        AutoCheckNote();
            };
        }
        else
        {
            NoteProcessAction = () =>
            {
                if ( currentNote.IsSlider ) CheckSlider();
                else                        CheckNote();
            };
        }
    }

    private void OnDestroy()
    {
        if ( waitNoteCoroutine != null )
            StopCoroutine( waitNoteCoroutine );
    }

    private IEnumerator NoteSelect()
    {
        var WaitNote = new WaitUntil( () => currentNote == null && notes.Count > 0 );
        while ( true )
        {
            yield return WaitNote;
            currentNote = notes.Dequeue();
        }
    }

    private void SelectNextNote( bool _isDespawn = true )
    {
        playback = 0f;
        isHolding = false;
        currentNote.isHolding = false;

        if ( _isDespawn ) currentNote.Despawn();
        currentNote = null;
    }

    private void AutoCheckNote()
    {
        float startDiff = currentNote.Time - NowPlaying.Playback;

        if ( startDiff <= 0f )
        {
            var startType = judge.GetJudgeType( startDiff );
            OnHitNote();
            judge.OnJudgement( startType );
            SelectNextNote();
        }
    }

    private void AutoCheckSlider()
    {
        if ( !isHolding )
        {
            float startDiff = currentNote.Time - NowPlaying.Playback;

            if ( startDiff <= 0f )
            {
                var startType   = judge.GetJudgeType( startDiff );
                isHolding = true;
                currentNote.isHolding = true;
                OnHitNote();
                judge.OnJudgement( startType );
            }
        }
        else
        {
            float endDiff = currentNote.SliderTime - NowPlaying.Playback;

            playback += Time.deltaTime;
            if ( playback > .1f )
            {
                OnHitNote();
                judge.OnJudgement( JudgeType.None );
                playback = 0f;
            }

            if ( endDiff <= 0f )
            {
                var endType = judge.GetJudgeType( endDiff );
                OnHitNote();
                judge.OnJudgement( endType );
                SelectNextNote();
            }
        }
    }

    private void CheckNote()
    {
        float startDiff = currentNote.Time - NowPlaying.Playback;
        var startType   = judge.GetJudgeType( startDiff );

        bool isInputDown = Input.GetKeyDown( GameSetting.Inst.Keys[key] );
        if ( isInputDown )
        {
            if ( startType != JudgeType.None && startType != JudgeType.Miss )
            {
                OnHitNote();
                judge.OnJudgement( startType );
                SelectNextNote();
            }
        }

        // 마지막 판정까지 안눌렀을 때 ( Miss )
        if ( startType != JudgeType.None && startType == JudgeType.Miss )
        {
            judge.OnJudgement( JudgeType.Miss );
            SelectNextNote();
        }        
    }

    private void CheckSlider()
    {
        if ( !isHolding )
        {
            float startDiff = currentNote.Time - NowPlaying.Playback;
            var startType   = judge.GetJudgeType( startDiff );

            bool isInputDown = Input.GetKeyDown( GameSetting.Inst.Keys[key] );
            if ( isInputDown )
            {
                if ( startType != JudgeType.None && startType != JudgeType.Miss )
                {
                    isHolding = true;
                    currentNote.isHolding = true;
                    OnHitNote();
                    judge.OnJudgement( startType );
                }
            }

            if ( startType != JudgeType.None && startType == JudgeType.Miss )
            {
                currentNote.SetBodyFail();
                judge.OnJudgement( JudgeType.Miss );
                sliderMissQueue.Enqueue( currentNote );
                SelectNextNote( false );
            }
        }
        if ( isHolding )
        {
            bool isInputHold = Input.GetKey( GameSetting.Inst.Keys[key] );

            float endDiff = currentNote.SliderTime - NowPlaying.Playback;
            var endType   = judge.GetJudgeType( endDiff );

            if ( isInputHold )
            {
                if ( endType != JudgeType.None && endType == JudgeType.Miss )
                {
                    judge.OnJudgement( JudgeType.Miss );
                    SelectNextNote();
                    return;
                }

                playback += Time.deltaTime;
                if ( playback > .1f )
                {
                    OnHitNote();
                    judge.OnJudgement( JudgeType.None );
                    playback = 0f;
                }
            }

            bool isInputUp = Input.GetKeyUp( GameSetting.Inst.Keys[key] );
            if ( isInputUp )
            {
                if ( endType != JudgeType.None && endType != JudgeType.Miss )
                {
                    OnHitNote();
                    judge.OnJudgement( endType );
                    SelectNextNote();
                }
                else if ( endType == JudgeType.None || endType == JudgeType.Miss )
                {
                    currentNote.SetBodyFail();
                    judge.OnJudgement( JudgeType.Miss );
                    sliderMissQueue.Enqueue( currentNote );
                    SelectNextNote( false );
                }
            }
        }
    }

    private void LateUpdate()
    {
        bool isInputDown = Input.GetKeyDown( GameSetting.Inst.Keys[key] );
        bool isInputUp   = Input.GetKeyUp( GameSetting.Inst.Keys[key] );

        if ( isInputDown )    OnInputEvent?.Invoke( true );
        else if ( isInputUp ) OnInputEvent?.Invoke( false );

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

        if ( currentNote != null )
             NoteProcessAction();
    }
}
