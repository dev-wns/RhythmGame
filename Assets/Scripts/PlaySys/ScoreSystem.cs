using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreSystem : MonoBehaviour
{
    private InGame scene;
    private Judgement judge;

    public List<Sprite> sprites = new List<Sprite>();
    private List<SpriteRenderer> images = new List<SpriteRenderer>();
    private double prevScore, curScore;
    private double maxScore;

    private void Awake()
    {
        images.AddRange( GetComponentsInChildren<SpriteRenderer>() );
        images.Reverse();

        scene = GameObject.FindGameObjectWithTag("Scene").GetComponent<InGame>();
        scene.OnSystemInitialize += Initialize;

        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += ScoreImageUpdate;
    }

    private void FixedUpdate()
    {
        if ( prevScore == curScore )
             return;

        prevScore = curScore;

        double calcScore = Mathf.RoundToInt( ( float )curScore );
        int num = curScore > 0 ? Globals.Log10( calcScore ) + 1 : 1;
        for ( int i = 0; i < images.Count; i++ )
        {
            if ( i == num ) break;

            images[i].sprite = sprites[( int )calcScore % 10];
            calcScore *= .1d;
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
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

        double addScore = 0d;
        switch ( _type )
        {
            case JudgeType.Perfect:     addScore = maxScore;         break; 
            case JudgeType.LazyPerfect: addScore = maxScore * .83d;  break; 
            case JudgeType.Great:       addScore = maxScore * .61d;  break; 
            case JudgeType.Good:        addScore = maxScore * .47d;  break; 
            case JudgeType.Bad:         addScore = maxScore * .25d;  break; 
            case JudgeType.Miss:        addScore = 0d;               break; 
        }
        curScore += addScore;
    }
}
