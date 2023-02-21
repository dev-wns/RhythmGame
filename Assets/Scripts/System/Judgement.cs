using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HitResult { None, Maximum, Perfect, Great, Good, Bad, Miss, Fast, Slow, Accuracy, Combo, Score, Count }

public class Judgement : MonoBehaviour
{
    public const double Maximum      = .0165d;           
    public const double Perfect      = .0225d + Maximum; 
    public const double Great        = .022d  + Perfect;  
    public const double Good         = .015d  + Great;    
    public const double Bad          = .013d  + Good;     
    public const double Miss         = .1f;

    public event Action<HitResult, NoteType> OnJudge;

    public bool CanBeHit( double _diff ) => Global.Math.Abs( _diff ) <= Bad;
    
    public bool IsMiss( double _diff ) => _diff < -Bad;

    public void ResultUpdate( double _diff, NoteType _noteType )
    {
        double diffAbs = Math.Abs( _diff );
        
        HitResult result = diffAbs <= Maximum                      ? HitResult.Maximum :
                           diffAbs > Maximum && diffAbs <= Perfect ? HitResult.Perfect :
                           diffAbs > Perfect && diffAbs <= Great   ? HitResult.Great   :
                           diffAbs > Great   && diffAbs <= Good    ? HitResult.Good    :
                           diffAbs > Good    && diffAbs <= Bad     ? HitResult.Bad     :
                           _diff   < -Bad                          ? HitResult.Miss    : 
                                                                     HitResult.None;

        //if ( diffAbs <= Maximum )                           result = HitResult.Maximum;
        //else if ( diffAbs > Maximum && diffAbs <= Perfect ) result = HitResult.Perfect;
        //else if ( diffAbs > Perfect && diffAbs <= Great   ) result = HitResult.Great;
        //else if ( diffAbs > Great   && diffAbs <= Good    ) result = HitResult.Good;
        //else if ( diffAbs > Good    && diffAbs <= Bad     ) result = HitResult.Bad;
        //else if ( _diff   < -Bad                          ) result = HitResult.Miss;
        //else                                                result = HitResult.None;

        if ( diffAbs > Perfect && diffAbs <= Bad )
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
                 NowPlaying.Inst.AddHitData( _type, Miss );
        }
    }
}
