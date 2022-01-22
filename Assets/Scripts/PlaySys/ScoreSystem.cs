using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreSystem : NumberAtlasBase
{
    [Header( "System" )]
    private InGame scene;
    private Judgement judge;

    private double currentScore;
    private double maxScore;

    protected override void Awake()
    {
        base.Awake();
        scene = GameObject.FindGameObjectWithTag("Scene").GetComponent<InGame>();
        scene.OnSystemInitialize += Initialize;

        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += ScoreImageUpdate;
    }

    private void Initialize( in Chart _chart )
    {
        int maxJudgeCount;
        if ( GameSetting.CurrentGameMode.HasFlag( GameMode.NoSlider ) )
        {
            maxJudgeCount = _chart.notes.Count;
        }
        else
        {
            maxJudgeCount = NowPlaying.Inst.CurrentSong.noteCount +
                          ( NowPlaying.Inst.CurrentSong.sliderCount * 2 );
        }

        maxScore = 1000000d / maxJudgeCount;
    }

    private void ScoreImageUpdate( JudgeType _type )
    {
        if ( _type == JudgeType.None ) return;

        double addScore = 0;
        switch ( _type )
        {
            case JudgeType.Perfect:     addScore = maxScore;         break; 
            case JudgeType.LazyPerfect: addScore = maxScore * .83d;  break; 
            case JudgeType.Great:       addScore = maxScore * .61d;  break; 
            case JudgeType.Good:        addScore = maxScore * .47d;  break; 
            case JudgeType.Bad:         addScore = maxScore * .25d;  break; 
            case JudgeType.Miss:        addScore = 0;                break; 
        }
        currentScore += addScore;

        int num;
        double calcScore = currentScore;
        if ( currentScore > 0 ) num = Globals.Log10( ( float )calcScore ) + 1;
        else                    num = 1;

        for ( int i = 0; i < images.Count; i++ )
        {
            if ( i == num ) break;

            images[i].sprite = sprites[Mathf.FloorToInt( ( float )calcScore ) % 10];
            calcScore *= .1d;
        }
    }
}
