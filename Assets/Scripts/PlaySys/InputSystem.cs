using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSystem : MonoBehaviour
{
    public struct InputData
    {
        public InputType type;
        public double time;

        public InputData( InputType _type, double _time )
        {
            type = _type;
            time = _time;
        }
    }

    #region Variables
    #region Objects
    private Lane lane;
    private InGame scene;
    private Judgement judge;
    #endregion
    private Queue<NoteRenderer> notes           = new Queue<NoteRenderer>();
    private Queue<NoteRenderer> sliderMissQueue = new Queue<NoteRenderer>();
    private NoteRenderer curNote;

    public event Action<NoteType, InputType> OnHitNote;
    public event Action<InputType>           OnInputEvent;

    private KeyCode key;
    private KeySound curSound;
    private bool isAuto, isReady, isPress;

    #region AutoPlay
    private NoteType autoNoteType;
    private double autoEffectDuration;
    private float autoHoldTime;
    private float rand;
    private bool isAutoTimeStart;
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
        scene.OnGameStart       += StartProcess;
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


        if ( isAuto )
        {
            AutoCheckNote();
        }
        else
        {
            if ( Input.GetKeyDown( key ) )
            {
                OnInputEvent?.Invoke( InputType.Down );
                SoundManager.Inst.Play( curSound );
            }
            else if ( Input.GetKeyUp( key ) )
            {
                OnInputEvent?.Invoke( InputType.Up );
            }

            CheckNote();
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

    private void StartProcess()
    {
        StartCoroutine( NoteSelect() );
        StartCoroutine( SliderMissCheck() );
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
        curNote  = null;
        isPress  = false;
        curSound = new KeySound();
        autoEffectDuration = 0;
        autoHoldTime       = 0;
    }

    /// <summary>
    /// process the slider when pausing, it will be judged immediately.
    /// </summary>
    private void Pause( bool _isPause )
    {
        isReady = !_isPause;
        OnInputEvent?.Invoke( InputType.Up );

        if ( !_isPause || curNote == null || !curNote.IsSlider ) 
             return;

        if ( isAuto )
        {
            OnHitNote?.Invoke( NoteType.Slider, InputType.Up );
            judge.ResultUpdate( HitResult.Perfect, NoteType.Slider );
            SelectNextNote();
        }
        else
        {
            curNote.SetBodyFail();
            OnHitNote?.Invoke( NoteType.Slider, InputType.Up );
            judge.ResultUpdate( HitResult.Miss, NoteType.Slider );
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
    private IEnumerator NoteSelect()
    {
        var WaitNote = new WaitUntil( () => curNote == null && notes.Count > 0 );
        while ( true )
        {
            yield return WaitNote;
            curNote  = notes.Dequeue();
            curSound = curNote.Sound;

            double nextAutoTime = notes.Count > 0 ? notes.Peek().Time : curNote.Time + .0651d;
            double offset       = Global.Math.Abs( nextAutoTime - curNote.Time );
            autoEffectDuration  = offset > .065d ? .065d : Global.Math.Lerp( 0.01d, offset, .5d );
        }
    }

    private IEnumerator SliderMissCheck()
    {
        var WaitEnqueue  = new WaitUntil( () => sliderMissQueue.Count > 0 );
        while ( true )
        {
            yield return WaitEnqueue;
            
            var note = sliderMissQueue.Peek();
            if ( note.TailPos < -640f )
            {
                note.Despawn();
                sliderMissQueue.Dequeue();
            }
        }
    }

    private void SelectNextNote( bool _isDespawn = true )
    {
        if ( _isDespawn )
        {
            curNote.gameObject.SetActive( false );
            curNote.Despawn();
        }

        curNote = null;
        isPress = false;
    }

    private void AutoCheckNote()
    {
        if ( isAutoTimeStart )
        {
            autoHoldTime += Time.deltaTime;
            if ( autoNoteType == NoteType.Default && autoHoldTime > autoEffectDuration )
            {
                OnInputEvent?.Invoke( InputType.Up );
                isAutoTimeStart = false;
            }
        }

        if ( curNote == null ) return;

        double startDiff = curNote.Time - NowPlaying.Playback;
        double endDiff   = curNote.SliderTime - NowPlaying.Playback;

        if ( !curNote.IsSlider )
        {
            if ( GameSetting.IsAutoRandom ? startDiff < rand : startDiff < 0d )
            {
                rand = UnityEngine.Random.Range( ( float )( -Judgement.Bad ), ( float )( Judgement.Bad ) );
                autoNoteType = NoteType.Default;
                autoHoldTime = 0f;
                OnInputEvent?.Invoke( InputType.Down );
                isAutoTimeStart = true;

                OnHitNote?.Invoke( NoteType.Default, InputType.Down );
                judge.ResultUpdate( startDiff, NoteType.Default );
                SoundManager.Inst.Play( curSound );
                SelectNextNote();
            }
        }
        else
        {
            if ( !isPress )
            {
                if ( startDiff <= 0d )
                {
                    autoNoteType = NoteType.Slider;
                    autoHoldTime = 0f;
                    OnInputEvent?.Invoke( InputType.Down );

                    isPress = curNote.ShouldResizeSlider = true;
                    OnHitNote?.Invoke( NoteType.Slider, InputType.Down );
                    SoundManager.Inst.Play( curSound );
                    judge.ResultUpdate( 0d, NoteType.Default );

                    inputStartTime = curNote.Time;
                }
            }
            else
            {
                if ( endDiff <= 0d )
                {
                    autoHoldTime = 0f;
                    OnInputEvent?.Invoke( InputType.Up );

                    OnHitNote?.Invoke( NoteType.Slider, InputType.Up );
                    judge.ResultUpdate( 0d, NoteType.Slider );
                    SelectNextNote();
                }

                inputHoldTime = NowPlaying.Playback - inputStartTime;
                if ( inputHoldTime > .1f )
                {
                    judge.ResultUpdate( HitResult.None, NoteType.None );
                    inputStartTime = NowPlaying.Playback - ( inputHoldTime - .1f );
                }
            }
        }
    }

    private void CheckNote()
    {
        if ( curNote == null ) return;

        double startDiff = curNote.Time - NowPlaying.Playback;
        if ( !curNote.IsSlider )
        {
            if ( Input.GetKeyDown( key ) && judge.CanBeHit( startDiff ) )
            {
                OnHitNote?.Invoke( NoteType.Default, InputType.Down );
                judge.ResultUpdate( startDiff, NoteType.Default );
                SelectNextNote();
                return;
            }

            if ( judge.IsMiss( startDiff ) )
            {
                judge.ResultUpdate( HitResult.Miss, NoteType.Default );
                SelectNextNote();
            }
        }
        else
        {
            if ( !isPress )
            {
                if ( Input.GetKeyDown( key ) && judge.CanBeHit( startDiff ) )
                {
                    isPress = curNote.ShouldResizeSlider = true;
                    OnHitNote?.Invoke( NoteType.Slider, InputType.Down );
                    judge.ResultUpdate( startDiff, NoteType.Default );

                    inputStartTime = curNote.Time;
                    return;
                }

                if ( judge.IsMiss( startDiff ) )
                {
                    curNote.SetBodyFail();
                    judge.ResultUpdate( HitResult.Miss, NoteType.Slider, 2 );
                    sliderMissQueue.Enqueue( curNote );
                    SelectNextNote( false );
                }
            }
            else
            {
                double endDiff = curNote.SliderTime - NowPlaying.Playback;
                if ( endDiff < 0 )
                {
                    judge.ResultUpdate( endDiff, NoteType.Slider );
                    OnHitNote?.Invoke( NoteType.Slider, InputType.Up );
                    SelectNextNote();
                    return;
                }

                inputHoldTime = NowPlaying.Playback - inputStartTime;
                if ( inputHoldTime > .1f )
                {
                    judge.ResultUpdate( HitResult.None, NoteType.None );
                    inputStartTime = NowPlaying.Playback - ( inputHoldTime - .1f );
                }
                
                if ( Input.GetKeyUp( key ) )
                {
                    if ( judge.CanBeHit( endDiff ) )
                    {
                        OnHitNote?.Invoke( NoteType.Slider, InputType.Up );
                        judge.ResultUpdate( endDiff, NoteType.Slider );
                        SelectNextNote();
                    }
                    else
                    {
                        curNote.SetBodyFail();
                        judge.ResultUpdate( HitResult.Miss, NoteType.Slider );
                        sliderMissQueue.Enqueue( curNote );
                        SelectNextNote( false );
                    }
                }
            }
        }
    }
    #endregion
}
