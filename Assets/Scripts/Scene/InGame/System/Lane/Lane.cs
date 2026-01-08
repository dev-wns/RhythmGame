using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class Lane : LaneBase
{
    //private InGame scene;
    public InputSystem  InputSys { get; private set; }
    public NoteSpawner  Spawner  { get; private set; }
    public LaneEffector Effector { get; private set; }

    public  Queue<HitData> dataQueue = new ();

    private void Awake()
    {
        //NowPlaying.OnUpdate    += UpdateLane;
        //NowPlaying.OnGameOver  += GameOver;
        //NowPlaying.OnPause     += Pause;
        //NowPlaying.OnClear     += Clear;
    }

    private void OnDestroy()
    {
        //NowPlaying.OnUpdate    -= UpdateLane;
        //NowPlaying.OnGameOver  -= GameOver;
        //NowPlaying.OnPause     -= Pause;
        //scene.OnClear     -= Clear;
    }

    public override void Initialize( int _lane, int _keyCount )
    {
        base.Initialize( _lane, _keyCount );

        if ( !TryGetComponent( out InputSystem inputSys ) )
             Debug.LogError( "InputSystem is not Created" );

        InputSys = inputSys;
        InputSys.Initialize( this );

        if ( !TryGetComponent( out NoteSpawner spawner ) )
             Debug.Log( "Note Spawner is not Created" );

        Spawner = spawner;
        Spawner.Initialize( this, _keyCount );

        if ( !TryGetComponent( out LaneEffector effector ) )
             Debug.Log( "Lane Effector is not Created" );

        Effector = effector;
        Effector.Initialize( this, _keyCount );
    }

    private void UpdateLane( double _playback, double _distance )
    {
        Spawner.SpawnNotes( _playback, _distance );
        Effector.UpdateEffect();

        //CheckHitData();
    }

    private void Update()
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

    public void GameOver()
    {
        Effector.Clear();

        NoteRenderer curNote = Spawner.CurrentNote;
        if ( curNote != null )
             curNote.IsKeyDown = false;
    }

    public void Pause( bool _isPause )
    {
        //Spawner.Pause( _isPause );
        InputSys.Pause( _isPause );
        Effector.Clear();

        //if ( _isPause )
        //{
        //    while ( dataQueue.Count > 0 )
        //            CheckHitData();
        //}

        // NoteRenderer curNote = Spawner.CurrentNote;
        // if ( curNote != null )
        // {
        //     if ( curNote.IsKeyDown )
        //          curNote.IsKeyDown = false;
        //     //curNote.Snap();
        // }

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
