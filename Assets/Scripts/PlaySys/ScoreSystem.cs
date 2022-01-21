using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreSystem : NumberAtlasBase
{
    [Header( "System" )]
    private InGame scene;
    private Judgement judge;

    private float currentScore;
    private float maxScore;

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

        maxScore = 1000000f / maxJudgeCount;
    }

    private void ScoreImageUpdate( JudgeType _type )
    {
        if ( _type == JudgeType.None ) return;

        float addScore = 0;
        switch ( _type )
        {
            case JudgeType.Perfect:     addScore = maxScore;         break; 
            case JudgeType.LazyPerfect: addScore = maxScore * .83f;  break; 
            case JudgeType.Great:       addScore = maxScore * .61f;  break; 
            case JudgeType.Good:        addScore = maxScore * .47f;  break; 
            case JudgeType.Bad:         addScore = maxScore * .25f;  break; 
            case JudgeType.Miss:        addScore = 0;                break; 
        }
        currentScore += addScore;

        int num;
        float calcScore = currentScore;
        if ( currentScore > 0 ) num = ( int )Globals.Log10( ( uint )calcScore ) + 1;
        else                    num = 1;

        for ( int i = 0; i < images.Count; i++ )
        {
            if ( i == num ) break;

            images[i].sprite = sprites[( int )calcScore % 10];
            calcScore *= .1f;
        }
    }
}
