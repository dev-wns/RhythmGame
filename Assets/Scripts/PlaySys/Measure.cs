using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Measure : MonoBehaviour
{
    private float timing;
    private float weight;

    private void OnEnable()
    {
        transform.localScale = new Vector3( GlobalSetting.GearWidth, GlobalSetting.MeasureHeight, 1f );
    }

    public void SetInfo( float _timing, float _weight )
    {
        timing = _timing;
        weight = _weight;
    }
    private void Update()
    {
        if ( transform.position.y <= GlobalSetting.JudgeLine - .5f )
        {
            InGame.mPool.Despawn( this );
        }
    }

    private void LateUpdate()
    {
        transform.position = new Vector3( 0, GlobalSetting.JudgeLine + ( ( timing - InGame.PlaybackChanged ) * weight ), 0f );
    }
}
