using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasureRenderer : MonoBehaviour
{
    public MeasureSystem system;
    private ObjectPool<MeasureRenderer> mPool;
    private float time;

    public void SetInfo( ObjectPool<MeasureRenderer> _pool, float _time )
    {
        mPool = _pool;
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
            gameObject.SetActive( false );
            mPool.Despawn( this );
        }
    }

    private void LateUpdate()
    {
        transform.position = new Vector3( 0, GlobalSetting.JudgeLine + ( ( time - NowPlaying.PlaybackChanged ) * GameSetting.Weight ), 3f );
    }
}
