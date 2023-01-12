using System.Linq;
using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneSystem : MonoBehaviour
{
    public Lane prefab;
    private InGame scene;
    private KeySampleSystem keySampleSystem;
    private List<Lane> lanes = new List<Lane>();
    private System.Random random;
    private readonly int MinimumSwapCount = 5;
    private int keyCount;

    private void Awake()
    {
        keyCount = NowPlaying.CurrentSong.keyCount;

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
            Debug.Log( $"Make enough lanes. AddCount : {addCount}" );
        }

        for ( int i = 0; i < keyCount; i++ )
        {
            lanes[i].UpdatePosition( i );
        }
    }

    private void Initialize( Chart _chart )
    {
        Timer perfomenceTimer = new Timer( true );
        if ( !NowPlaying.CurrentSong.isOnlyKeySound )
        {
            if ( SoundManager.Inst.Load( NowPlaying.CurrentSong.audioPath ) )
                 keySampleSystem.AddSample( new KeySound( 0d, Path.GetFileName( NowPlaying.CurrentSong.audioPath ), 1f ) );
        }

        var dir = Path.GetDirectoryName( NowPlaying.CurrentSong.filePath );
        for ( int i = 0; i < _chart.samples.Count; i++ )
        {
            var sample = _chart.samples[i];
            if ( SoundManager.Inst.Load( Path.Combine( dir, sample.name ) ) )
                 keySampleSystem.AddSample( sample );
        }
        Debug.Log( $"BGM load completed ( {perfomenceTimer.End} ms ) TotalCount : {SoundManager.Inst.KeySoundCount}" );

        CreateNotes( _chart );
        keySampleSystem.SortSamples();
        NowPlaying.Inst.IsLoadKeySound = true;
        Debug.Log( $"LaneSystem initialization completed ( {perfomenceTimer.End} ms )" );
    }

    private void CreateNotes( Chart _chart )
    {
        Timer perfomenceTimer = new Timer( true );
        var notes        = _chart.notes;
        string dir       = Path.GetDirectoryName( NowPlaying.CurrentSong.filePath );
        bool hasNoSlider = GameSetting.CurrentGameMode.HasFlag( GameMode.NoSlider );
        random           = new System.Random( ( int )System.DateTime.Now.Ticks );

        List<int/* lane */> emptyLanes = new List<int>( keyCount );
        double[] prevTimes             = Enumerable.Repeat( double.MinValue, keyCount ).ToArray();
        double secondPer16Beats        = ( 60d / ( NowPlaying.CurrentSong.medianBpm + 1/* 최소오차 */ ) ) * .25d; // 4/16
        for ( int i = 0; i < notes.Count; i++ )
        {
            Note newNote = notes[i];
            if ( hasNoSlider ) 
                 newNote.isSlider = false;

            switch ( GameSetting.CurrentRandom )
            {
                case GameRandom.None:
                case GameRandom.Mirror:
                case GameRandom.Basic_Random:
                case GameRandom.Half_Random:
                {
                    newNote.calcTime       = NowPlaying.Inst.GetIncludeBPMTime( newNote.time );
                    newNote.calcSliderTime = NowPlaying.Inst.GetIncludeBPMTime( newNote.sliderTime );

                    SoundManager.Inst.Load( Path.Combine( dir, newNote.keySound.name ) );
                    lanes[newNote.lane].NoteSys.AddNote( in newNote );
                } break;

                case GameRandom.Max_Random:
                {
                    emptyLanes.Clear();
                    for ( int j = 0; j < keyCount; j++ ) // 32비트 빠른계단, 즈레 보정
                    {
                        if ( secondPer16Beats < ( newNote.time - prevTimes[j] ) )
                             emptyLanes.Add( j );
                    }

                    if ( emptyLanes.Count == 0 ) // 보정할 수 없을때 남은 자리 찾기
                    {
                        for ( int j = 0; j < keyCount; j++ )
                        {
                            if ( prevTimes[j] < newNote.time )
                                 emptyLanes.Add( j );
                        }
                    }

                    int selectLane        = emptyLanes[random.Next( 0, int.MaxValue ) % emptyLanes.Count];
                    prevTimes[selectLane] = newNote.isSlider ? newNote.sliderTime : newNote.time;

                    newNote.calcTime       = NowPlaying.Inst.GetIncludeBPMTime( newNote.time );
                    newNote.calcSliderTime = NowPlaying.Inst.GetIncludeBPMTime( newNote.sliderTime );

                    SoundManager.Inst.Load( Path.Combine( dir, newNote.keySound.name ) );
                    lanes[selectLane].NoteSys.AddNote( in newNote );
                } break;
            }
        }

        // 라인별 스왑일 때
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
            LaneSwap( 0,                keyCountHalf, MinimumSwapCount );
            LaneSwap( keyCountHalf + 1, keyCount,     MinimumSwapCount );
            break;
        }

        Debug.Log( $"Note distribution with KeySound has been completed. ( {perfomenceTimer.End} ms )  RandomType : {GameSetting.CurrentRandom}" );
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
