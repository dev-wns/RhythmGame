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
    private NoteRenderer curNote;

    private Queue<NoteRenderer> sliderMissQueue = new Queue<NoteRenderer>();
    public event Action<bool> OnInputEvent;
    public event Action OnHitNote;

    private GameKeyAction key;
    private float playback;

    private Action NoteProcessAction;
    private bool isAuto;
    public void Enqueue( NoteRenderer _note ) => notes.Enqueue( _note );

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnGameStart += () => StartCoroutine( NoteSelect() );
        scene.OnPause += DuringPauseProcess;

        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();

        lane  = GetComponent<Lane>(); 
        lane.OnLaneInitialize += _key => key = ( GameKeyAction )_key;

        isAuto = GameSetting.CurrentGameMode.HasFlag( GameMode.AutoPlay );
        if ( isAuto )
        {
            NoteProcessAction = () =>
            {
                if ( curNote.IsSlider ) AutoCheckSlider();
                else                    AutoCheckNote();
            };
        }
        else
        {
            NoteProcessAction = () =>
            {
                if ( curNote.IsSlider ) CheckSlider();
                else                    CheckNote();
            };
        }
    }

    /// <summary>
    /// process the slider when pausing, it will be judged immediately.
    /// </summary>
    private void DuringPauseProcess()
    {
        if ( curNote == null || !curNote.IsSlider || !curNote.isHolding ) 
             return;

        if ( isAuto )
        {
            OnHitNote();
            judge.ResultUpdate( HitResult.Perfect );
            SelectNextNote();
        }
        else
        {
            OnHitNote();
            judge.ResultUpdate( curNote.SliderTime - NowPlaying.Playback );
            SelectNextNote();
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    /// <summary>
    /// Find the next note in the current lane.
    /// </summary>
    /// <returns></returns>
    private IEnumerator NoteSelect()
    {
        var WaitNote = new WaitUntil( () => curNote == null && notes.Count > 0 );
        while ( true )
        {
            yield return WaitNote;
            curNote = notes.Dequeue();
        }
    }

    private void SelectNextNote( bool _isDespawn = true )
    {
        playback = 0f;

        if ( _isDespawn ) curNote.Despawn();
        curNote = null;
    }

    private void AutoCheckNote()
    {
        double startDiff = curNote.Time - NowPlaying.Playback;

        if ( startDiff <= 0f )
        {
            OnHitNote();
            judge.ResultUpdate( startDiff );
            SelectNextNote();
        }
    }

    private void AutoCheckSlider()
    {
        if ( !curNote.isHolding )
        {
            double startDiff = curNote.Time - NowPlaying.Playback;
            if ( startDiff <= 0f )
            {
                curNote.isHolding = true;
                OnHitNote();
                judge.ResultUpdate( startDiff );
            }
        }
        else
        {
            double endDiff = curNote.SliderTime - NowPlaying.Playback;
            if ( endDiff <= 0f )
            {
                OnHitNote();
                judge.ResultUpdate( endDiff );
                SelectNextNote();
            }

            playback += Time.deltaTime;
            if ( playback > .1f )
            {
                OnHitNote();
                judge.ResultUpdate( HitResult.None );
                playback = 0f;
            }
        }
    }

    private void CheckNote()
    {
        double startDiff = curNote.Time - NowPlaying.Playback;
        if ( judge.CanBeHit( startDiff ) && Input.GetKeyDown( GameSetting.Inst.Keys[key] ) )
        {
            OnHitNote();
            judge.ResultUpdate( startDiff );
            SelectNextNote();
            return;
        }

        if( judge.IsMiss( startDiff ) )
        {
            judge.ResultUpdate( HitResult.Miss );
            SelectNextNote();
        }        
    }

    private void CheckSlider()
    {
        if ( !curNote.isHolding )
        {
            double startDiff = curNote.Time - NowPlaying.Playback;

            if ( judge.CanBeHit( startDiff ) && Input.GetKeyDown( GameSetting.Inst.Keys[key] ) )
            {
                curNote.isHolding = true;
                OnHitNote();
                judge.ResultUpdate( startDiff );
                return;
            }

            if ( judge.IsMiss( startDiff ) )
            {
                curNote.SetBodyFail();
                judge.ResultUpdate( HitResult.Miss );
                sliderMissQueue.Enqueue( curNote );
                SelectNextNote( false );
                return;
            }
        }

        if ( curNote.isHolding )
        {
            double endDiff = curNote.SliderTime - NowPlaying.Playback;
            if ( Input.GetKey( GameSetting.Inst.Keys[key] ) )
            { 
                if ( endDiff <= 0f )
                { 
                    judge.ResultUpdate( endDiff );
                    SelectNextNote();
                    return;
                }

                playback += Time.deltaTime;
                if ( playback > .1f )
                {
                    OnHitNote();
                    judge.ResultUpdate( HitResult.None );
                    playback = 0f;
                }
            }

            if ( Input.GetKeyUp( GameSetting.Inst.Keys[key] ) )
            {
                if ( judge.CanBeHit( endDiff ) )
                {
                    OnHitNote();
                    judge.ResultUpdate( endDiff );
                    SelectNextNote();
                }
                else
                {
                    curNote.SetBodyFail();
                    judge.ResultUpdate( HitResult.Miss );
                    sliderMissQueue.Enqueue( curNote );
                    SelectNextNote( false );
                }
            }
        }
    }

    private void LateUpdate()
    {
        if ( Input.GetKeyDown( GameSetting.Inst.Keys[key] ) )    OnInputEvent?.Invoke( true );
        else if ( Input.GetKeyUp( GameSetting.Inst.Keys[key] ) ) OnInputEvent?.Invoke( false );

        if ( sliderMissQueue.Count > 0 )
        {
            var slider = sliderMissQueue.Peek();
            if ( judge.IsMiss( slider.SliderTime - NowPlaying.Playback ) )
            {
                slider.Despawn();
                sliderMissQueue.Dequeue();
            }
        }

        if ( curNote != null )
             NoteProcessAction();
    }
}
