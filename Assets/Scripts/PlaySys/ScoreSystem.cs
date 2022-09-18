using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScoreSystem : MonoBehaviour
{
    private InGame scene;
    private Judgement judge;

    public List<Sprite> sprites = new List<Sprite>();
    private List<SpriteRenderer> images = new List<SpriteRenderer>();
    private double curScore;
    private double incScore;
    private double maxScore;
    private Tweener tweener;

    private void Awake()
    {
        images.AddRange( GetComponentsInChildren<SpriteRenderer>() );
        images.Reverse();

        scene = GameObject.FindGameObjectWithTag("Scene").GetComponent<InGame>();
        scene.OnSystemInitialize += Initialize;
        scene.OnReLoad += ReLoad;

        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += ScoreUpdate;
        NowPlaying.Inst.OnResult += Result;
    }

    private void OnDestroy()
    {
        tweener?.Kill();
        NowPlaying.Inst.OnResult -= Result;
    }

    private void Result() => judge.SetResult( HitResult.Score, ( int )Globals.Round( curScore ) );

    private void ReLoad()
    {
        tweener?.Kill();
        curScore = 0d;
        incScore = 0d;

        for ( int i = 0; i < images.Count; i++ )
        {
            images[i].sprite = sprites[0];
        }
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

    private void ScoreUpdate( HitResult _type )
    {

        switch ( _type )
        {
            case HitResult.None:
            case HitResult.Fast:
            case HitResult.Slow:
            return;

            case HitResult.Perfect: curScore += maxScore;        break;
            case HitResult.Great:   curScore += maxScore * .87d; break;
            case HitResult.Good:    curScore += maxScore * .63d; break;
            case HitResult.Bad:     curScore += maxScore * .41d; break;
            case HitResult.Miss:    curScore += 0d;              break;
        }

        tweener?.Kill();
        tweener = DOTween.To( () => incScore, x => ImageUpdate( x ), curScore, .1f );
    }

    private void ImageUpdate( double _value )
    {
        incScore = _value;
        double calcScore = Globals.Round( _value );
        int num = Globals.Log10( calcScore ) + 1;
        for ( int i = 0; i < images.Count; i++ )
        {
            if ( i == num ) break;

            images[i].sprite = sprites[( int )calcScore % 10];
            calcScore *= .1d;
        }
    }
}
