using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Measure : MonoBehaviour
{
    private float time; // calculated timing

    public void Initialized( float _time )
    {
        time = _time;
    }

    private void Awake()
    {
        transform.localScale = new Vector3( GlobalSetting.GearWidth, GlobalSetting.MeasureHeight, 1f );
    }

    private void Update()
    {
        if ( transform.position.y <= -Screen.height * .5f )
        {
            MeasureSystem.mPool.Despawn( this );
        }
    }

    private void LateUpdate()
    {
        transform.position = new Vector3( 0, GlobalSetting.JudgeLine + ( ( time - NowPlaying.PlaybackChanged ) * NowPlaying.Weight ), 0f );
    }
}
