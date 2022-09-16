using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneSystem : MonoBehaviour
{
    private InGame scene;
    private KeySampleSystem keySampleSystem;
    private List<Lane> lanes = new List<Lane>();

    private System.Random random;

    private void Awake()
    {
        keySampleSystem = GetComponent<KeySampleSystem>();
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnSystemInitializeThread += Initialize;
        scene.OnGameStart += SetLane;

        lanes.AddRange( GetComponentsInChildren<Lane>() );

        random = new System.Random( ( int )System.DateTime.Now.Ticks );
    }

    private void Initialize( in Chart _chart )
    {
        if ( !NowPlaying.Inst.CurrentSong.isOnlyKeySound )
        {
            KeySound bgm = new KeySound( 0d, "BGM", 1f );
            if ( SoundManager.Inst.LoadKeySound( NowPlaying.Inst.CurrentSong.audioPath, out bgm.sound ) )
                bgm.hasSound = true;

            keySampleSystem.AddSample( bgm );
        }

        var dir = System.IO.Path.GetDirectoryName( NowPlaying.Inst.CurrentSong.filePath );
        for ( int i = 0; i < _chart.samples.Count; i++ )
        {
            var sample = _chart.samples[i];
            if( sample.hasSound )
                SoundManager.Inst.LoadKeySound( System.IO.Path.Combine( dir, sample.name ), out sample.sound );
            keySampleSystem.AddSample( sample );
        }

        NowPlaying.Inst.IsLoadKeySounds = true;

        CreateNotes( _chart );
    }

    private void SetLane()
    {
        for ( int i = 0; i < ( int )GameKeyAction.Count; i++ )
        {
            lanes[i].SetLane( i );
        }
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

    private void CreateNotes( in Chart _chart )
    {
        var dir = System.IO.Path.GetDirectoryName( NowPlaying.Inst.CurrentSong.filePath );

        var notes = _chart.notes;
        double[] sliderTimes = new double[6];
        BitArray isUsedColumn = new BitArray( 6 );
        double prevTime = 0d;

        for ( int i = 0; i < notes.Count; i++ )
        {
            bool hasNoSliderMod = GameSetting.CurrentGameMode.HasFlag( GameMode.NoSlider );

            switch ( GameSetting.CurrentRandom )
            {
                case GameRandom.None:
                case GameRandom.Mirror:
                case GameRandom.Basic_Random:
                case GameRandom.Half_Random:
                {
                    Note newNote = notes[i];

                    if ( hasNoSliderMod )
                         newNote.isSlider = false;

                    newNote.calcTime       = NowPlaying.Inst.GetChangedTime( newNote.time );
                    newNote.calcSliderTime = NowPlaying.Inst.GetChangedTime( newNote.sliderTime );

                    if ( newNote.keySound.hasSound )
                         SoundManager.Inst.LoadKeySound( System.IO.Path.Combine( dir, newNote.keySound.name ), out newNote.keySound.sound );

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
                    if ( hasNoSliderMod )
                         newNote.isSlider = false;

                    newNote.calcTime       = NowPlaying.Inst.GetChangedTime( newNote.time );
                    newNote.calcSliderTime = NowPlaying.Inst.GetChangedTime( newNote.sliderTime );

                    if ( newNote.keySound.hasSound )
                         SoundManager.Inst.LoadKeySound( System.IO.Path.Combine( dir, newNote.keySound.name ), out newNote.keySound.sound );

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
