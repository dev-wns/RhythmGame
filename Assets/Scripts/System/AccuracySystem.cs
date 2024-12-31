using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AccuracySystem : MonoBehaviour
{
    [Header("RateSystem")]
    private InGame scene;
    private Judgement judge;

    private int curMaxCount;
    private double curAccuracy;

    public TextMeshProUGUI text;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnReLoad += OnReLoad;
        scene.OnResult += OnResult;

        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += AccuracyUpdate;
    }

    private void OnResult()
    {
        NowPlaying.Inst.SetResult( HitResult.Accuracy, ( int )( curAccuracy / curMaxCount ) );
    }

    private void OnReLoad()
    {
        curMaxCount = 0;
        curAccuracy = 0d;

        text.text = $"100.00%";
    }

    private void AccuracyUpdate( JudgeResult _result )
    {
        HitResult hitResult = _result.hitResult;
        if ( hitResult == HitResult.None ) 
             return;

        switch ( hitResult )
        {
            case HitResult.Maximum:
            case HitResult.Perfect: curAccuracy +=  100d; break; 
            case HitResult.Great:   curAccuracy +=  90d;  break; 
            case HitResult.Good:    curAccuracy +=  80d;  break; 
            case HitResult.Bad:     curAccuracy +=  70d;  break; 
            case HitResult.Miss:    curAccuracy +=  .0001d; break; 
            default:                                        return;
        }
        ++curMaxCount;

        text.text = $"{(curAccuracy / curMaxCount):F2}%";
    }
}
