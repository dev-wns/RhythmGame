using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasureRenderer : MonoBehaviour
{
    private MeasureSystem system;
    private double time;

    private SpriteRenderer rdr;

    public void SetInfo( MeasureSystem _system, double _time )
    {
        rdr.enabled = true;
        system = _system;
        time = _time;
    }

    private void Awake()
    {
        rdr = GetComponent<SpriteRenderer>();
        transform.localScale = new Vector2( GameSetting.GearWidth, GameSetting.MeasureHeight );
    }

    private void LateUpdate()
    {
        var pos = GameSetting.JudgePos + ( ( time - NowPlaying.PlaybackChanged ) * GameSetting.Weight );
        transform.position = new Vector2( 0, ( float )pos );

        if ( pos <= GameSetting.JudgePos )
        {
            rdr.enabled = false;
            system.Despawn( this );
        }
    }
}
