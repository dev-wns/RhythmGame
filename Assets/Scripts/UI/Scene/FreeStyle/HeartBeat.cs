using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HeartBeat : MonoBehaviour
{
    public FreeStyleMainScroll mainScroll;
    public SoundPitchOption pitchOption;
    private RectTransform rt => transform as RectTransform;
    private float startSize, endSize;

    [Header( "BPM" )]
    private double curBPM;
    private float previewTime;
    private float spb;

    public float duration = .15f;
    public float power = 1.25f;
    private float time = 0f;
    private bool isStop = true;

    private void Awake()
    {
        mainScroll.OnSelectSong   += UpdateSong;
        mainScroll.OnSoundRestart += UpdateSong;
        pitchOption.OnPitchUpdate += UpdatePitch;

        startSize = rt.sizeDelta.x;
        endSize = startSize * power;
    }

    private void UpdatePitch( float _pitch )
    {
        UpdateBPM( curBPM * _pitch );
    }

    private void UpdateSong( Song _song )
    {
        isStop      = false;
        curBPM      = _song.mainBPM;
        previewTime = _song.previewTime * GameSetting.CurrentPitch * .001f;
        UpdateBPM( curBPM * GameSetting.CurrentPitch );
    }

    private void UpdateBPM( double _bpm )
    {
        spb  = ( float )( 60d / _bpm );
        time = previewTime % spb;
        rt.sizeDelta = new Vector2( startSize, startSize );
    }

    private void Update()
    {
        if ( isStop ) return;

        time += Time.deltaTime;
        if ( spb < time )
        {
            time %= spb;
        }

        //float t = ( 1f + Mathf.Cos( time ) ) * .5f;
        float cos = Mathf.Cos( ( Global.Math.Clamp( time, 0f, duration ) / duration ) * Mathf.PI );
        float t    = ( 1f + cos ) * .5f; // 1 -> 0
        float size = Global.Math.Clamp( Global.Math.Lerp( startSize, endSize, t ), startSize, endSize );
        rt.sizeDelta = new Vector2( size, size );
    }
}
