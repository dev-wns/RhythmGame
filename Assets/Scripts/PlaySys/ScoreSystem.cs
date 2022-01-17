using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

public class ScoreSystem : MonoBehaviour
{
    private InGame scene;
    public SpriteAtlas atlas;
    public Judgement judge;

    private List<Image> images = new List<Image>();
    private List<Sprite> sprites = new List<Sprite>();
    private float currentScore;

    private float maxScore;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag("Scene").GetComponent<InGame>();
        scene.OnSystemInitialize += Initialize;
        judge.OnJudge += ScoreImageUpdate;

        images.AddRange( GetComponentsInChildren<Image>() );
        images.Reverse();

        Sprite[] spriteArray = new Sprite[atlas.spriteCount];
        atlas.GetSprites( spriteArray );
        sprites.AddRange( spriteArray );

        sprites.Sort( ( Sprite A, Sprite B ) => A.name.CompareTo( B.name ) );
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
            case JudgeType.Kool: addScore = maxScore;       break; // 100%
            case JudgeType.Cool: addScore = maxScore * .85f; break; // 90%
            case JudgeType.Good: addScore = maxScore * .63f; break; // 80%
            case JudgeType.Bad:  addScore = maxScore * .41f; break; // 70%
            case JudgeType.Miss: addScore = 0;              break; // 0%
        }
        currentScore += addScore;

        int num;
        float calcScore = currentScore;
        if ( currentScore > 0 ) num = ( int )Mathf.Log10( calcScore ) + 1;
        else                    num = 1;

        for ( int i = 0; i < images.Count; i++ )
        {
            if ( i == num ) break;

            images[i].sprite = sprites[( int )calcScore % 10];
            //images[i].sprite = atlas.GetSprite( $"score-{( int )calcScore % 10}" );
            calcScore *= .1f;
        }
    }
}
