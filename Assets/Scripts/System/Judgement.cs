using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HitResult { None, Maximum, Perfect, Great, Good, Bad, Miss, Fast, Slow, Accuracy, Combo, Score, Count }

public class Judgement : MonoBehaviour
{
    public struct JudgeData
    {
        public double maximum;
        public double perfect;
        public double great;
        public double good;
        public double bad;
        public double miss;
    }
    public static readonly JudgeData OriginJudgeData = new JudgeData() 
    { 
        maximum = .0165d, 
        perfect = .039d, 
        great   = .061d, 
        good    = .076d, 
        bad     = .089d, 
        miss    = .1d 
    };
    public static readonly JudgeData HardJudgeData   = new JudgeData()
    {
        maximum = .0165d * .75d,
        perfect = .039d  * .75d,
        great   = .061d  * .75d,
        good    = .076d  * .75d,
        bad     = .089d  * .75d,
        miss    = .1d
    };
    public static JudgeData Judge;
    public event Action<HitResult, NoteType> OnJudge;

    private void Awake()
    {
        Judge = GameSetting.CurrentGameMode.HasFlag( GameMode.HardJudge ) ? HardJudgeData : OriginJudgeData;
    }

    public bool CanBeHit( double _diff ) => Global.Math.Abs( _diff ) <= Judge.bad;
    
    public bool IsMiss( double _diff ) => _diff < -Judge.bad;

    public void ResultUpdate( double _diff, NoteType _noteType )
    {
        double diffAbs = Math.Abs( _diff );
        HitResult result = diffAbs <= Judge.maximum                             ? HitResult.Maximum :
                           diffAbs >  Judge.maximum && diffAbs <= Judge.perfect ? HitResult.Perfect :
                           diffAbs >  Judge.perfect && diffAbs <= Judge.great   ? HitResult.Great   :
                           diffAbs >  Judge.great   && diffAbs <= Judge.good    ? HitResult.Good    :
                           diffAbs >  Judge.good    && diffAbs <= Judge.bad     ? HitResult.Bad     :
                           _diff   < -Judge.bad                                 ? HitResult.Miss    : 
                                                                                  HitResult.None;

        if ( diffAbs > Judge.perfect && diffAbs <= Judge.bad )
        {
            HitResult fsType = _diff >= 0d ? HitResult.Fast : HitResult.Slow;
            OnJudge?.Invoke( fsType, _noteType );
            NowPlaying.Inst.IncreaseResult( fsType );
        }

        OnJudge?.Invoke( result, _noteType );
        NowPlaying.Inst.IncreaseResult( result );
        NowPlaying.Inst.AddHitData( _noteType, _diff );
    }

    public void ResultUpdate( HitResult _result, NoteType _type, int _count = 1 )
    {
        OnJudge?.Invoke( _result, _type );

        for ( int i = 0; i < _count; i++ )
        {
            if ( _result == HitResult.Miss )
                 NowPlaying.Inst.AddHitData( _type, Judge.miss );
        }
    }
}
