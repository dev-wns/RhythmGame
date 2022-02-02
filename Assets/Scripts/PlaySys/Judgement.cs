using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HitResult { None, Perfect, Great, Good, Bad, Miss, Fast, Slow }

public class Judgement : MonoBehaviour
{
    public const double Perfect     = .0284d;
    public const double Great       = .064d;
    public const double Good        = .097d;
    public const double Bad         = .127d;
    public const double Miss        = .151d;

    public event Action<HitResult> OnJudge;

    private void Awake()
    {
        var rt = transform as RectTransform;
        rt.anchoredPosition = new Vector3( 0f, GameSetting.JudgePos, -1f );
        rt.sizeDelta        = new Vector3( GameSetting.GearWidth, GameSetting.JudgeHeight, 1f );
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

        if ( diffAbs > Perfect && diffAbs <= Bad )
        {
            if ( _timeOffset >= 0d ) OnJudge?.Invoke( HitResult.Fast );
            else                     OnJudge?.Invoke( HitResult.Slow );
        }
    }

    public void ResultUpdate( HitResult _type )
    {
        OnJudge?.Invoke( _type );
    }
}
