using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    private InGame scene;

    [Header("ScoreSystem")]
    private double targetScore;
    private double curScore;

    private float countDuration = 0.1f; // 카운팅에 걸리는 시간 설정.
    private float countOffset;

    public TextMeshProUGUI text;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnReLoad += OnReLoad;


        InputManager.OnHitNote += UpdateScore;
    }

    private void OnDestroy()
    {
        InputManager.OnHitNote -= UpdateScore;
    }

    private void OnReLoad()
    {
        targetScore   = 0d;
        curScore      = 0d;
    }
    
    private void UpdateScore( HitData _hitData )
    {
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
