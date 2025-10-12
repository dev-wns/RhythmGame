using System.Collections.Generic;
using UnityEngine;

public class ScoreMeterSystem : MonoBehaviour
{
    [Header( "Score Meter" )]
    private Queue<ScoreMeterRenderer> rdrQueue = new Queue<ScoreMeterRenderer>();
    public ObjectPool<ScoreMeterRenderer> pool;
    public ScoreMeterRenderer prefab;
    public Transform background;

    [Header( "Color" )]
    public readonly Color PerfectColor = new Color( .25f,   1f,   1f, .65f );
    public readonly Color GreatColor   = new Color(  .5f,   1f,  .5f, .65f );
    public readonly Color GoodColor    = new Color(   1f, .85f, .75f, .65f );

    private void Awake()
    {
        NowPlaying.OnClear     += AllDespawn;
        InputManager.OnHitNote += UpdateScoreMeter;

        pool = new ObjectPool<ScoreMeterRenderer>( prefab, 30, false );
        background.localScale = new Vector2( Judgement.HitRange.Miss, background.localScale.y );
    }

    private void OnDestroy()
    {
        NowPlaying.OnClear     -= AllDespawn;
        InputManager.OnHitNote -= UpdateScoreMeter;
    }

    private void AllDespawn()
    {
        while ( rdrQueue.Count > 0 )
        {
            ScoreMeterRenderer endScoreMeter = rdrQueue.Dequeue();
            endScoreMeter.Clear();
        }
    }

    private void UpdateScoreMeter( HitData _hitData )
    {
        Color color = Color.red;
        switch ( _hitData.hitResult )
        {
            case HitResult.Maximum:
            case HitResult.Perfect: color = PerfectColor; break;
            case HitResult.Great:   
            case HitResult.Good:    color = GreatColor;   break;
            case HitResult.Bad:     color = GoodColor;    break;
            default: return;
        }

        ScoreMeterRenderer newScoreMeter = pool.Spawn();
        newScoreMeter.SetInfo( color, new Vector2( ( float )-_hitData.diff, transform.position.y ) );
        rdrQueue.Enqueue( newScoreMeter );
    }
}