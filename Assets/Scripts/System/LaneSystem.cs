using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class LaneSystem : MonoBehaviour
{
    [Header( "Lane" )]
    public Lane prefab;
    private InGame scene;
    private KeySampleSystem keySampleSystem;
    private List<Lane> lanes = new List<Lane>();
    private System.Random random;
    private readonly int MinimumSwapCount = 5;
    private int keyCount;

    [Header( "Loading" )]
    private Timer soundTimer = new Timer();
    public static uint soundSampleTime;
    public static uint keySoundTime;

    private Timer noteTimer  = new Timer();
    public static uint noteTime;

    private void Awake()
    {
        keyCount = NowPlaying.KeyCount;
        keySampleSystem = GetComponent<KeySampleSystem>();
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnSystemInitializeThread += Initialize;
        scene.OnGameStart += () =>
        {
            for ( int i = 0; i < keyCount; i++ )
            {
                lanes[i].SetLane( i );
            }
        };

        lanes.AddRange( GetComponentsInChildren<Lane>() );
        if ( lanes.Count < keyCount )
        {
            int addCount = keyCount - lanes.Count;
            for ( int i = 0; i < addCount; i++ )
            {
                lanes.Add( Instantiate( prefab, transform ) );
            }
            Debug.Log( $"Create {addCount} lanes." );
        }

        for ( int i = 0; i < keyCount; i++ )
        {
            lanes[i].UpdatePosition( i );
        }
    }

    private void OnDestroy()
    {
        soundSampleTime = 0;
        keySoundTime = 0;
        noteTime = 0;
    }

    private void Initialize( Chart _chart )
    {
        soundTimer.Start();
        if ( !NowPlaying.CurrentSong.isOnlyKeySound )
        {
            if ( AudioManager.Inst.Load( NowPlaying.CurrentSong.audioPath ) )
                 keySampleSystem.AddSample( new KeySound( GameSetting.SoundOffset, Path.GetFileName( NowPlaying.CurrentSong.audioPath ), 1f ) );
        }

        var dir = Path.GetDirectoryName( NowPlaying.CurrentSong.filePath );
        for ( int i = 0; i < _chart.samples.Count; i++ )
        {
            var sample = _chart.samples[i];
            if ( AudioManager.Inst.Load( Path.Combine( dir, sample.name ) ) )
                 keySampleSystem.AddSample( sample );
        }
        soundSampleTime += soundTimer.End;

        noteTimer.Start();
        CreateNotes( _chart );
        noteTime += noteTimer.End;

        keySampleSystem.SortSamples();
        NowPlaying.IsLoadKeySound = true;
    }

    private void CreateNotes( Chart _chart )
    {
        var notes          = _chart.notes;
        string dir         = Path.GetDirectoryName( NowPlaying.CurrentSong.filePath );
        bool hasNoSlider   = GameSetting.CurrentGameMode.HasFlag( GameMode.NoSlider );
        bool hasConversion = GameSetting.CurrentGameMode.HasFlag( GameMode.KeyConversion ) && NowPlaying.CurrentSong.keyCount == 7;
        random = new System.Random( ( int )System.DateTime.Now.Ticks );

        List<int/* lane */> emptyLanes = new List<int>( keyCount );
        double[] prevTimes             = Enumerable.Repeat( double.MinValue, keyCount ).ToArray();
        double secondPerBeat           = ( ( ( 60d / NowPlaying.CurrentSong.mainBPM ) * 4d ) / 32d );
        bool isSevenButton             = NowPlaying.CurrentSong.keyCount == 7;
        for ( int i = 0; i < notes.Count; i++ )
        {
            Note newNote = notes[i];
            if ( hasConversion && isSevenButton )
            {
                if ( newNote.lane == 3 )
                {
                    soundTimer.Start();
                    if ( AudioManager.Inst.Load( Path.Combine( dir, newNote.keySound.name ) ) )
                         keySampleSystem.AddSample( new KeySound( newNote ) );

                    keySoundTime += soundTimer.End;
                    continue;
                }
                else if ( newNote.lane > 3 )
                          newNote.lane -= 1;

                //if ( newNote.lane == 6 )
                //{
                //    if ( AudioManager.Inst.Load( Path.Combine( dir, newNote.keySound.name ) ) )
                //         keySampleSystem.AddSample( new KeySound( newNote ) );

                //    continue;
                //}
            }

            if ( hasNoSlider )
                 newNote.isSlider = false;

            switch ( GameSetting.CurrentRandom )
            {
                case GameRandom.None:
                case GameRandom.Mirror:
                case GameRandom.Basic_Random:
                case GameRandom.Half_Random:
                {
                    newNote.noteDistance   = NowPlaying.Inst.GetDistance( newNote.time );
                    newNote.sliderDistance = NowPlaying.Inst.GetDistance( newNote.sliderTime );

                    soundTimer.Start();
                    AudioManager.Inst.Load( Path.Combine( dir, newNote.keySound.name ) );
                    keySoundTime += soundTimer.End;

                    lanes[newNote.lane].InputSys.AddNote( in newNote );
                }
                break;

                case GameRandom.Max_Random:
                {
                    emptyLanes.Clear();
                    // 빠른계단, 즈레 등 고밀도로 배치될 때 보정
                    for ( int j = 0; j < keyCount; j++ )
                    {
                        if ( secondPerBeat < ( newNote.time - prevTimes[j] ) )
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

                    int selectLane        = emptyLanes[random.Next( 0, int.MaxValue ) % emptyLanes.Count];
                    prevTimes[selectLane] = newNote.isSlider ? newNote.sliderTime : newNote.time;

                    newNote.noteDistance   = NowPlaying.Inst.GetDistance( newNote.time );
                    newNote.sliderDistance = NowPlaying.Inst.GetDistance( newNote.sliderTime );

                    soundTimer.Start();
                    AudioManager.Inst.Load( Path.Combine( dir, newNote.keySound.name ) );
                    keySoundTime += soundTimer.End;

                    lanes[selectLane].InputSys.AddNote( in newNote );
                }
                break;
            }
        }

        switch ( GameSetting.CurrentRandom )
        {
            case GameRandom.Mirror:
            lanes.Reverse();
            break;

            case GameRandom.Basic_Random:
            LaneSwap( 0, keyCount, MinimumSwapCount );
            break;

            case GameRandom.Half_Random:
            int keyCountHalf = Mathf.FloorToInt( keyCount * .5f );
            LaneSwap( 0, keyCountHalf, MinimumSwapCount );
            LaneSwap( keyCountHalf + 1, keyCount, MinimumSwapCount );
            break;
        }
    }

    private void LaneSwap( int _min, int _max, int _swapCount )
    {
        for ( int i = 0; i < _swapCount; i++ )
        {
            var randA = random.Next( _min, _max );
            var randB = random.Next( _min, _max );

            var tmp      = lanes[randA];
            lanes[randA] = lanes[randB];
            lanes[randB] = tmp;
        }
    }
}
