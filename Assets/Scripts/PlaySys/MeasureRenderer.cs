using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasureRenderer : MonoBehaviour
{
    private InGame game;
    private MeasureSystem system;
    private double time;

    private SpriteRenderer rdr;
    private double weight;

    private void Awake()
    {
        game = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        game.OnScrollChanged += ScrollUpdate;

        rdr = GetComponent<SpriteRenderer>();
        transform.localScale = new Vector3( GameSetting.GearWidth, GameSetting.MeasureHeight, 1f );

        ScrollUpdate();
    }

    private void OnDestroy() => game.OnScrollChanged -= ScrollUpdate;

    private void ScrollUpdate() => weight = GameSetting.Weight;

    private void LateUpdate()
    {
        var pos = GameSetting.JudgePos + ( float )( ( time - NowPlaying.PlaybackChanged ) * weight );
        transform.localPosition = new Vector2( 0, pos );

        if ( pos <= GameSetting.JudgePos )
        {
            rdr.enabled = false;
            system.Despawn( this );
        }
    }

    public void SetInfo( MeasureSystem _system, double _time )
    {
        rdr.enabled = true;
        system = _system;
        time = _time;
    }
}
