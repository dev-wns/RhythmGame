using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    private InGame scene;
    private Judgement judge;
    
    [Header("Sprite")]
    public int sortingOrder;

    [Header("ScoreSystem")]
    public List<Sprite> sprites = new List<Sprite>();
    private List<SpriteRenderer> images = new List<SpriteRenderer>();
    private double targetScore;
    private double curScore;
    private double maxScore;

    private float countDuration = 0.1f; // 카운팅에 걸리는 시간 설정.
    private float countOffset;

    private void Awake()
    {
        images.AddRange( GetComponentsInChildren<SpriteRenderer>() );
        images.Reverse();

        scene = GameObject.FindGameObjectWithTag("Scene").GetComponent<InGame>();
        scene.OnSystemInitialize += Initialize;
        scene.OnReLoad += OnReLoad;
        scene.OnResult += OnResult;

        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += ScoreUpdate;

        for ( int i = 0; i < images.Count; i++ )
              images[i].sortingOrder = sortingOrder;
    }

    private void OnResult()
    {
        NowPlaying.Inst.SetResult( HitResult.Score, ( int )Global.Math.Round( targetScore ) );
    }

    private void OnReLoad()
    {
        targetScore = 0d;
        curScore    = 0d;

        for ( int i = 0; i < images.Count; i++ )
        {
            images[i].sprite = sprites[0];
        }
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

    private void ScoreUpdate( HitResult _result, NoteType _type )
    {
        switch ( _result )
        {
            case HitResult.None:
            case HitResult.Fast:
            case HitResult.Slow:
            return;

            case HitResult.Maximum: targetScore += maxScore;        break;
            case HitResult.Perfect: targetScore += maxScore * .82d; break;
            case HitResult.Great:   targetScore += maxScore * .63d; break;
            case HitResult.Good:    targetScore += maxScore * .41d; break;
            case HitResult.Bad:     targetScore += maxScore * .25d; break;
            case HitResult.Miss:    targetScore += 0d;              break;
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

            double calcScore = Global.Math.Round( curScore );
            int num = Global.Math.Log10( calcScore ) + 1;
            for ( int i = 0; i < images.Count; i++ )
            {
                if ( i == num )
                    break;

                images[i].sprite = sprites[( int )calcScore % 10];
                calcScore *= .1d;
            }
        }
    }
}
