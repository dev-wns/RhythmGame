using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HitResult { None, Perfect, Great, Good, Bad, Miss, Fast, Slow }

public class Judgement : MonoBehaviour
{
    public const double Perfect     = .0224d;
    public const double Great       = .064d;
    public const double Good        = .097d;
    public const double Bad         = .127d;
    public const double Miss        = .151d;

    public event Action<HitResult> OnJudge, OnFastSlow;

    private void Awake()
    {
        var rt = transform as RectTransform;
        rt.anchoredPosition = new Vector3( 0f, GameSetting.JudgePos, -1f );
        rt.sizeDelta        = new Vector3( GameSetting.GearWidth, GameSetting.JudgeHeight, 1f );
    }

    //private static readonly DifficultyRange[] base_ranges =
    //{
    //        new DifficultyRange(HitResult.Perfect, 22.4D, 19.4D, 13.9D),
    //        new DifficultyRange(HitResult.Great, 64, 49, 34),
    //        new DifficultyRange(HitResult.Good, 97, 82, 67),
    //        new DifficultyRange(HitResult.Bad, 127, 112, 97),
    //        new DifficultyRange(HitResult.Miss, 151, 136, 121),
    //};

    static double DifficultyRange( double difficulty, double min, double mid, double max )
    {
        if ( difficulty > 5 )
            return mid + ( max - mid ) * ( difficulty - 5 ) / 5;
        if ( difficulty < 5 )
            return mid - ( mid - min ) * ( 5 - difficulty ) / 5;

        return mid;
    }

    public bool CanBeHit( double _timeOffset )
    {
        return Globals.Abs( _timeOffset ) <= Bad ? true : false;
    }
    
    public bool IsMiss( double _timeOffset )
    {
        return _timeOffset < -Bad ? true : false;
    }

    public void ResultUpdate( double _timeOffset )
    {
        double diffAbs = Globals.Abs( _timeOffset );

        if ( _timeOffset < -Bad )                           OnJudge?.Invoke( HitResult.Miss );
        else if ( diffAbs > Good    && diffAbs <= Bad )     OnJudge?.Invoke( HitResult.Bad );
        else if ( diffAbs > Great   && diffAbs <= Good )    OnJudge?.Invoke( HitResult.Good );
        else if ( diffAbs > Perfect && diffAbs <= Great )   OnJudge?.Invoke( HitResult.Great );
        else if ( diffAbs >= 0d     && diffAbs <= Perfect ) OnJudge?.Invoke( HitResult.Perfect );
        else                                                OnJudge?.Invoke( HitResult.None );
    }

    public void ResultUpdate( HitResult _type )
    {
        OnJudge?.Invoke( _type );
    }
}
