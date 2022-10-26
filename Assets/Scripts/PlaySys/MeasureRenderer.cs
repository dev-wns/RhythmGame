using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasureRenderer : MonoBehaviour
{
    private InGame game;
    private ObjectPool<MeasureRenderer> pool;
    private double time;

    private SpriteRenderer rdr;
    private double weight;

    private void Awake()
    {
        game = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        game.OnScrollChange += ScrollUpdate;

        rdr = GetComponent<SpriteRenderer>();
        transform.localScale = new Vector3( GameSetting.GearWidth, GameSetting.MeasureHeight, 1f );

        ScrollUpdate();
    }

    private void OnDestroy() => game.OnScrollChange -= ScrollUpdate;

    private void ScrollUpdate() => weight = GameSetting.Weight;

    private void LateUpdate()
    {
        var pos = GameSetting.JudgePos + ( float )( ( time - NowPlaying.PlaybackChanged ) * weight );
        transform.localPosition = new Vector2( 0, pos );

        if ( pos <= GameSetting.JudgePos )
        {
            rdr.enabled = false;
            pool.Despawn( this );
        }
    }

    public void SetInfo( ObjectPool<MeasureRenderer> _pool, double _time )
    {
        rdr.enabled = true;
        pool = _pool;
        time = _time;
    }
}
