using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum JudgeType { None, Perfect, LazyPerfect, Great, Good, Bad, Miss }

public class Judgement : MonoBehaviour
{
    public const double Perfect     = 22d;
    public const double LazyPerfect = 20d + Perfect;
    public const double Great       = 18d + LazyPerfect;
    public const double Good        = 16d + Great;
    public const double Bad         = 14d + Good;

    private int perfectCount, lazyPerfectCount, greatCount, goodCount, badCount, missCount;

    public event Action<JudgeType> OnJudge;

    private void Awake()
    {
        var rt = transform as RectTransform;
        rt.anchoredPosition = new Vector3( 0f, GameSetting.JudgePos, -1f );
        rt.sizeDelta        = new Vector3( GameSetting.GearWidth, GameSetting.JudgeHeight, 1f );
    }

    public JudgeType GetJudgeType( double _diff )
    {
        double diffAbs = Globals.Abs( _diff );
    
        if ( diffAbs <= Perfect )                                   return JudgeType.Perfect;
        else if ( diffAbs > Perfect     && diffAbs <= LazyPerfect ) return JudgeType.LazyPerfect;
        else if ( diffAbs > LazyPerfect && diffAbs <= Great )       return JudgeType.Great;
        else if ( diffAbs > Great       && diffAbs <= Good )        return JudgeType.Good;
        else if ( diffAbs > Good        && diffAbs <= Bad )         return JudgeType.Bad;
        else if ( _diff < -Bad )                                    return JudgeType.Miss;
        else                                                        return JudgeType.None;
    }

    public void OnJudgement( JudgeType _type )
    {
        switch ( _type )
        {
            case JudgeType.None:                            break;
            case JudgeType.Perfect:     perfectCount++;     break;
            case JudgeType.LazyPerfect: lazyPerfectCount++; break;
            case JudgeType.Great:       greatCount++;       break;
            case JudgeType.Good:        goodCount++;        break;
            case JudgeType.Bad:         badCount++;         break;
            case JudgeType.Miss:        missCount++;        break;
        }

        OnJudge?.Invoke( _type );
    }
}
