using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasureRenderer : MonoBehaviour, IObjectPool<MeasureRenderer>
{
    public ObjectPool<MeasureRenderer> pool { get; set; }

    private double time;

    private SpriteRenderer rdr;

    private void Awake()
    {
        rdr = GetComponent<SpriteRenderer>();
        transform.localScale = new Vector3( GameSetting.GearWidth, GameSetting.MeasureHeight, 1f );
    }

    private void LateUpdate()
    {
        transform.localPosition = new Vector2( 0, GameSetting.JudgePos + ( float )( ( time - NowPlaying.ScaledPlayback ) * GameSetting.Weight ) );
        if ( transform.localPosition.y <= GameSetting.JudgePos )
        {
            rdr.enabled = false;
            pool.Despawn( this );
        }
    }

    public void SetInfo( double _time )
    {
        rdr.enabled = true;
        time = _time;
    }
}
