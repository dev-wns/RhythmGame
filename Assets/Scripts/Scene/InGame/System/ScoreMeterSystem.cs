using System.Collections.Generic;
using UnityEngine;

public class ScoreMeterSystem : MonoBehaviour
{
    [Header( "Score Meter" )]
    private static readonly int MaxScoreMeterCount = 40;
    private Queue<ScoreMeterRenderer> rdrQueue = new Queue<ScoreMeterRenderer>();
    public ObjectPool<ScoreMeterRenderer> pool;
    public ScoreMeterRenderer prefab;
    public RectTransform contents;

    private static readonly float DespawnSection = 3;
    private float time;

    [Header( "Color" )]
    public readonly Color PerfectColor = new Color( .25f,   1f,   1f, .65f );
    public readonly Color GreatColor   = new Color(  .5f,   1f,  .5f, .65f );
    public readonly Color GoodColor    = new Color(   1f, .85f, .75f, .65f );

    private void Awake()
    {
        NowPlaying.OnClear     += AllDespawn;
        InputManager.OnHitNote += UpdateScoreMeter;

        pool = new ObjectPool<ScoreMeterRenderer>( prefab, contents, 5 );
    }

    private void OnDestroy()
    {
        NowPlaying.OnClear     -= AllDespawn;
        InputManager.OnHitNote -= UpdateScoreMeter;
    }

    private void Update()
    {
        time += Time.deltaTime;
        if ( time < DespawnSection )
        {
            time = 0f;
            AllDespawn();
        }
    }

    private void AllDespawn()
    {
        while ( rdrQueue.Count > 0 )
        {
            ScoreMeterRenderer endScoreMeter = rdrQueue.Dequeue();
            endScoreMeter.Despawn();
        }
    }

    private void UpdateScoreMeter( HitData _hitData )
    {
        if ( Global.Math.Abs( _hitData.diff ) < double.Epsilon )
             return;

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

        time = 0f;
        ScoreMeterRenderer newScoreMeter = pool.Spawn();
        newScoreMeter.SetInfo( color, new Vector2( ( float )_hitData.diff, 0f ) );
        rdrQueue.Enqueue( newScoreMeter );
        if ( rdrQueue.Count > MaxScoreMeterCount )
        {
            ScoreMeterRenderer endScoreMeter = rdrQueue.Dequeue();
            endScoreMeter.Despawn();
        }
    }
}