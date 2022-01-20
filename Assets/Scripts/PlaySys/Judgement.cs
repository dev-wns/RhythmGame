using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum JudgeType { None, Kool, Cool, Good, Bad, Miss }

public class Judgement : MonoBehaviour
{
    public const float Kool = 22f;
    public const float Cool = 24f + Kool;
    public const float Good = 26f + Cool;
    public const float Bad  = 28f + Good;

    private int koolCount, coolCount, goodCount, badCount, missCount;

    public delegate void DelJudge( JudgeType _type );
    public event DelJudge OnJudge;

    private void Awake()
    {
        var rt = transform as RectTransform;
        rt.anchoredPosition = new Vector3( 0f, GameSetting.JudgePos, -1f );
        rt.sizeDelta        = new Vector3( GameSetting.GearWidth, GameSetting.JudgeHeight, 1f );
    }

    public JudgeType GetJudgeType( float _diff )
    {
        float diffAbs = Globals.Abs( _diff );

        if ( diffAbs <= Kool )                        return JudgeType.Kool;
        else if ( diffAbs > Kool && diffAbs <= Cool ) return JudgeType.Cool;
        else if ( diffAbs > Cool && diffAbs <= Good ) return JudgeType.Good;
        else if ( diffAbs > Good && diffAbs <= Bad  ) return JudgeType.Bad;
        else if ( _diff < -Bad )                      return JudgeType.Miss;
        else                                          return JudgeType.None;
        
    }

    public void OnJudgement( JudgeType _type )
    {
        switch ( _type )
        {
            case JudgeType.None: break;
            case JudgeType.Kool:
            {
                koolCount++;
            }
            break;

            case JudgeType.Cool:
            {
                coolCount++;
            } break;

            case JudgeType.Good:
            {
                goodCount++;
            } break;

            case JudgeType.Bad:
            {
                badCount++;
            } break;

            case JudgeType.Miss:
            {
                missCount++;
            } break;
        }

        OnJudge?.Invoke( _type );
    }
}
