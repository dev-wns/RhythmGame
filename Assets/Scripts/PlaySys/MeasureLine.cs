using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasureLine : MonoBehaviour
{
    private RectTransform tf;
    private float hitTiming;

    private void Awake()
    {
        tf = GetComponent<RectTransform>();
    }

    public void SetInfo( float _hitTiming )
    {
        hitTiming = _hitTiming;
    }

    private void LateUpdate()
    {
        tf.anchoredPosition = new Vector2( 0, GlobalSetting.JudgeLine + ( ( hitTiming - InGame.PlaybackChanged ) * GlobalSetting.BPMWeight * GlobalSetting.ScrollSpeed ) );
    }
}
