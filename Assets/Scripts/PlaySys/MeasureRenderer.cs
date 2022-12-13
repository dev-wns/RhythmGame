using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasureRenderer : MonoBehaviour
{
    private ObjectPool<MeasureRenderer> pool;
    private double time;

    private SpriteRenderer rdr;

    private void Awake()
    {
        rdr = GetComponent<SpriteRenderer>();
        transform.localScale = new Vector3( GameSetting.GearWidth, GameSetting.MeasureHeight, 1f );
    }

    private void LateUpdate()
    {
        transform.localPosition = new Vector2( 0, GameSetting.JudgePos + ( float )( ( time - NowPlaying.PlaybackChanged ) * GameSetting.Weight ) );
        if ( transform.localPosition.y <= GameSetting.JudgePos )
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
