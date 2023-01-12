using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NoteType { None, Default, Slider }

public class InputSystem : MonoBehaviour
{
    #region Variables
    #region Objects
    private Lane lane;
    private InGame scene;
    private Judgement judge;
    #endregion

    #region Note
    private ObjectPool<NoteRenderer> notePool;
    public NoteRenderer note1 /* Lane 0,2,3,5 */, note2 /* Lane 1,4 */, noteMedian;

    private List<Note> noteDatas = new List<Note>();
    private int noteSpawnIndex;
    #endregion

    private Queue<NoteRenderer> notes           = new Queue<NoteRenderer>();
    private Queue<NoteRenderer> sliderMissQueue = new Queue<NoteRenderer>();
    private NoteRenderer curNote;

    public event Action<NoteType, InputType> OnHitNote;
    public event Action<InputType> OnInputEvent;

    private KeyCode key;
    private KeySound curSound;
    private bool isAuto, isReady, isPress;

    private float rand;
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

        isAuto = GameSetting.CurrentGameMode.HasFlag( GameMode.AutoPlay );
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

        NoteRenderer note = note1;
        if ( NowPlaying.CurrentSong.keyCount == 4 )      note = _key == 1 || _key == 2 ? note2 : note1;
        else if ( NowPlaying.CurrentSong.keyCount == 6 ) note = _key == 1 || _key == 4 ? note2 : note1;
        else if ( NowPlaying.CurrentSong.keyCount == 7 ) note = _key == 1 || _key == 5 ? note2 : _key == 3 ? noteMedian : note1;
        notePool ??= new ObjectPool<NoteRenderer>( note, 5 );

        isReady = true;
    }

    private void StartProcess()
    {
        StartCoroutine( NoteSpawn() );
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

        noteSpawnIndex = 0;
    }

    public void AddNote( in Note _note ) => noteDatas.Add( _note );

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
    private IEnumerator NoteSpawn()
    {
        Note curData = new Note();
        if ( noteDatas.Count > 0 )
        {
            curData = noteDatas[noteSpawnIndex];
            curSound = curData.keySound;
        }

        WaitUntil waitNextNote = new WaitUntil( () => curData.calcTime <= NowPlaying.PlaybackInBPM + GameSetting.PreLoadTime );
        while ( noteSpawnIndex < noteDatas.Count )
        {
            yield return waitNextNote;

            NoteRenderer note = notePool.Spawn();
            note.SetInfo( lane.Key, in curData, noteSpawnIndex );
            notes.Enqueue( note );

            if ( ++noteSpawnIndex < noteDatas.Count )
                 curData = noteDatas[noteSpawnIndex];
        }
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
            curNote  = notes.Dequeue();
            curSound = curNote.Sound;
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
        if ( curNote == null ) return;

        double startDiff = curNote.Time - NowPlaying.Playback;
        double endDiff   = curNote.SliderTime - NowPlaying.Playback;

        if ( !curNote.IsSlider )
        {
            rand = UnityEngine.Random.Range( ( float )( -Judgement.Bad ), ( float )( Judgement.Bad ) );
            if ( GameSetting.IsAutoRandom ? startDiff < rand : startDiff < 0d )
            {
                OnInputEvent?.Invoke( InputType.Down );
                OnInputEvent?.Invoke( InputType.Up );

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
                if ( inputHoldTime > .1d )
                {
                    judge.ResultUpdate( HitResult.None, NoteType.None );
                    inputStartTime = NowPlaying.Playback - ( inputHoldTime - .1d );
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
