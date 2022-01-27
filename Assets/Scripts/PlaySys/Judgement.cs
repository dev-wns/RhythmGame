using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum JudgeType { None, Perfect, LatePerfect, Great, Good, Bad, Miss }

public class Judgement : MonoBehaviour
{
    public const double Perfect     = 22d;
    public const double LatePerfect = 20d + Perfect;
    public const double Great       = 18d + LatePerfect;
    public const double Good        = 16d + Great;
    public const double Bad         = 14d + Good;

    private int perfectCount, latePerfectCount, greatCount, goodCount, badCount, missCount;

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
        else if ( diffAbs > Perfect     && diffAbs <= LatePerfect ) return JudgeType.LatePerfect;
        else if ( diffAbs > LatePerfect && diffAbs <= Great )       return JudgeType.Great;
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
            case JudgeType.LatePerfect: latePerfectCount++; break;
            case JudgeType.Great:       greatCount++;       break;
            case JudgeType.Good:        goodCount++;        break;
            case JudgeType.Bad:         badCount++;         break;
            case JudgeType.Miss:        missCount++;        break;
        }

        OnJudge?.Invoke( _type );
    }
}
