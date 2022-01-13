using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasureRenderer : MonoBehaviour
{
    public MeasureSystem system;
    private float time;

    private SpriteRenderer rdr;
    private float weight;

    public void SetInfo( MeasureSystem _system, float _time )
    {
        rdr.enabled = true;
        system = _system;
        time = _time;

        system.scene.OnScrollChanged += ScrollChange;
    }

    private void Awake()
    {
        rdr = GetComponent<SpriteRenderer>();
        transform.localScale = new Vector3( GameSetting.GearWidth, GameSetting.MeasureHeight, 1f );

        weight = GameSetting.Weight;
    }

    private void ScrollChange() => weight = GameSetting.Weight;

    private void LateUpdate()
    {
        var pos = GameSetting.JudgePos + ( ( time - NowPlaying.PlaybackChanged ) * weight );
        transform.position = new Vector3( 0, pos, 3f );

        if ( pos <= GameSetting.JudgePos )
        {
            system.scene.OnScrollChanged -= ScrollChange;
            rdr.enabled = false;
            system.Despawn( this );
        }
    }
}
