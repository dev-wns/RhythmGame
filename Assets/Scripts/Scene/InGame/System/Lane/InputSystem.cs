using System;
using System.Collections.ObjectModel;
using UnityEngine;

public struct LNState
{
    public int lane;
    public KeyState keyState;
    public double start;
    public double end;

    public LNState( int _lane, KeyState _keyState, double _start, double _end )
    {
        lane     = _lane;
        keyState = _keyState;
        start    = _start;
        end      = _end;
    }
}

public class InputSystem : MonoBehaviour
{
    [Header( "Lane" )]
    private int lane;
    private int vKey;
    private ReadOnlyCollection<Note> notes;

    [Header( "Input Process" )]
    public  bool     IsEntry { get; private set; } // 롱노트 진입점
    private int      index;      // 노트 인덱스
    private double   lnHeadDiff; // 롱노트 Head Hit Diff
    private KeyState keyState;   // 레인 입력 상태
    private KeySound keySound;

    public event Action<LNState> OnChangedLNState;

    public void Initialize( Lane _lane )
    {
        lane  = _lane.Key;
        vKey  = _lane.VKey;
    }

    public void SetNotes( ReadOnlyCollection<Note> _notes ) => notes = _notes;

    public void Clear()
    {
        index      = 0;
        lnHeadDiff = 0d;
        IsEntry    = false;
        keyState   = KeyState.None;
        keySound   = notes[0].keySound;
    }

    public void UpdateInput( double _playback, KeyEvent _keyEvent )
    {
        if ( vKey == _keyEvent.vKey )
             keyState = _keyEvent.keyState;

        //  키음 선 실행
        if ( keyState == KeyState.Down )
        {
            // 노트에 키음이 없을 수도 있다. 
            if ( DataStorage.Inst.GetSound( keySound.name, out FMOD.Sound sound ) )
                 AudioManager.Inst.Play( sound, keySound.volume );
        }

        // 데이터 소진 시 계산 불필요
        if ( index >= notes.Count )
             return;

        Note   note     = notes[index];
        double headDiff = note.time    - _playback;
        double tailDiff = note.endTime - _playback;
        bool   isSlider = note.isSlider;


        if ( !IsEntry ) // 노트 시작판정
        {
            if ( Judgement.IsLateMiss( headDiff ) ) // Head Late Miss
            {
                Judgement.UpdateResult( note, lane, _playback, headDiff, 0d, KeyState.Down );
                SelectNextNote();
                return;
            }

            if ( keyState == KeyState.Down )
            {
                if ( !isSlider && Judgement.IsEarlyMiss( headDiff ) )
                {
                    Judgement.UpdateResult( note, lane, _playback, headDiff, 0d, KeyState.Down );
                    SelectNextNote();
                    return;
                }

                if ( Judgement.CanBeHit( headDiff ) ) // Head Hit
                {
                    Judgement.UpdateResult( note, lane, _playback, headDiff, 0d, KeyState.Down );

                    if ( isSlider )
                    {
                        IsEntry    = true;
                        lnHeadDiff = headDiff;
                        OnChangedLNState?.Invoke( new LNState( lane, KeyState.Down, _playback, note.endTime ) );
                    }
                    else
                    {
                        SelectNextNote();
                    }
                }
            }
        }
        else // 롱노트 끝판정
        {
            if ( Judgement.IsLateMiss( tailDiff ) )
            {
                OnChangedLNState?.Invoke( new LNState( lane, KeyState.Up, 0d, 0d ) );
                Judgement.UpdateResult( note, lane, _playback, lnHeadDiff, tailDiff, KeyState.Up );
                SelectNextNote();
                return;
            }

            if ( keyState == KeyState.Up )
            {
                OnChangedLNState?.Invoke( new LNState( lane, KeyState.Up, 0d, 0d ) );
                Judgement.UpdateResult( note, lane, _playback, lnHeadDiff, tailDiff, KeyState.Up );
                SelectNextNote();
            }
        }

        if ( keyState == KeyState.Down )
             keyState =  KeyState.Hold;
         
        if ( keyState == KeyState.Up )
             keyState =  KeyState.None;
    }

    private void SelectNextNote()
    {
        index++;
        IsEntry    = false;
        lnHeadDiff = 0d;
        keyState   = KeyState.None;

        // 사운드 변경 ( 모든 데이터 체크완료 시 마지막 사운드로 고정 )
        if ( index < notes.Count )
             keySound = notes[index].keySound;
    }
}
