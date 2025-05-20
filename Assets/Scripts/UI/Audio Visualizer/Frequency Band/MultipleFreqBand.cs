using System;
using UnityEngine;

public class MultipleFreqBand : FrequencyBand
{
    [Header("Frequency")]
    [Range(1, 100)] public int   freqCount;
    [Range(0, 100)] public int   startIndex;
    [Range(1, 100)] public int   multipleCount;
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
                // Type1
                //int index = startIndex + ( i * multipleCount ) + j;
                //sum += ( _values[0][index] + _values[1][index] ) * .5f;

                // Type2
                //int index = j % 2 == 0 ? startIndex + ( freqCount * ( ( j + 1 ) * 2 ) ) - i :   // 20~10 40~30 60~50
                //                         startIndex + ( freqCount * ( j * 2 ) ) + i;            //  0~10 20~30 40~50
                //sum += ( ( _values[0][index] + _values[1][index] ) * .5f );

                // Type3
                int   aIndex = startIndex + ( freqCount * ( ( j + 1 ) * 2 ) ) - i;
                sum += ( _values[0][aIndex] + _values[1][aIndex] ) * .5f;
                //float aValue = ( _values[0][aIndex] + _values[1][aIndex] ) * .5f;
                //sum = sum < aValue ? aValue : sum;

                int   bIndex = startIndex + ( freqCount * ( j * 2 ) ) + i;
                sum += ( _values[0][bIndex] + _values[1][bIndex] ) * .5f;
                //float bValue = ( _values[0][bIndex] + _values[1][bIndex] ) * .5f;
                //sum = sum < bValue ? bValue : sum;
            }

            freqBand[i] = sum * power;
            Average += freqBand[i];
        }
        Average = Average / freqCount;

        OnUpdateBand?.Invoke( freqBand );
    }
}
