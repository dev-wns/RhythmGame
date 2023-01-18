using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private readonly float Duration = .18f;
    private float time = 0f;

    private void Awake()
    {
        mainScroll.OnSelectSong   += UpdateSong;
        mainScroll.OnSoundRestart += UpdateSong;
        pitchOption.OnPitchUpdate += UpdatePitch;

        startSize = rt.sizeDelta.x;
        endSize   = rt.sizeDelta.x * 1.5f;
    }

    private void UpdatePitch( float _pitch )
    {
        UpdateBPM( curBPM * _pitch );
    }

    private void UpdateSong( Song _song )
    {
        curBPM      = _song.medianBpm;
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
        time += Time.deltaTime;
        if ( spb < time )
             time %= spb;

        float t    = ( 1f + Mathf.Cos( ( Global.Math.Clamp( time, 0f, Duration ) / Duration ) * Mathf.PI ) ) * .5f; // 1 -> 0
        float size = Global.Math.Clamp( Global.Math.Lerp( startSize, endSize, t ), startSize, endSize );
        rt.sizeDelta = new Vector2( size, size );
    }
}
