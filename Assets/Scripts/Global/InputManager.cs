using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;

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

public struct NoteDatas
{
    public int index;
    public Note[] notes;

    public Note this[int i, int j]
    {
        get => notes[i + j];
        set => notes[i + j] = value;
    }
}

public class InputManager : Singleton<InputManager>
{
    #region Lane
    [SerializeField] Lane prefab;
    [SerializeField] List<Lane> lanes = new();
    [SerializeField] List<Note>[] notes;
    #endregion

    #region Thread

    private CancellationTokenSource cancelSource = new();
    
    [DllImport( "user32.dll" )] static extern short GetAsyncKeyState( int _vKey );
    #endregion

    [Header( "Loading" )]
    private Timer soundTimer = new Timer();
    public TextMeshProUGUI soundText;
    public static uint soundSampleTime;
    public static uint keySoundTime;

    private Timer noteTimer  = new Timer();
    public static uint noteTime;

    [Header( "Lane" )]
    private System.Random random;

    protected override void Awake()
    {
        base.Awake();

        NowPlaying.OnPostInitialize += Initialize;

        DataStorage.Inst.LoadAssetsAsync( "Lane", ( GameObject _lane ) =>
        {
            if ( !_lane.TryGetComponent( out prefab ) )
                 Debug.LogError( $"Load Asset Failed ( Lane )" );
        } );
    }

    private void OnApplicationQuit()
    {
        cancelSource?.Cancel();
        Release();
    }

    public async void GameStart()
    {
        for ( int i = 0; i < lanes.Count; i++ )
        {
            lanes[i].GameStart( notes[i] );
        }

        await Task.Run( () => Process( cancelSource.Token ) );
    }

    public async void Process( CancellationToken _token )
    {
        int[]      indexes   = new int     [lanes.Count];
        bool[]     isEntries = new bool    [lanes.Count]; // 하나의 입력으로 하나의 노트만 처리하기위한 노트 진입점
        KeyState[] keyStates = new KeyState[lanes.Count]; // 레인별 입력 상태
        KeySound[] keySounds = new KeySound[lanes.Count];
        for ( int i = 0; i < lanes.Count; i++ )
              keySounds[i] = notes[i][0].keySound;

        Debug.Log( " Input Process Start " );
        while ( !_token.IsCancellationRequested )
        {
            for ( int i = 0; i < lanes.Count; i++ )
            {
                // 입력 상태 체크
                KeyState previous = keyStates[i];
                if ( ( GetAsyncKeyState( lanes[i].VKey ) & 0x8000 ) != 0 ) keyStates[i] = previous == KeyState.None || previous == KeyState.Up   ? KeyState.Down : KeyState.Hold;
                else                                                       keyStates[i] = previous == KeyState.Down || previous == KeyState.Hold ? KeyState.Up   : KeyState.None;

                //  키음 선 실행
                if ( keyStates[i] == KeyState.Down )
                {
                    // 노트에 키음이 없을 수도 있다. 
                    if ( DataStorage.Inst.TryGetSound( keySounds[i].name, out FMOD.Sound sound ) )
                         AudioManager.Inst.Play( sound, keySounds[i].volume );
                }

                // 데이터 소진 시 계산 불필요
                if ( indexes[i] >= notes[i].Count )
                     continue;
                
                Note note        = notes[i][indexes[i]];
                double startDiff = note.time       - NowPlaying.Playback;
                double endDiff   = note.sliderTime - NowPlaying.Playback;

                // 미스 체크
                if ( !isEntries[i] && Judgement.IsMiss( startDiff ) )
                {
                    indexes[i]  += 1;
                    isEntries[i] = false;
                    lanes[i].AddData( new InputData( NowPlaying.Playback, startDiff, KeyState.None, NoteState.Miss ) );

                    continue;
                }

                // 입력 상태에 따른 노트 처리
                if ( !isEntries[i] && keyStates[i] == KeyState.Down )
                {
                    if ( Judgement.CanBeHit( startDiff ) )
                    {
                        isEntries[i] = note.isSlider;
                        indexes[i]   = note.isSlider ? indexes[i] : ++indexes[i];

                        lanes[i].AddData( new InputData( NowPlaying.Playback, startDiff, KeyState.Down, NoteState.Hit ) );
                    }
                }

                if ( isEntries[i] && keyStates[i] == KeyState.Hold && endDiff < 0d )
                {
                    indexes[i]  += 1;
                    isEntries[i] = false;
                    lanes[i].AddData( new InputData( NowPlaying.Playback, 0d, KeyState.None, NoteState.Hit ) );
                }

                if ( isEntries[i] && keyStates[i] == KeyState.Up )
                {
                    indexes[i]  += 1;
                    isEntries[i] = false;
                    lanes[i].AddData( new InputData( NowPlaying.Playback, endDiff, KeyState.Up,
                                                     Judgement.CanBeHit( endDiff ) ? NoteState.Hit : NoteState.Miss ) );
                }

                // 사운드 변경 ( 모든 데이터 체크완료 시 마지막 사운드로 고정 )
                if ( indexes[i] < notes[i].Count )
                     keySounds[i] = notes[i][indexes[i]].keySound;
            }

            await Task.Delay( 1 );
        }

        Debug.Log( " Input Process End " );
    }

