using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrequencyBand : MonoBehaviour
{
    public AudioVisualizer visuzlizer;
    public enum FreqType { FreqBand, BandBuffer, }
    public FreqType type = FreqType.FreqBand;

    private float[] freqBand;
    private float[] bandBuffer;
    private float[] bufferDecrease;
    private readonly int MaxFreqBand = 10;

    [Range(1, 10)]
    public int bandRange = 1;

    public Action<float[]> OnUpdateBand;

    public void Awake()
    {
        freqBand       = new float[MaxFreqBand];
        bandBuffer     = new float[MaxFreqBand];
        bufferDecrease = new float[MaxFreqBand];

        visuzlizer.OnUpdateSpectrums += UpdateFreqBand;
    }


    /* 48000 / 4096 : 11.71875 Hertz
     * ------------------------------------
     *     count    : Hertz :    Range
     * ------------------------------------
     * 0.  2        : 23    : 0     ~ 23
     * 1.  4        : 47    : 23    ~ 70
     * 2.  8        : 94    : 71    ~ 164
     * 3.  16       : 188   : 165   ~ 352
     * 4.  32       : 375   : 353   ~ 727
     * 5.  64       : 750   : 728   ~ 1477
     * 6.  128      : 1500  : 1478  ~ 2977
     * 7.  256      : 3000  : 2978  ~ 5977
     * 8.  512      : 6000  : 5978  ~ 11977
     * 9.  1024     : 12000 : 11978 ~ 23977
     * 10. 2048 + 2 : 24023 : 23978 ~ 48000
     * Total : 4096
     */
    private void UpdateFreqBand( float[][] _values )
    {
        int count = 0;
        for ( int i = 0; i < bandRange; i++ )
        {
            float sum = 0f;
            int sampleCount = ( int )Mathf.Pow( 2, i ) * 2;
            if ( i == 9 ) 
                 sampleCount += 2;

            for ( int j = 0; j < sampleCount; j++ )
            {
                sum += ( _values[0][count] + _values[1][count] ) * .5f;
                count += 1;
            }

            freqBand[i] = sum / sampleCount;
        }

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
        for ( int i = 0; i < bandRange; i++ )
        {
            if ( bandBuffer[i] < freqBand[i] )
            {
                bandBuffer[i] = freqBand[i];
                bufferDecrease[i] = .001f;
            }

            if ( bandBuffer[i] > freqBand[i] )
            {
                bandBuffer[i] -= bufferDecrease[i];
                bufferDecrease[i] *= 1.1f;
            }
        }
    }
}
