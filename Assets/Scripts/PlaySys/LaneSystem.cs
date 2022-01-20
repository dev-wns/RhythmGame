using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneSystem : MonoBehaviour
{
    private InGame scene;
    private List<Lane> lanes = new List<Lane>();

    private struct CalcNote
    {
        public Note? note;
        public float noteTime;
        public float sliderTime;
    }

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnSystemInitialize += Initialize;

        lanes.AddRange( GetComponentsInChildren<Lane>() );

        Random.InitState( ( int )System.DateTime.Now.Ticks );
    }

    private void Initialize( in Chart _chart )
    {
        CreateNotes( _chart );

        for ( int i = 0; i < ( int )GameKeyAction.Count; i++ )
            lanes[i].SetLane( i );
    }

    private void LaneSwap( int _min, int _max, int _swapCount = 6 )
    {
        for ( int i = 0; i < _swapCount; i++ )
        {
            var randA = Random.Range( _min, _max );
            var randB = Random.Range( _min, _max );

            var tmp = lanes[randA];
            lanes[randA] = lanes[randB];
            lanes[randB] = tmp;
        }
    }

    private void CreateNotes( in Chart _chart )
    {
        var notes = _chart.notes;
        CalcNote[] column = new CalcNote[6];

        for ( int i = 0; i < notes.Count; i++ )
        {
            bool hasNoSliderMod = GameSetting.CurrentGameMode.HasFlag( GameMode.NoSlider );

            switch ( GameSetting.CurrentRandom )
            {
                case GameRandom.None:
                case GameRandom.Mirror:
                case GameRandom.Random:
                case GameRandom.Half_Random:
                {
                    Note newNote = notes[i];
                    if ( hasNoSliderMod ) 
                         newNote.isSlider = false;

                    newNote.calcTime       = NowPlaying.Inst.GetChangedTime( newNote.time );
                    newNote.calcSliderTime = NowPlaying.Inst.GetChangedTime( newNote.sliderTime );

                    lanes[notes[i].line].NoteSys.AddNote( newNote );
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
                            column[notes[i + j].line].note = notes[i + j];
                            column[notes[i + j].line].noteTime = notes[i + j].time;

                            if ( !hasNoSliderMod && notes[i + j].isSlider )
                            {
                                column[notes[i + j].line].sliderTime = notes[i + j].sliderTime;
                            }

                            count++;
                        }
                        else break;
                    }
                    i += count;

                    // 일반노트만 있을 때 스왑
                    for ( int j = 0; j < 6; j++ )
                    {
                        var rand = Random.Range( 0, 5 );

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
                    }

                    // 노트 추가
                    for ( int j = 0; j < 6; j++ )
                    {
                        if ( column[j].note.HasValue )
                        {
                            Note newNote = column[j].note.Value;

                            newNote.calcTime       = NowPlaying.Inst.GetChangedTime( newNote.time );
                            newNote.calcSliderTime = NowPlaying.Inst.GetChangedTime( newNote.sliderTime );

                            if ( hasNoSliderMod ) 
                                 newNote.isSlider = false;

                            lanes[j].NoteSys.AddNote( newNote );
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

            case GameRandom.Random:
            LaneSwap( 0, 5 );
            break;

            case GameRandom.Half_Random:
            LaneSwap( 0, 2 );
            LaneSwap( 3, 5 );
            break;
        }
    }
}
