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

        switch ( _type )
        {
            case JudgeType.Perfect:     curScore += maxScore;         break; 
            case JudgeType.LatePerfect: curScore += maxScore * .83d;  break; 
            case JudgeType.Great:       curScore += maxScore * .61d;  break; 
            case JudgeType.Good:        curScore += maxScore * .47d;  break; 
            case JudgeType.Bad:         curScore += maxScore * .25d;  break; 
            case JudgeType.Miss:        curScore += 0d;               break; 
        }

        double calcCurScore  = Globals.Round( curScore );
        double calcPrevScore = Globals.Round( prevScore );
        for ( int i = 0; i < images.Count; i++ )
        {
            if ( ( int )calcPrevScore % 10 == ( int )calcCurScore % 10 )
                 break;

            images[i].sprite = sprites[( int )calcCurScore % 10];
            calcCurScore  *= .1d;
            calcPrevScore *= .1d;
        }

        prevScore = curScore;
    }
}
