using TMPro;
using UnityEngine;

public class AccuracySystem : MonoBehaviour
{
    [Header("RateSystem")]
    private InGame scene;
    private Judgement judge;

    public TextMeshProUGUI text;

    // Judge Count
    private int maximum;
    private int perfect;
    private int great;
    private int good;
    private int bad;
    private int miss;

    private float Total   => ( 300f * ( maximum + perfect ) ) + ( 200f * great ) + ( 100f * good ) + ( 50f * bad );
    private float Max     => 3f * ( maximum + perfect + great + good + bad + miss );
    public float Accuracy => Total / Max;

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
        DataStorage.Inst.UpdateResult( HitResult.Accuracy, ( int )( Accuracy * 100f ) );
    }

    private void OnReLoad()
    {
        maximum = 0;
        perfect = 0;
        great   = 0;
        good    = 0;
        bad     = 0;
        miss    = 0;

        text.text = $"100.00%";
    }

    private void AccuracyUpdate( JudgeResult _result )
    {
        HitResult hitResult = _result.hitResult;
        if ( hitResult == HitResult.None )
             return;

        switch ( hitResult )
        {
            case HitResult.Maximum: maximum++; break;
            case HitResult.Perfect: perfect++; break;
            case HitResult.Great:   great++;   break;
            case HitResult.Good:    good++;    break;
            case HitResult.Bad:     bad++;     break;
            case HitResult.Miss:    miss++;    break;
            default: return;
        }

        text.text = $"{ Accuracy:F2}%";

        //switch ( hitResult )
        //{
        //    case HitResult.Maximum:
        //    case HitResult.Perfect: curAccuracy += 10000d;  break;
        //    case HitResult.Great:   curAccuracy += 6250d;   break;
        //    case HitResult.Good:    curAccuracy += 3125d;   break;
        //    case HitResult.Bad:     curAccuracy += 1562.5d; break;
        //    case HitResult.Miss:    curAccuracy += .0001d;  break;
        //    default: return;
        //}
        //++curMaxCount;

        //text.text = $"{( ( int )( curAccuracy / curMaxCount ) * .01d ):F2}%";
    }
}
