using TMPro;
using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    [Header("ScoreSystem")]
    private double targetScore;
    private double curScore;

    private float countDuration = 0.1f; // 카운팅에 걸리는 시간 설정.
    private float countOffset;

    public TextMeshProUGUI text;

    private void Awake()
    {
        Judgement.OnHitNote += UpdateScore;
        NowPlaying.OnClear  += Clear;
    }

    private void OnDestroy()
    {
        Judgement.OnHitNote -= UpdateScore;
        NowPlaying.OnClear  -= Clear;
    }

    private void Clear()
    {
        targetScore   = 0d;
        curScore      = 0d;
        text.text     = $"{( ( int ) Global.Math.Round( curScore ) ):D7}";
    }
    
    private void UpdateScore( HitData _hitData )
    {
        if ( _hitData.hitResult == HitResult.None )
             return;

        targetScore = Judgement.CurrentResult.Score;
        countOffset = ( float )( targetScore - curScore ) / countDuration;
    }

    private void Update()
    {
        if ( targetScore > curScore )
        {
            curScore += countOffset * Time.deltaTime;
            if ( curScore >= targetScore )
                 curScore = targetScore;

            text.text = $"{( ( int )Global.Math.Round( curScore ) ):D7}";
        }
    }
}
