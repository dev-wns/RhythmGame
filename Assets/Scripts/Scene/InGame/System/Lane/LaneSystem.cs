using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout( LayoutKind.Sequential )]
public struct KeyEvent
{
    public int       vKey;
    public KeyState  keyState;
    public long      timestamp;
}

public class LaneSystem : MonoBehaviour
{
    [Header( "Lane" )]
    public  Lane prefab;
    private List<Lane> lanes = new();

    [Header( "Hold Process" )]
    private int    bitHolding;
    private double lnStartTime;
    private double lnEndTime;
    private bool   isHolding;

    private static readonly double LNComboInterval = 100; // ms

    [DllImport( "Assistant" )] private static extern bool GetKeyEvent( out KeyEvent keyEvent );
    [DllImport( "Assistant" )] private static extern bool GetPerformanceCounter( out long performanceCount );
    public static ReadOnlyCollection<Note>[] Notes { get; private set; } // 레인별로 분할된 노트 데이터

    private void Awake()
    {
        NowPlaying.OnInitialize     += Initialize;
        NowPlaying.OnLoadAsync      += DivideNotes;
        NowPlaying.OnUpdateInThread += UpdateInput;
        NowPlaying.OnClear          += Clear;
        NowPlaying.OnRelease        += Release;
        Judgement.OnHitNote         += Transfer;
    }

    private void OnDestroy()
    {
        NowPlaying.OnInitialize     -= Initialize;
        NowPlaying.OnLoadAsync      -= DivideNotes;
        NowPlaying.OnUpdateInThread -= UpdateInput;
        NowPlaying.OnClear          -= Clear;
        NowPlaying.OnRelease        -= Release;
        Judgement.OnHitNote         -= Transfer;

        Notes = null;
    }

    private void Clear()
    {
        bitHolding = 0;

        foreach( Lane lane in lanes )
        {
            lane.Clear();
        }
    }

    private void Release()
    {
        Notes = null;
        for ( int lane = 0; lane < NowPlaying.KeyCount; lane++ )
        {
            InputSystem inputSys = lanes[lane].InputSys;
            inputSys.OnChangedLNState -= DetectHoldNote;
        }
    }

    private void Initialize()
    {
        Notes = new ReadOnlyCollection<Note>[NowPlaying.KeyCount];
        for ( int lane = 0; lane < NowPlaying.KeyCount; lane++ )
        {
            lanes.Add( Instantiate( prefab, transform ) );
            lanes[lane].Initialize( lane );
            
            InputSystem inputSys = lanes[lane].InputSys;
            inputSys.OnChangedLNState += DetectHoldNote;
        }
        Debug.Log( $"Create {lanes.Count} lanes." );
    }

    private void UpdateInput( double _playback )
    {
        GetKeyEvent( out KeyEvent keyEvent );
        foreach ( Lane lane in lanes )
        {
            InputSystem inputSys = lane.InputSys;
            inputSys.UpdateInput( _playback, keyEvent );
        }

        if ( isHolding )
        {
            while ( lnStartTime + LNComboInterval <= _playback &&
                    lnStartTime + LNComboInterval <= lnEndTime )
            {
                Judgement.AddCombo();
                lnStartTime += LNComboInterval;
            }
        }
    }

    private void DetectHoldNote( LNState _info )
    {
        if ( _info.keyState == KeyState.Down )
        {
            lnEndTime = Global.Math.Max( lnEndTime, _info.end ); // 더 긴 LN을 기준으로 함
            if ( bitHolding == 0 ) // Hold 중인 노트가 없을 때
            {
                isHolding   = true;
                lnStartTime = _info.start;
            }

            bitHolding |= ( 1 << _info.lane );  // Bit On
        }
        else
        {
            bitHolding &= ~( 1 << _info.lane ); // Bit Off
            if ( bitHolding == 0 )
            {
                isHolding   = false;
                lnStartTime = 0;
                lnEndTime   = 0;
            }
        }
    }

    private void Transfer( HitData _data )
    {
        lanes[_data.lane].AddData( _data );
    }

