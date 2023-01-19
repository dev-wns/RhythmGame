using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HitResult { None, Maximum, Perfect, Great, Good, Bad, Miss, Fast, Slow, Rate, Combo, Score, Count }
public struct HitData
{
    public HitResult result;
    public double diff;
    public double time;

    public HitData( HitResult _res, double _diff, double _time )
    {
        result = _res;
        diff   = _diff;
        time   = _time;
    }
}

public class Judgement : MonoBehaviour
{
    public const double Maximum      = .0165d;           
    public const double Perfect      = .0225d + Maximum; 
    public const double Great        = .022d + Perfect;  
    public const double Good         = .015d + Great;    
    public const double Bad          = .013d + Good;     
    public const double Miss         = .1f;

    public event Action<HitResult, NoteType> OnJudge;

    public bool CanBeHit( double _diff ) => Global.Math.Abs( _diff ) <= Bad;
    
    public bool IsMiss( double _diff ) => _diff < -Bad;

    public void ResultUpdate( double _diff, NoteType _type )
    {
        double diffAbs = Math.Abs( _diff );
        
        HitResult result;
        if ( diffAbs <= Maximum )                           result = HitResult.Maximum;
        else if ( diffAbs > Maximum && diffAbs <= Perfect ) result = HitResult.Perfect;
        else if ( diffAbs > Perfect && diffAbs <= Great   ) result = HitResult.Great;
        else if ( diffAbs > Great   && diffAbs <= Good    ) result = HitResult.Good;
        else if ( diffAbs > Good    && diffAbs <= Bad     ) result = HitResult.Bad;
        else if ( _diff   < -Bad                          ) result = HitResult.Miss;
        else                                                result = HitResult.None;
        
        if ( diffAbs > Perfect && diffAbs <= Bad )
        {
            if ( _diff > 0d ) OnJudge?.Invoke( HitResult.Fast, _type );
            else              OnJudge?.Invoke( HitResult.Slow, _type );
        }

        OnJudge?.Invoke( result, _type );
        NowPlaying.Inst.AddHitData( result, _diff );
    }

    public void ResultUpdate( HitResult _result, NoteType _type, int _count = 1 )
    {
        OnJudge?.Invoke( _result, _type );

        for ( int i = 0; i < _count; i++ )
        {
            if ( _result == HitResult.Miss )
                 NowPlaying.Inst.AddHitData( HitResult.Miss, Miss );
        }
    }
}
