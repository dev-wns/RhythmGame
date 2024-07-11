using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreMeterSystem : MonoBehaviour
{
    [Header( "Score Meter" )]
    public ObjectPool<ScoreMeterRenderer> pool;
    public ScoreMeterRenderer prefab;
    public RectTransform contents;

    [Header( "Color" )]
    public readonly Color PerfectColor = new Color( .25f,   1f,   1f, 1f );
    public readonly Color GreatColor   = new Color(  .5f,   1f,  .5f, 1f );
    public readonly Color GoodColor    = new Color(   1f, .85f, .75f, 1f );

    private void Awake()
    {
        Judgement judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += UpdateScoreMeter;

        pool = new ObjectPool<ScoreMeterRenderer>( prefab, contents, 100 );
    }

    private void UpdateScoreMeter( JudgeResult _result )
    {
        if ( _result.diffAbs < double.Epsilon )
             return;

        Color color = Color.red;
        switch ( _result.hitResult )
        {
            case HitResult.Maximum:
            case HitResult.Perfect: color = PerfectColor; break;
            case HitResult.Great:
            case HitResult.Good:    color = GreatColor;   break;
            case HitResult.Bad:     color = GoodColor;    break;
            default:                                      return;
        }
                      
        ScoreMeterRenderer scoreMeter = pool.Spawn();
        scoreMeter.SetInfo( color, new Vector2( ( float )_result.diff * 1000f, 0f ) );
    }
}