    /// <summary> 게임모드가 적용된 상태로 노트를 분할합니다. </summary>
    private void DivideNotes()
    {
        int keyCount    = NowPlaying.KeyCount;
        bool isNoSlider = GameSetting.CurrentGameMode.HasFlag( GameMode.NoSlider );
        bool isConvert  = GameSetting.CurrentGameMode.HasFlag( GameMode.ConvertKey ) && NowPlaying.CurrentSong.keyCount == 7;

        const double Epsilon           = .5d; // 0.5 ms
        List<int/* lane */> emptyLanes = new List<int>( keyCount );
        List<Note>[] notes             = Array.ConvertAll( new int[keyCount], _ => new List<Note>() );
        double[] prevTimes             = Enumerable.Repeat( double.MinValue, keyCount ).ToArray();
        double   msPerBeat             = ( 60000d / NowPlaying.MainBPM ) / 4d; // 1/4박자( 16분음표 )

        GetPerformanceCounter( out long qpc );
        int seed             = ( int )( DateTime.Now.Ticks ^ qpc ); // 현재시간 xor 시스템시간
        System.Random random = new System.Random( seed );
        
        for ( int i = 0; i < DataStorage.Notes.Count; i++ )
        {
            Note newNote = DataStorage.Notes[i];
            newNote.isSlider = isNoSlider ? false : newNote.isSlider;
            if ( isConvert )
            {
                if ( newNote.lane == 3 )
                {
                    // 잘린 노트의 키음은 자동재생되도록 한다.
                    DataStorage.Inst.LoadSound( newNote.keySound.name );
                    DataStorage.Inst.AddSample( newNote.keySound );
                    continue;
                }
                else if ( newNote.lane > 3 )
                {
                    // 제외된 중앙노트보다 우측의 노트는 한칸 이동시킨다.
                    newNote.lane -= 1;
                }
            }

            switch ( GameSetting.CurrentRandom )
            {
                // 레인 인덱스와 동일한 번호에 노트 분배
                case GameRandom.None:
                case GameRandom.Mirror:
                case GameRandom.Basic_Random:
                case GameRandom.Half_Random:
                {
                    newNote.distance    = NowPlaying.Inst.GetDistance( newNote.time );
                    newNote.endDistance = NowPlaying.Inst.GetDistance( newNote.endTime );

                    DataStorage.Inst.LoadSound( newNote.keySound.name );
                    notes[newNote.lane].Add( newNote );
                } break;

                // 맥스랜덤은 무작위 레인에 노트를 배치한다.
                case GameRandom.Max_Random:
                {
                    emptyLanes.Clear();
                    // 빠른계단, 즈레 등 고밀도로 배치될 때 보정
                    for ( int j = 0; j < keyCount; j++ )
                    {
                        double diff = newNote.time - prevTimes[j]; // 이전 노트와 현재 노트 사이의 시간
                        if ( diff + Epsilon >= msPerBeat )         // 연타는 16분음표까지만 허용
                             emptyLanes.Add( j );
                    }

                    // 자리가 없을 때 보정되지않은 상태로 배치
                    if ( emptyLanes.Count == 0 )
                    {
                        for ( int j = 0; j < keyCount; j++ )
                        {
                            if ( prevTimes[j] < newNote.time )
                                 emptyLanes.Add( j );
                        }
                    }

                    int selectLane        = emptyLanes[random.Next( emptyLanes.Count )];
                    prevTimes[selectLane] = newNote.isSlider ? newNote.endTime : newNote.time;

                    newNote.lane        = selectLane;
                    newNote.distance    = NowPlaying.Inst.GetDistance( newNote.time );
                    newNote.endDistance = NowPlaying.Inst.GetDistance( newNote.endTime );

                    DataStorage.Inst.LoadSound( newNote.keySound.name );
                    notes[selectLane].Add( newNote );
                } break;
            }
        }

        // 기본방식의 랜덤은 노트분배가 끝난 후, 완성된 데이터를 스왑한다.
        switch ( GameSetting.CurrentRandom )
        {
            case GameRandom.Mirror:       notes.Reverse();                           break;
            case GameRandom.Basic_Random: Global.Math.Shuffle( notes, 0, keyCount ); break;
            case GameRandom.Half_Random:
            {
                int keyCountHalf = Mathf.FloorToInt( keyCount * .5f );
                Global.Math.Shuffle( notes, 0,                keyCountHalf );
                Global.Math.Shuffle( notes, keyCountHalf + 1, keyCount );
            } break;
        }

        for ( int i = 0; i < keyCount; i++ )
        {
            Notes[i] = new ReadOnlyCollection<Note>( notes[i] );
            lanes[i].SetNotes( Notes[i] );
        }
    }
}
