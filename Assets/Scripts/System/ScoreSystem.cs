using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    private InGame scene;
    private Judgement judge;

    [Header("ScoreSystem")]
    private int    totalNotes;
    
    private double baseScore;
    private double bonusScore;
    private int    bonus = 100;

    private double targetScore;
    private double curScore;

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
        DataStorage.Inst.UpdateResult( HitResult.Score, ( int )Global.Math.Round( targetScore ) );
    }

    private void OnReLoad()
    {
        baseScore     = 0d;
        targetScore   = 0d;
        curScore      = 0d;
        bonus         = 100;
    }

    private void Initialize()
    {
        bool hasKeyConversion = GameSetting.CurrentGameMode.HasFlag( GameMode.KeyConversion ) &&  NowPlaying.CurrentSong.keyCount == 7;
        var slider = hasKeyConversion ? NowPlaying.CurrentSong.sliderCount - NowPlaying.CurrentSong.delSliderCount : NowPlaying.CurrentSong.sliderCount;
        var note   = hasKeyConversion ? NowPlaying.CurrentSong.noteCount   - NowPlaying.CurrentSong.delNoteCount   : NowPlaying.CurrentSong.noteCount;

        totalNotes = GameSetting.CurrentGameMode.HasFlag( GameMode.NoSlider ) ? note : note + ( slider * 2 );
        StartCoroutine( Count() );
    }

    
    private void ScoreUpdate( JudgeResult _result )
    {
        HitResult hitResult = _result.hitResult;
        if ( hitResult == HitResult.None )
             return;

        int hitBonus      = 0; // 판정에 따른 보너스 추가량
        int hitPunishment = 0; // 판정에 따른 보너스 감소량
        int hitScoreValue = 0; // 기본 스코어 판정 밸류
        int hitBonusValue = 0; // 기본 스코어 판정 보너스 밸류
        switch ( hitResult )
        {
            case HitResult.Maximum:
            {
                hitScoreValue = 320;
                hitBonusValue = 32;
                hitBonus = 2;
            } break;

            case HitResult.Perfect:
            {
                hitScoreValue = 300;
                hitBonusValue = 32;
                hitBonus = 1;
            } break;

            case HitResult.Great:
            {
                hitScoreValue = 200;
                hitBonusValue = 16;
                hitPunishment = 8;
                hitBonus      = 0;
            } break;

            case HitResult.Good:
            {
                hitScoreValue = 100;
                hitBonusValue = 8;
                hitPunishment = 24;
                hitBonus      = 0;
            } break;

            case HitResult.Bad:
            { 
                hitScoreValue = 50;
                hitBonusValue = 4;
                hitPunishment = 44;
                hitBonus      = 0;
            } break;

            case HitResult.Miss:
            {
                hitScoreValue = 0;
                hitBonusValue = 0;
                hitPunishment = int.MaxValue;
                hitBonus      = 0;
            } break;
            default: return;
        }

        bonus       = ( Global.Math.Clamp( ( bonus + hitBonus - hitPunishment ), 0, 100 ) );
        baseScore   = ( ( 1000000d * .5d ) / totalNotes ) * ( hitScoreValue / 320d );
        bonusScore  = ( ( 1000000d * .5d ) / totalNotes ) * ( hitBonusValue * Mathf.Sqrt( bonus ) / 320d );
        targetScore += baseScore + bonusScore;

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
                 curScore  = targetScore;

            text.text = $"{( ( int )Global.Math.Round( curScore ) ):D7}";
        }
    }
}
