using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasureRenderer : MonoBehaviour
{
    private InGame scene;
    private ObjectPool<MeasureRenderer> pool;
    private double time;

    private SpriteRenderer rdr;

    private void Awake()
    {
        var sceneObj = GameObject.FindGameObjectWithTag( "Scene" );
        if ( !sceneObj.TryGetComponent<InGame>( out scene ) )
             Debug.LogError( "Game scene component not found." );


        rdr = GetComponent<SpriteRenderer>();
        transform.localScale = new Vector3( GameSetting.GearWidth, GameSetting.MeasureHeight, 1f );
    }

    private void LateUpdate()
    {
        var pos = GameSetting.JudgePos + ( float )( ( time - NowPlaying.PlaybackChanged ) * GameSetting.Weight );
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
