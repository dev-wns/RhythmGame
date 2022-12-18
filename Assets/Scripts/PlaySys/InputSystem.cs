using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSystem : MonoBehaviour
{
    #region Variables
    #region Objects
    private Lane lane;
    private InGame scene;
    private Judgement judge;
    #endregion
    private Queue<NoteRenderer> notes           = new Queue<NoteRenderer>();
    private Queue<NoteRenderer> sliderMissQueue = new Queue<NoteRenderer>();
    private NoteRenderer curNote;

    public event Action<NoteType, bool/*Key Up*/> OnHitNote;
    public event Action<bool/*Key Down*/>         OnInputEvent;

    private KeyCode key;
    private KeySound curSound;
    private bool isAuto, isReady, shouldFindNextNote = true;

    #region AutoPlay
    private NoteType autoNoteType;
    private double autoEffectDuration;
    private float autoHoldTime;
    private float rand;
    #endregion
    #region Time
    private double inputStartTime;
    private double inputHoldTime;
    #endregion
    #endregion

    #region Unity Event Function

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        //scene.OnGameStart += () => isReady = true;
        scene.OnReLoad          += ReLoad;
        NowPlaying.Inst.OnPause += Pause;

        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();

        lane  = GetComponent<Lane>(); 
        lane.OnLaneInitialize += Initialize;

        isReady = false;
        isAuto  = GameSetting.CurrentGameMode.HasFlag( GameMode.AutoPlay );
        rand = UnityEngine.Random.Range( ( float )( -Judgement.Bad ), ( float )( Judgement.Bad ) );
    }

    private void LateUpdate()
    {
        if ( !isReady ) return;

        if ( shouldFindNextNote && notes.Count > 0 )
        {
            curNote = notes.Peek();
            curSound = curNote.Sound;
            shouldFindNextNote = false;
        }

        if ( !shouldFindNextNote )
        {
            if ( curNote.IsSlider ) CheckSlider();
            else                    CheckNote();
        }

        if ( isAuto )
        {
            autoHoldTime += Time.deltaTime;
            if ( autoNoteType == NoteType.Default && autoHoldTime > autoEffectDuration )
                 OnInputEvent?.Invoke( false );
        }
        else
        {
            if ( Input.GetKeyDown( key ) )
            {
                OnInputEvent?.Invoke( true );
                SoundManager.Inst.Play( curSound );
            }
            else if ( Input.GetKeyUp( key ) )
            {
                OnInputEvent?.Invoke( false );
            }
        }

        if ( sliderMissQueue.Count > 0 )
        {
            var note = sliderMissQueue.Peek();
            if ( judge.IsMiss( note.SliderTime - NowPlaying.Playback ) )
            {
                note.Despawn();
                sliderMissQueue.Dequeue();
            }
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        NowPlaying.Inst.OnPause -= Pause;
    }
    #endregion

    #region Initialize
    public void AddNote( NoteRenderer _note ) => notes.Enqueue( _note );

    public void SetSound( in KeySound _sound ) => curSound = _sound;
    #endregion

    #region Event
    public void Initialize( int _key )
    {
        key = KeySetting.Inst.Keys[( GameKeyCount )NowPlaying.CurrentSong.keyCount][_key];
        isReady = true;
    }

    private void ReLoad()
    {
        StopAllCoroutines();
        while ( sliderMissQueue.Count > 0 )
        {
            var note = sliderMissQueue.Dequeue();
            note.Despawn();
        }

        while ( notes.Count > 0 )
        {
            var note = notes.Dequeue();
            note.Despawn();
        }

        judge.ReLoad();
        shouldFindNextNote = true;
        curSound = new KeySound();
        autoEffectDuration = 0;
        autoHoldTime = 0;
    }

    /// <summary>
    /// process the slider when pausing, it will be judged immediately.
    /// </summary>
    private void Pause( bool _isPause )
    {
        isReady = !_isPause;
        OnInputEvent?.Invoke( false );

        if ( !_isPause || curNote == null || !curNote.IsSlider || !curNote.IsPressed ) 
             return;

        if ( isAuto )
        {
            curNote.IsPressed = false;
            OnHitNote?.Invoke( NoteType.Slider, true );
            judge.ResultUpdate( HitResult.Perfect );
            SelectNextNote();
        }
        else
        {
            curNote.IsPressed = false;
            curNote.SetBodyFail();
            OnHitNote?.Invoke( NoteType.Slider, true );
            judge.ResultUpdate( HitResult.Miss );
            sliderMissQueue.Enqueue( curNote );
            SelectNextNote( false );
        }
    }
    #endregion

    #region Note Process
    /// <summary>
    /// Find the next note in the current lane.
    /// </summary>
    /// <returns></returns>
    //private IEnumerator NoteSelect()
    //{
    //    var WaitNote = new WaitUntil( () => curNote == null && notes.Count > 0 );
    //    while ( true )
    //    {
    //        yield return WaitNote;
    //        curNote = notes.Dequeue();
    //        curSound = curNote.Sound;

    //        double nextAutoTime = notes.Count > 0 ? notes.Peek().Time : curNote.Time + .0651d;
    //        double offset = Global.Math.Abs( nextAutoTime - curNote.Time );
    //        autoEffectDuration = offset > .065d ? .065d : Global.Math.Lerp( 0.01d, offset, .5d );
    //    }
    //}

    private void SelectNextNote( bool _isDespawn = true )
    {
        if ( _isDespawn )
        {
            curNote.gameObject.SetActive( false );
            curNote.Despawn();
        }

        notes.Dequeue();
        shouldFindNextNote = true;

        autoEffectDuration = .065d;
        if ( isAuto && notes.Count > 0 )
        {
            double nextAutoTime = notes.Count > 0 ? notes.Peek().Time : curNote.Time + .0651d;
            double offset = Global.Math.Abs( nextAutoTime - curNote.Time );
            autoEffectDuration = offset > .065d ? .065d : Global.Math.Lerp( 0.01d, offset, .5d );
        }
    }

    private void CheckNote()
    {
        double startDiff = curNote.Time - NowPlaying.Playback;
        if ( isAuto )
        {
            //rand = UnityEngine.Random.Range( ( float )( -Judgement.Bad + .01d ), ( float )( Judgement.Bad - .01d ) );
            bool isHit = GameSetting.IsAutoRandom ? startDiff <= rand : startDiff <= 0d;
            if ( isHit )
            {
                rand = UnityEngine.Random.Range( ( float )( -Judgement.Bad ), ( float )( Judgement.Bad ) );
                autoNoteType = NoteType.Default;
                autoHoldTime = 0f;
                OnInputEvent?.Invoke( true );

                OnHitNote?.Invoke( NoteType.Default, false );
                judge.ResultUpdate( startDiff );
                SoundManager.Inst.Play( curSound );
                SelectNextNote();
            }
        }
        else {
            if ( judge.CanBeHit( startDiff ) && Input.GetKeyDown( key ) )
            {
                OnHitNote?.Invoke( NoteType.Default, false );
                judge.ResultUpdate( startDiff );
                SelectNextNote();
                return;
            }

            if ( judge.IsMiss( startDiff ) ) {
                judge.ResultUpdate( HitResult.Miss );
                SelectNextNote();
            }
        }
    }

    private void CheckSlider()
    {
        double startDiff = curNote.Time       - NowPlaying.Playback;
        double endDiff   = curNote.SliderTime - NowPlaying.Playback;
        if ( isAuto )
        {
            if ( !curNote.IsPressed )
            {
                if ( startDiff <= 0d )
                {
                    autoNoteType = NoteType.Slider;
                    autoHoldTime = 0f;
                    OnInputEvent?.Invoke( true );

                    curNote.IsPressed = true;
                    OnHitNote?.Invoke( NoteType.Slider, false );
                    SoundManager.Inst.Play( curSound );
                    //SoundManager.Inst.Play( SoundSfxType.Clap );
                    judge.ResultUpdate( 0d );

                    inputStartTime = curNote.Time;
                }
            }
            else
            {
                if ( endDiff <= 0d )
                {
                    autoHoldTime = 0f;
                    OnInputEvent?.Invoke( false );

                    OnHitNote?.Invoke( NoteType.Slider, true );
                    judge.ResultUpdate( 0d );
                    SelectNextNote();
                }

                inputHoldTime = NowPlaying.Playback - inputStartTime;
                if ( inputHoldTime > .1f )
                {
                    judge.ResultUpdate( HitResult.None );
                    inputStartTime = NowPlaying.Playback - ( inputHoldTime - .1f );
                }
            }
        }
        else
        {
            if ( !curNote.IsPressed )
            {
                if ( judge.CanBeHit( startDiff ) && Input.GetKeyDown( key ) )
                {
                    curNote.IsPressed = true;
                    OnHitNote?.Invoke( NoteType.Slider, false );
                    judge.ResultUpdate( startDiff );

                    inputStartTime = curNote.Time;
                    return;
                }

                if ( judge.IsMiss( startDiff ) )
                {
                    curNote.SetBodyFail();
                    judge.ResultUpdate( HitResult.Miss );
                    sliderMissQueue.Enqueue( curNote );
                    SelectNextNote( false );
                }
            }
            else
            {
                if ( Input.GetKey( key ) )
                {
                    if ( endDiff <= 0d )
                    {
                        curNote.IsPressed = false;
                        judge.ResultUpdate( endDiff );
                        OnHitNote?.Invoke( NoteType.Slider, true );
                        SelectNextNote();
                        return;
                    }

                    inputHoldTime = NowPlaying.Playback - inputStartTime;
                    if ( inputHoldTime > .1f )
                    {
                        judge.ResultUpdate( HitResult.None );
                        inputStartTime = NowPlaying.Playback - ( inputHoldTime - .1f );
                    }
                }

                if ( Input.GetKeyUp( key ) )
                {
                    OnHitNote?.Invoke( NoteType.Slider, true );

                    if ( judge.CanBeHit( endDiff ) )
                    {
                        judge.ResultUpdate( endDiff );
                        SelectNextNote();
                    }
                    else
                    {
                        curNote.IsPressed = false;
                        curNote.SetBodyFail();
                        judge.ResultUpdate( HitResult.Miss );
                        sliderMissQueue.Enqueue( curNote );
                        SelectNextNote( false );
                    }
                }
            }
        }
    }
    #endregion
}
