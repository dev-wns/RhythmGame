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
    private float spb, spbHalf, spbQuarter;
    private float time = 0f;
    private bool isWaitTime = true;

    private void Awake()
    {
        mainScroll.OnSelectSong   += UpdateSong;
        mainScroll.OnSoundRestart += UpdateSong;
        pitchOption.OnPitchUpdate += UpdatePitch;

        startSize = rt.sizeDelta.x;
        endSize   = rt.sizeDelta.x * 1.75f;
    }

    private void UpdatePitch( float _pitch )
    {
        UpdateBPM( curBPM * _pitch );
    }

    private void UpdateSong( Song _song )
    {
        curBPM      = _song.medianBpm;
        previewTime = _song.previewTime * .001f;
        UpdateBPM( curBPM * GameSetting.CurrentPitch );
    }

    private void UpdateBPM( double _bpm )
    {
        spb        = ( float )( 60d / _bpm );
        spbHalf    = spb * .5f;
        spbQuarter = spb * .25f;

        time         = previewTime - ( spb * Mathf.FloorToInt( previewTime / spb ) );
        isWaitTime   = false;
        rt.sizeDelta = new Vector2( startSize, startSize );
    }

    private void Update()
    {
        time += Time.deltaTime;
        if ( isWaitTime && spb < time )
        {
            time       -= spb;
            isWaitTime = false;
        }
        else
        {
            float t    = Mathf.Cos( ( 1f + ( time / spbQuarter ) ) * .5f ); // 0 ~ 1
            float size = Global.Math.Lerp( startSize, endSize, t );

            size = Global.Math.Clamp( size, startSize, endSize );
            rt.sizeDelta = new Vector2( size, size );

            if ( spbHalf < time )
                 isWaitTime = true;
        }
    }
}
