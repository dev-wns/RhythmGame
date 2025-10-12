using System.Collections.Generic;
using UnityEngine;

public class ScoreMeterSystem : MonoBehaviour
{
    [Header( "Color" )]
    public readonly Color PerfectColor = new Color( .25f,   1f,   1f, .65f );
    public readonly Color GreatColor   = new Color(  .5f,   1f,  .5f, .65f );
    public readonly Color GoodColor    = new Color(   1f, .85f, .75f, .65f );

    [Header( "Score Meter" )]
    public ObjectPool<ScoreMeterRenderer> pool;
    public ScoreMeterRenderer prefab;
    public Transform background;
    private Queue<ScoreMeterRenderer> rdrQueue = new ();
    private readonly int MaxAverageCount = 4;

    [Header( "Marker" )]
    public MarkerRenderer marker;
    private float sumDiff;

    private void Awake()
    {
        NowPlaying.OnClear     += Clear;
        InputManager.OnHitNote += UpdateScoreMeter;

        pool = new ObjectPool<ScoreMeterRenderer>( prefab, 30, false );
        background.localScale = new Vector2( Judgement.HitRange.Miss * .5f, background.localScale.y );
    }

    private void OnDestroy()
    {
        NowPlaying.OnClear     -= Clear;
        InputManager.OnHitNote -= UpdateScoreMeter;
    }

    private void Clear()
    {
        sumDiff = 0f;
        marker.Clear();
        while ( rdrQueue.Count > 0 )
        {
            var obj = rdrQueue.Dequeue();
            Despawn( obj );
        }
    }

    public void Despawn( ScoreMeterRenderer _rdr )
    {
        _rdr.Clear();
        pool.Despawn( _rdr );
    }

    private void UpdateScoreMeter( HitData _hitData )
    {
        if ( _hitData.hitResult < 0 ||                           // Miss 제외
             Global.Math.Abs( _hitData.diff ) < double.Epsilon ) // 보정된 판정 제외
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

        // Score Meter
        float diff = -( float )_hitData.diff;
        ScoreMeterRenderer scoreMeter = pool.Spawn();
        if ( scoreMeter.system == null )
        {
            scoreMeter.system = this;
            scoreMeter.transform.position = transform.position;
        }

        if ( rdrQueue.Count >= MaxAverageCount )
             sumDiff -= rdrQueue.Dequeue().Diff;

        scoreMeter.SetInfo( color, diff );
        rdrQueue.Enqueue( scoreMeter );
        sumDiff += diff;

        // Marker
        if ( _hitData.hitResult >= 0 ) // Hit
             marker.SetInfo( sumDiff / rdrQueue.Count );
    }
}