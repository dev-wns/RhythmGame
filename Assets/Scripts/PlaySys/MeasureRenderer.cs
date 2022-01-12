using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasureRenderer : MonoBehaviour
{
    public MeasureSystem system;
    private float time;

    private SpriteRenderer rdr;
    private bool isActive = false;

    public void SetInfo( MeasureSystem _system, float _time )
    {
        rdr.enabled = true;
        system = _system;
        time = _time;
        isActive = true;
    }

    private void Awake()
    {
        rdr = GetComponent<SpriteRenderer>();
        transform.localScale = new Vector3( GlobalSetting.GearWidth, GlobalSetting.MeasureHeight, 1f );
    }

    private void LateUpdate()
    {
        if ( !isActive ) return;

        var pos = GlobalSetting.JudgeLine + ( ( time - NowPlaying.PlaybackChanged ) * GameSetting.Weight );
        transform.position = new Vector3( 0, pos, 3f );

        if ( pos <= GlobalSetting.JudgeLine )
        {
            isActive = false;
            rdr.enabled = false;
            system.Despawn( this );
        }
    }
}
