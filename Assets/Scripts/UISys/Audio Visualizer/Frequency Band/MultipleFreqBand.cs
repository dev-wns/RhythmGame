using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultipleFreqBand : FrequencyBand
{
    [Header("Frequency")]
    [Range(1, 100)] public int   freqCount;
    [Range(1, 100)] public int   startIndex;
    [Range(1, 100)] public int   multipleCount;
    [Range(0f, 1f)] public float multiplePower;
    public float Average { get; private set; }

    protected override void Initialize()
    {
        freqBand = new float[freqCount];
    }

    protected override void UpdateFreqBand( float[][] _values )
    {
        Average = 0f;
        for ( int i = 0; i < freqCount; i++ )
        {
            float sum = 0f;
            for ( int j = 0; j < multipleCount; j++ )
            {
                int index = startIndex + ( i * multipleCount ) + j;
                sum += ( ( _values[0][index] + _values[1][index] ) * .5f ) * index;
            }
            freqBand[i] = ( sum / multipleCount ) * power;
            Average += freqBand[i];
        }
        Average = Average / freqCount;

        OnUpdateBand?.Invoke( freqBand );
    }
}
