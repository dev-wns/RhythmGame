using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FreqColor : MonoBehaviour
{
    public FixedFreqBand freqBand;
    public List<Image> leftBars  = new List<Image>();
    public List<Image> rightBars = new List<Image>();
    public bool isReverse;

    [Header("Frequency")]
    private float[] buffer;
    private int     freqCount;
    public float decreasePower = 1f;

    [Header( "Normalize" )]
    public bool isNormalized;
    public int NormalizedRange = 2;

    private void Awake()
    {
        freqBand.OnUpdateBand += UpdateColors;
        freqCount = ( int )( ( leftBars.Count + rightBars.Count ) * .5f );
        buffer = new float[freqCount];

        if ( isReverse ) rightBars.Reverse();
        else leftBars.Reverse();
    }

    private void UpdateColors( float[] _values )
    {
        for ( int i = 0; i < freqCount; i++ )
        {
            float value = _values[i];
            if ( isNormalized )
            {
                float sumValue = 0f;
                int start = Global.Math.Clamp( i - NormalizedRange, 0, freqCount );
                int end   = Global.Math.Clamp( i + NormalizedRange, 0, freqCount );
                for ( int idx = start; idx < end; idx++ )
                    sumValue += _values[idx];

                value = sumValue / ( end - start + 1 );
            }

            buffer[i] = buffer[i] < value ? value :
                                            Global.Math.Clamp( buffer[i] - ( ( .001f + buffer[i] ) * decreasePower * Time.deltaTime ), 0f, 1f );

            leftBars[i].color = rightBars[i].color = new Color( 1, 1, 1, buffer[i] );
        }
    }
}
