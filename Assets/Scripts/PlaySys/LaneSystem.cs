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

    private struct CalcNote
    {
        public Note? note;
        public double noteTime;
        public double sliderTime;
    }

    private void Awake()
    {
        keySampleSystem = GetComponent<KeySampleSystem>();
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnSystemInitializeThread += Initialize;
        scene.OnGameStart += SetLane;

        lanes.AddRange( GetComponentsInChildren<Lane>() );

        random = new System.Random( ( int )System.DateTime.Now.Ticks );
        //Random.InitState( ( int )System.DateTime.Now.Ticks );
    }

    private void Initialize( in Chart _chart )
    {
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
            lanes[i].SetLane( i );
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

            Thread.Sleep( 1 );
        }
    }

    private void CreateNotes( in Chart _chart )
    {
        var dir = System.IO.Path.GetDirectoryName( NowPlaying.Inst.CurrentSong.filePath );

        var notes = _chart.notes;
        CalcNote[] column = new CalcNote[6];
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
                    int lane = newNote.lane;

                    if ( hasNoSliderMod ) 
                         newNote.isSlider = false;

                    newNote.calcTime       = NowPlaying.Inst.GetChangedTime( newNote.time );
                    newNote.calcSliderTime = NowPlaying.Inst.GetChangedTime( newNote.sliderTime );

                    if ( newNote.keySound.hasSound ) 
                         SoundManager.Inst.LoadKeySound( System.IO.Path.Combine( dir, newNote.keySound.name ), out newNote.keySound.sound );

                    lanes[lane].NoteSys.AddNote( in newNote );
                }
                break;

                case GameRandom.Max_Random:
                {
                    int count = -1;
                    // 타격시간이 같은 노트 저장
                    for ( int j = 0; j < 6; j++ )
                    {
                        if ( i + j < notes.Count && notes[i].time == notes[i + j].time )
                        {
                            column[notes[i + j].lane].note = notes[i + j];
                            column[notes[i + j].lane].noteTime = notes[i + j].time;

                            if ( !hasNoSliderMod && notes[i + j].isSlider )
                            {
                                column[notes[i + j].lane].sliderTime = notes[i + j].sliderTime;
                            }

                            count++;
                        }
                        else break;
                    }
                    i += count;

                    // 일반노트만 있을 때 스왑
                    for ( int j = 0; j < 6; j++ )
                    {
                        var rand = random.Next( 0, 5 );

                        bool isOverlab = false;
                        for ( int k = 0; k < 6; k++ )
                        {
                            if ( column[k].sliderTime >= column[rand].noteTime )
                            {
                                isOverlab = true;
                                break;
                            }
                        }

                        if ( !isOverlab )
                        {
                            var tmp      = column[j];
                            column[j]    = column[rand];
                            column[rand] = tmp;
                        }

                        Thread.Sleep( 1 );
                    }

                    // 노트 추가
                    for ( int j = 0; j < 6; j++ )
                    {
                        if ( column[j].note.HasValue )
                        {
                            Note newNote = column[j].note.Value;
                            
                            if ( hasNoSliderMod )
                                 newNote.isSlider = false;

                            newNote.calcTime       = NowPlaying.Inst.GetChangedTime( newNote.time );
                            newNote.calcSliderTime = NowPlaying.Inst.GetChangedTime( newNote.sliderTime );

                            if ( newNote.keySound.hasSound )
                                 SoundManager.Inst.LoadKeySound( System.IO.Path.Combine( dir, newNote.keySound.name ), out newNote.keySound.sound );

                            lanes[j].NoteSys.AddNote( in newNote );
                        }

                        column[j].note = null;
                    }
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
