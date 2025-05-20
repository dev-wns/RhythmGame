using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    private InGame scene;
    private Judgement judge;

    [Header("ScoreSystem")]
    private double targetScore;
    private double curScore;
    private double maxScore;

    private float countDuration = 0.1f; // 카운팅에 걸리는 시간 설정.
    private float countOffset;

    public TextMeshProUGUI text;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnSystemInitialize += Initialize;
        scene.OnReLoad += OnReLoad;
        scene.OnResult += OnResult;

        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += ScoreUpdate;
    }

    private void OnResult()
    {
        NowPlaying.Inst.SetResult( HitResult.Score, ( int )Global.Math.Round( targetScore ) );
    }

    private void OnReLoad()
    {
        targetScore = 0d;
        curScore = 0d;
    }

    private void Initialize( Chart _chart )
    {
        bool hasKeyConversion = GameSetting.CurrentGameMode.HasFlag( GameMode.KeyConversion ) &&  NowPlaying.CurrentSong.keyCount == 7;
        var slider = hasKeyConversion ? NowPlaying.CurrentSong.sliderCount - NowPlaying.CurrentSong.delSliderCount : NowPlaying.CurrentSong.sliderCount;
        var note   = hasKeyConversion ? NowPlaying.CurrentSong.noteCount   - NowPlaying.CurrentSong.delNoteCount   : NowPlaying.CurrentSong.noteCount;

        int maxJudgeCount = GameSetting.CurrentGameMode.HasFlag( GameMode.NoSlider ) ? note : note + ( slider * 2 );
        maxScore = 1000000d / maxJudgeCount;
        StartCoroutine( Count() );
    }

    private void ScoreUpdate( JudgeResult _result )
    {
        switch ( _result.hitResult )
        {
            case HitResult.Maximum: targetScore += maxScore; break;
            case HitResult.Perfect: targetScore += maxScore * .70d; break;
            case HitResult.Great: targetScore += maxScore * .50d; break;
            case HitResult.Good: targetScore += maxScore * .30d; break;
            case HitResult.Bad: targetScore += maxScore * .15d; break;
            default: return;
        }

        countOffset = ( float )( targetScore - curScore ) / countDuration;
    }

    private IEnumerator Count()
    {
        WaitUntil waitNextValue = new WaitUntil( () => targetScore > curScore );
        while ( true )
        {
            yield return waitNextValue;

            curScore += countOffset * Time.deltaTime;
            if ( curScore >= targetScore )
                curScore = targetScore;

            text.text = $"{( ( int )Global.Math.Round( curScore ) ):D7}";
        }
    }
}
