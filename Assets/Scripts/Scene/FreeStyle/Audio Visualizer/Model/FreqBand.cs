using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FreqBand : MonoBehaviour
{
    [Header( "- Visualizer -" )]
    public AudioVisualizer visualizer;
    [SerializeField] 
    [Range( 1, 10 )] int   bandCount;
    public           float power;

    [Header( "- Renderer -" )]
    public           float dropAmount;
    public           float riseAmount;
    public           bool  isNormalized;
    public           bool  isReverse;

    public List<SpriteRenderer> leftBars  = new List<SpriteRenderer>();
    public List<SpriteRenderer> rightBars = new List<SpriteRenderer>();
    
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
                int start = Global.Math.Clamp( i - NormalizedRange, 0, bandCount );
                int end   = Global.Math.Clamp( i + NormalizedRange, 0, bandCount );
                for ( int idx = start; idx < end; idx++ )
                      sumValue += _values[idx];

                value = ( sumValue / ( end - start + 1 ) ) * power;
            }

            buffer[i] = buffer[i] < value ? value : Global.Math.Clamp( buffer[i] - ( ( .001f + buffer[i] ) * dropAmount * Time.deltaTime ), 0f, 1f );
            leftBars[i].color = rightBars[i].color = new Color( 1, 1, 1, buffer[i] );
        }
    }
}
