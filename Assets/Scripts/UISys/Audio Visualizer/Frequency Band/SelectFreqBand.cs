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
        freqBand       = new float[1];
        bandBuffer     = new float[1];
        bufferDecrease = new float[1];
    }

    protected override void UpdateFreqBand( float[][] _values )
    {
        range = end - start;
        if ( range <= 0 ) return;

        float sum = 0f;
        for ( int i = start; i < end; i++ )
        {
            sum += ( _values[0][i] + _values[1][i] ) * .5f;
        }
        freqBand[0] = ( sum / range ) * power;

        startHz = start * hzPerFFTSize;
        endHz   = end   * hzPerFFTSize;

        switch ( type )
        {
            case FreqType.FreqBand:
            OnUpdateBand?.Invoke( freqBand );
            break;

            case FreqType.BandBuffer:
            UpdateBandBuffer();
            OnUpdateBand?.Invoke( bandBuffer );
            break;
        }
    }

    private void UpdateBandBuffer()
    {
        if ( bandBuffer[0] < freqBand[0] )
        {
            bandBuffer[0] = freqBand[0];
            bufferDecrease[0] = .001f;
        }

        if ( bandBuffer[0] > freqBand[0] )
        {
            bandBuffer[0] -= bufferDecrease[0];
            bufferDecrease[0] *= 1.1f;
        }
    }
}
