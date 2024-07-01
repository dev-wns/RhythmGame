using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectFreqBand : FrequencyBand
{
    private readonly float Frequency = 48000f;
    private readonly float FFTSize   = 4096f;
    private float hzPerFFTSize;


    [Header( "Range" )]
    [Range(0, 4096)] public int start;
    [Range(0, 4096)] public int end;
    [SerializeField] private float startHz;
    [SerializeField] private float endHz;
    [SerializeField] private int   range;

    protected override void Awake()
    {
        base.Awake();

        hzPerFFTSize = Frequency / FFTSize;
    }

    protected override void Initialize()
    {
        freqBand = new float[1];
    }

    protected override void UpdateFreqBand( float[][] _values )
    {
        range = end - start;
        if ( range <= 0 ) return;

        float sum = 0f;
        //int count = 1;
        for ( int i = start; i < end; i++ )
        {
            sum += ( ( _values[0][i] + _values[1][i] ) * .5f );// * count++;
        }
        freqBand[0] = ( sum / range ) * power;

        startHz = start * hzPerFFTSize;
        endHz   = end   * hzPerFFTSize;

        OnUpdateBand?.Invoke( freqBand );
    }
}
