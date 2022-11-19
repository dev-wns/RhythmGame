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


    private int keyCount;
    private void Awake()
    {
        keyCount = NowPlaying.Inst.CurrentSong.keyCount == 8 ? 7 : NowPlaying.Inst.CurrentSong.keyCount;

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
        }

        for ( int i = 0; i < keyCount; i++ )
        {
            lanes[i].UpdatePosition( i );
        }

        random = new System.Random( ( int )System.DateTime.Now.Ticks );
    }

    private void Initialize( Chart _chart )
    {
        if ( !NowPlaying.Inst.CurrentSong.isOnlyKeySound )
        {
            KeySound bgm = new KeySound( 0d, "BGM", 1f );
            if ( SoundManager.Inst.Load( NowPlaying.Inst.CurrentSong.audioPath, out bgm.sound ) )
                bgm.hasSound = true;

            keySampleSystem.AddSample( bgm );
        }

        var dir = System.IO.Path.GetDirectoryName( NowPlaying.Inst.CurrentSong.filePath );
        for ( int i = 0; i < _chart.samples.Count; i++ )
        {
            var sample = _chart.samples[i];
            if( sample.hasSound )
                SoundManager.Inst.Load( Path.Combine( dir, sample.name ), out sample.sound );
            keySampleSystem.AddSample( sample );
        }

        CreateNotes( _chart );
        keySampleSystem.SortSamples();
        NowPlaying.Inst.IsLoadKeySound = true;
    }

    private void LaneSwap( int _min, int _max, int _swapCount = 6 )
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

    private void CreateNotes( Chart _chart )
    {
        var dir = System.IO.Path.GetDirectoryName( NowPlaying.Inst.CurrentSong.filePath );

        var notes = _chart.notes;
        double[] sliderTimes = new double[6];
        BitArray isUsedColumn = new BitArray( 6 );
        double prevTime = 0d;

        bool isEightKey = NowPlaying.Inst.OriginKeyCount == 8;
        for ( int i = 0; i < notes.Count; i++ )
        {
            bool hasNoSlider = GameSetting.CurrentGameMode.HasFlag( GameMode.NoSlider );

            switch ( GameSetting.CurrentRandom )
            {
                case GameRandom.None:
                case GameRandom.Mirror:
                case GameRandom.Basic_Random:
                case GameRandom.Half_Random:
                {
                    Note newNote = notes[i];
                    if ( hasNoSlider )
                         newNote.isSlider = false;

                    newNote.calcTime       = NowPlaying.Inst.GetChangedTime( newNote.time );
                    newNote.calcSliderTime = NowPlaying.Inst.GetChangedTime( newNote.sliderTime );

                    if ( newNote.keySound.hasSound )
                         SoundManager.Inst.Load( Path.Combine( dir, newNote.keySound.name ), out newNote.keySound.sound );
                        
                    lanes[newNote.lane].NoteSys.AddNote( in newNote );
                }
                break;

                case GameRandom.Max_Random:
                {
                    if ( prevTime < notes[i].time )
                    {
                        for ( int j = 0; j < 6; j++ )
                        {
                            if ( sliderTimes[j] < notes[i].time )
                                 isUsedColumn[j] = false;
                        }
                    }

                    var rand = random.Next( 0, 6 );
                    while ( isUsedColumn[rand] || sliderTimes[rand] > notes[i].time )
                    {
                        rand = random.Next( 0, 6 );
                    }

                    isUsedColumn[rand] = true;
                    prevTime = notes[i].time;

                    if ( notes[i].isSlider )
                         sliderTimes[rand] = notes[i].sliderTime;

                    Note newNote = notes[i];
                    if ( hasNoSlider )
                         newNote.isSlider = false;

                    newNote.calcTime       = NowPlaying.Inst.GetChangedTime( newNote.time );
                    newNote.calcSliderTime = NowPlaying.Inst.GetChangedTime( newNote.sliderTime );

                    if ( newNote.keySound.hasSound )
                         SoundManager.Inst.Load( Path.Combine( dir, newNote.keySound.name ), out newNote.keySound.sound );

                    lanes[rand].NoteSys.AddNote( in newNote );
                }
                break;
            }
        }

        // 라인별 스왑일 때
        switch ( GameSetting.CurrentRandom )
        {
            case GameRandom.Mirror:
            lanes.Reverse();
            break;

            case GameRandom.Basic_Random:
            LaneSwap( 0, 5 );
            break;

            case GameRandom.Half_Random:
            LaneSwap( 0, 2 );
            LaneSwap( 3, 5 );
            break;
        }
    }
}
