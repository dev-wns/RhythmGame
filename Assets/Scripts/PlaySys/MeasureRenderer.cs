using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasureRenderer : MonoBehaviour
{
    private MeasureSystem MSystem;
    private float time;

    public void Initialized( float _time )
    {
        time = _time;
    }

    private void Awake()
    {
        MSystem = GameObject.FindGameObjectWithTag( "Systems" ).GetComponent<MeasureSystem>();
        transform.localScale = new Vector3( GlobalSetting.GearWidth, GlobalSetting.MeasureHeight, 1f );
    }

    private void Update()
    {
        if ( transform.position.y <= -Screen.height * .5f )
        {
            gameObject.SetActive( false );
            MSystem.mPool.Despawn( this );
        }
    }

    private void LateUpdate()
    {
        transform.position = new Vector3( 0, GlobalSetting.JudgeLine + ( ( time - NowPlaying.PlaybackChanged ) * GameSetting.Weight ), 3f );
    }
}
