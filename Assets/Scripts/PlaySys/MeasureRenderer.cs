using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasureRenderer : MonoBehaviour
{
    public MeasureSystem system;
    private float time;

    private SpriteRenderer rdr;

    public void SetInfo( MeasureSystem _system, float _time )
    {
        rdr.enabled = true;
        system = _system;
        time = _time;
    }

    private void Awake()
    {
        rdr = GetComponent<SpriteRenderer>();
        transform.localScale = new Vector3( GameSetting.GearWidth, GameSetting.MeasureHeight, 1f );
    }

    private void LateUpdate()
    {
        var pos = GameSetting.JudgePos + ( ( time - NowPlaying.PlaybackChanged ) * GameSetting.Weight );
        transform.position = new Vector3( 0, pos, 3f );

        if ( pos <= GameSetting.JudgePos )
        {
            rdr.enabled = false;
            system.Despawn( this );
        }
    }
}