    public void Release()
    {
        for ( int i = 0; i < lanes.Count; i++ )
            DestroyImmediate( lanes[i] );
        
        lanes.Clear();

        for ( int i = 0; i < notes.Length; i++ )
            notes[i].Clear();

        notes = null;
    }

    private void Initialize()
    {
        notes = new List<Note>[NowPlaying.KeyCount];
        for ( int i = 0; i < NowPlaying.KeyCount; i++ )
        {
            lanes.Add( Instantiate( prefab, transform ) );
            lanes[i].Initialize( i );
            notes[i] = new List<Note>();
        }
        Debug.Log( $"Create {lanes.Count} lanes." );

        noteTimer.Start();
        DivideDatas( NowPlaying.CurrentChart.notes );
        noteTime += noteTimer.End;
    }

    private void DivideDatas( ReadOnlyCollection<Note> _datas )
    {
        bool isConvert  = GameSetting.HasFlag( GameMode.KeyConversion ) && NowPlaying.CurrentSong.keyCount == 7;
        bool isNoSlider = GameSetting.HasFlag( GameMode.NoSlider );
        random = new System.Random( ( int )DateTime.Now.Ticks );

        List<int/* lane */> emptyLanes = new List<int>( NowPlaying.KeyCount );
        double[] prevTimes             = Enumerable.Repeat( double.MinValue, NowPlaying.KeyCount ).ToArray();
        double   secondPerBeat         = ( ( ( 60d / NowPlaying.CurrentSong.mainBPM ) * 4d ) / 32d );
        for ( int i = 0; i < _datas.Count; i++ )
        {
            Note newNote = _datas[i];

            if ( isConvert )
            {
                switch ( newNote.lane )
                {
                    // 잘려진 노트는 키음만 자동재생되도록 한다.
                    case 3:
                    {
                        soundTimer.Start();
                        NowPlaying.Inst.AddSample( new KeySound( newNote ), SoundType.BGM );
                        keySoundTime += soundTimer.End;
                        
                    } continue;
                    
                    // 잘려진 옆 노트를 한칸씩 이동한다.
                    case > 3: newNote.lane -= 1; 
                    break;
                }
            }
            
            if ( isNoSlider )
                 newNote.isSlider = false;

            switch ( GameSetting.CurrentRandom )
            {
                // 레인 인덱스와 동일한 번호에 노트 분배
                case GameRandom.None:
                case GameRandom.Mirror:
                case GameRandom.Basic_Random:
                case GameRandom.Half_Random:
                {
                    newNote.noteDistance   = NowPlaying.Inst.GetDistance( newNote.time );
                    newNote.sliderDistance = NowPlaying.Inst.GetDistance( newNote.sliderTime );

                    soundTimer.Start();

                    NowPlaying.Inst.AddSample( newNote.keySound, SoundType.KeySound );
                    keySoundTime += soundTimer.End;

                    notes[newNote.lane].Add( newNote );
                } break;

                // 맥스랜덤은 무작위 레인에 노트를 배치한다.
                case GameRandom.Max_Random:
                {
                    emptyLanes.Clear();
                    // 빠른계단, 즈레 등 고밀도로 배치될 때 보정
                    for ( int j = 0; j < NowPlaying.KeyCount; j++ )
                    {
                        if ( secondPerBeat < ( newNote.time - prevTimes[j] ) )
                             emptyLanes.Add( j );
                    }

                    // 자리가 없을 때 보정되지않은 상태로 배치
                    if ( emptyLanes.Count == 0 )
                    {
                        for ( int j = 0; j < NowPlaying.KeyCount; j++ )
                        {
                            if ( prevTimes[j] < newNote.time )
                                 emptyLanes.Add( j );
                        }
                    }

                    int selectLane        = emptyLanes[random.Next( 0, int.MaxValue ) % emptyLanes.Count];
                    prevTimes[selectLane] = newNote.isSlider ? newNote.sliderTime : newNote.time;

                    newNote.noteDistance   = NowPlaying.Inst.GetDistance( newNote.time );
                    newNote.sliderDistance = NowPlaying.Inst.GetDistance( newNote.sliderTime );

                    soundTimer.Start();
                    NowPlaying.Inst.AddSample( newNote.keySound, SoundType.KeySound );
                    keySoundTime += soundTimer.End;

                    notes[selectLane].Add( newNote );
                } break;
            }
        }

        switch ( GameSetting.CurrentRandom )
        {
            // 레인은 유지하고, 분배된 노트 데이터만 스왑한다.
            case GameRandom.Mirror:
            {
                notes.Reverse();
            } break;

            case GameRandom.Basic_Random:
            {
                Swap( 0, NowPlaying.KeyCount );
            } break;

            case GameRandom.Half_Random:
            { 
                int keyCountHalf = Mathf.FloorToInt( NowPlaying.KeyCount * .5f );
                Swap( 0,                keyCountHalf );
                Swap( keyCountHalf + 1, NowPlaying.KeyCount );
            } break;
        }
    }

    private void Swap( int _min, int _max )
    {
        for ( int i = 0; i < 10; i++ )
        {
            var randA = random.Next( _min, _max );
            var randB = random.Next( _min, _max );

            var tmp      = notes[randA];
            notes[randA] = notes[randB];
            notes[randB] = tmp;
        }
    }
}
