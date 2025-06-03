using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public struct InputData
{
    public double    time;
    public double    diff;
    public KeyState  keyState;
    public NoteState noteState;

    public InputData( double _time, double _diff, KeyState _keyState = KeyState.None, NoteState _noteState = NoteState.None )
    {
        time      = _time;
        diff      = _diff;
        keyState  = _keyState;
        noteState = _noteState;
    }
}
public enum NoteState { None, Hit, Miss, }

public class InputManager : Singleton<InputManager>
{
    #region Lane
    [Header( "Lane" )]
    [SerializeField] Lane       prefab;
    [SerializeField] List<Lane> lanes = new();
    #endregion

    #region Thread
    [Header( "Thread" )]
    private CancellationTokenSource cancelSource = new();
    [DllImport( "user32.dll" )]
    private static extern short GetAsyncKeyState( int _vKey );
    #endregion

    private void Start()
    {
        //await Task.Run( () => Process( cancelSource.Token ) );

        GameManager.Inst.LoadAssetsAsync( "Lane", ( GameObject _lane ) => 
        {
            if ( !_lane.TryGetComponent( out prefab ) )
                 Debug.LogError( "Prefab Loading Failed( Lane )" );
        } );
    }

    public void Connect( Lane _lane )
    {
        lanes.Add( _lane );
        Debug.Log( $"Connect {_lane.UKey} Lane" );
    }

    public void Stop()
    {
        //systems.Clear();
    }

    private void Update()
    {

        return;
    }

    private void OnDestroy()
    {
        cancelSource?.Cancel();
    }

    private void OnApplicationQuit()
    {
        cancelSource?.Cancel();
    }

    //private async void Process( CancellationToken _token )
    //{
    //    int index = 0;
    //    KeyState keyState = KeyState.None;
    //    bool isEntry = false; // 하나의 입력으로 하나의 노트만 처리하기위한 노트 진입점
    //    Debug.Log( " Input Process Start " );
    //    try
    //    {
    //        while ( !_token.IsCancellationRequested )
    //        {
    //            if ( index >= noteDatas.Count )
    //                break;

    //            double startDiff = noteDatas[index].time       - NowPlaying.Playback;
    //            double endDiff   = noteDatas[index].sliderTime - NowPlaying.Playback;

    //            if ( !isEntry && Judgement.IsMiss( startDiff ) )
    //            {
    //                isEntry = false;
    //                index = ++index;
    //                inputQueue.Enqueue( new InputData( NowPlaying.Playback, startDiff, KeyState.None, NoteState.Miss ) );

    //                continue;
    //            }

    //            KeyState previous = keyState;
    //            if ( ( GetAsyncKeyState( vKey ) & 0x8000 ) != 0 )
    //            {
    //                keyState = previous == KeyState.None || previous == KeyState.Up ? KeyState.Down : KeyState.Hold;
    //                if ( !isEntry && keyState == KeyState.Down )
    //                {
    //                    AudioManager.Inst.Play( noteDatas[index].keySound );

    //                    if ( Judgement.CanBeHit( startDiff ) )
    //                    {
    //                        // 일반노트는 끝판정 처리를 무시한다.
    //                        isEntry = noteDatas[index].isSlider;
    //                        index = noteDatas[index].isSlider ? index : ++index;

    //                        inputQueue.Enqueue( new InputData( NowPlaying.Playback, startDiff, KeyState.Down, NoteState.Hit ) );
    //                    }
    //                }

    //                if ( isEntry && keyState == KeyState.Hold && endDiff < 0d )
    //                {
    //                    isEntry = false;
    //                    index = ++index;
    //                    inputQueue.Enqueue( new InputData( NowPlaying.Playback, 0d, KeyState.None, NoteState.Hit ) );
    //                }
    //            }
    //            else
    //            {
    //                keyState = previous == KeyState.Down || previous == KeyState.Hold ? KeyState.Up : KeyState.None;

    //                if ( isEntry && keyState == KeyState.Up )
    //                {
    //                    isEntry = false;
    //                    index = ++index;
    //                    inputQueue.Enqueue( new InputData( NowPlaying.Playback, endDiff, KeyState.Up,
    //                                                       Judgement.CanBeHit( endDiff ) ? NoteState.Hit : NoteState.Miss ) );
    //                }
    //            }
    //        }

    //        await Task.Delay( 1 );
    //    }
    //    catch ( Exception _ex )
    //    {
    //        Debug.Log( _ex.Message );
    //    }

    //    Debug.Log( " Input Process End " );
    //}

}
