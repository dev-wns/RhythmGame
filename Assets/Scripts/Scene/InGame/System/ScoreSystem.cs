using TMPro;
using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    [SerializeField] private InGame scene;
    [Header("ScoreSystem")]
    private double targetScore;
    private double curScore;

    private float countDuration = 0.1f; // 카운팅에 걸리는 시간 설정.
    private float countOffset;

    public TextMeshProUGUI text;

    private void Awake()
    {
        Judgement.Inst.OnHitNote += UpdateScore;
        scene.OnClear  += Clear;
    }

    private void OnDestroy()
    {
        Judgement.Inst.OnHitNote -= UpdateScore;
        scene.OnClear  -= Clear;
    }

    private void Clear()
    {
        targetScore   = 0d;
        curScore      = 0d;
        text.SetText( "{0:0000000}", ( int )Global.Math.Round( curScore ) );
    }
    
    private void UpdateScore( in HitData _hitData )
    {
        if ( _hitData.hitResult == HitResult.None )
             return;

        targetScore = Judgement.Inst.Score;
        countOffset = ( float )( targetScore - curScore ) / countDuration;
    }

    private void Update()
    {
        if ( targetScore > curScore )
        {
            curScore += countOffset * Time.deltaTime;
            if ( curScore >= targetScore )
                 curScore = targetScore;

            text.SetText( "{0:0000000}", ( int )Global.Math.Round( curScore ) );
        }
    }
}
