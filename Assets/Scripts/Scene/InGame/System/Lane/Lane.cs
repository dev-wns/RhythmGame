using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class Lane : LaneBase
{
    public InputSystem  InputSys { get; private set; }
    public NoteSpawner  Spawner  { get; private set; }
    public LaneEffector Effector { get; private set; }

    public  Queue<HitData> dataQueue = new ();

    public event Action<double> OnUpdate;

    private void Awake()
    {
        NowPlaying.OnUpdate    += UpdateLane;
        NowPlaying.OnGameOver  += GameOver;
        NowPlaying.OnPause     += Pause;
    }

    private void OnDestroy()
    {
        NowPlaying.OnUpdate    -= UpdateLane;
        NowPlaying.OnGameOver  -= GameOver;
        NowPlaying.OnPause     -= Pause;
    }

    public override void Initialize( int _lane )
    {
        base.Initialize( _lane );

        if ( !TryGetComponent( out InputSystem inputSys ) )
             Debug.LogError( "InputSystem is not Created" );

        InputSys = inputSys;
        InputSys.Initialize( this );

        if ( !TryGetComponent( out NoteSpawner spawner ) )
             Debug.Log( "Note Spawner is not Created" );

        Spawner = spawner;
        Spawner.Initialize( this );

        if ( !TryGetComponent( out LaneEffector effector ) )
             Debug.Log( "Lane Effector is not Created" );

        Effector = effector;
        Effector.Initialize( this );
    }

    private void UpdateLane( double _playback )
    {
        Spawner.SpawnNotes( _playback );
        Effector.UpdateEffect();

        CheckHitData();
    }

    private void CheckHitData()
    {
        NoteRenderer curNote = Spawner.CurrentNote;
        if ( curNote == null || !dataQueue.TryDequeue( out HitData data ) )
             return;

        if ( data.hitResult >= 0 ) // Hit
        {
            if ( !curNote.IsKeyDown )
            {
                Effector.SetHitEffect( KeyState.Down, curNote.IsSlider );
                if ( curNote.IsSlider ) curNote.IsKeyDown = true;
                else                    Spawner.Despawn();
            }
            else
            {
                Effector.SetHitEffect( KeyState.Up, curNote.IsSlider );
                Spawner.AddEarlyQueue();
            }
        }
        else if ( data.hitResult == HitResult.Miss ) // Miss
        {
            Effector.SetHitEffect( KeyState.Up, curNote.IsSlider );
            if ( !curNote.IsSlider )
            {
                Spawner.Despawn();
            }
            else
            {
                curNote.SetSliderFail();
                Spawner.AddMissQueue();
            }
        }
    }

    public void AddData( in HitData _data ) => dataQueue.Enqueue( _data );

    public void SetNotes( ReadOnlyCollection<Note> _datas )
    {
        InputSys.SetNotes( _datas );
        Spawner.SetNotes( _datas );
    }

    public void Clear()
    {
        InputSys.Clear();
        Spawner.Clear();
        Effector.Clear();

        dataQueue.Clear();
    }

    private void GameOver()
    {
        Effector.Clear();

        NoteRenderer curNote = Spawner.CurrentNote;
        if ( curNote != null )
             curNote.IsKeyDown = false;
    }

    private void Pause( bool _isPause )
    {
        Effector.Clear();
        // ClearEffect();
        //laneRenderer.color = new Color( laneColor.r, laneColor.g, laneColor.b, laneAlpha = 0f );
        //if ( !_isPause || curNote == null || !curNote.IsSlider )
        //     return;

        //if ( GameSetting.HasFlag( GameMode.AutoPlay ) )
        //{
        //    //judge.ResultUpdate( HitResult.Perfect, NoteType.Slider );
        //    SelectNextNote();
        //}
        //else
        //{
        //    curNote.SetSliderFail();
        //    //judge.ResultUpdate( HitResult.Miss, NoteType.Slider );
        //    sliderMissQueue.Enqueue( curNote );
        //    SelectNextNote( false );
        //}
    }
}
