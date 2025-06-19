using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FreqBand : MonoBehaviour
{
    public AudioVisualizer visualizer;
    public           float power;
    public           float dropAmount;
    public           float riseAmount;
    [SerializeField] 
    [Range( 1, 10 )] int  bandCount;
    public           bool isNormalized;
    public           bool isReverse;

    public List<Image> leftBars  = new List<Image>();
    public List<Image> rightBars = new List<Image>();
    
    private float[]   buffer;
    private const int NormalizedRange = 1;

    private void Awake()
    {
        visualizer.OnUpdateBand += UpdateBand;
        buffer = new float[AudioVisualizer.MaxFreqBand];

        if ( isReverse ) rightBars.Reverse();
        else              leftBars.Reverse();
    }

    private void UpdateBand( float[] _values )
    {
        for ( int i = 0; i < bandCount; i++ )
        {
            float value = _values[i] * power;
            if ( isNormalized )
            {
                float sumValue = 0f;
                int start = Global.Math.Clamp( i,                   0, bandCount - 1 );
                int end   = Global.Math.Clamp( i + NormalizedRange, 0, bandCount - 1 );
                for ( int idx = start; idx <= end; idx++ )
                      sumValue += _values[idx];

                value = ( sumValue / ( end - start + 1 ) ) * power;
            }

            buffer[i] = buffer[i] < value ? value :
                                            Global.Math.Clamp( buffer[i] - ( ( .001f + buffer[i] ) * dropAmount * Time.fixedDeltaTime ), 0f, 1f );

            leftBars[i].color = rightBars[i].color = new Color( 1, 1, 1, buffer[i] );
        }
    }
}
