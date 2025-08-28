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
    public           bool  isReverse;

    public List<SpriteRenderer> leftBars  = new List<SpriteRenderer>();
    public List<SpriteRenderer> rightBars = new List<SpriteRenderer>();
    private const int MaxFreqBand = 10;

    private float[]   alpha;

    private void Awake()
    {
        visualizer.OnUpdate += UpdateBand;
        alpha    = new float[MaxFreqBand];

        if ( isReverse ) rightBars.Reverse();
        else              leftBars.Reverse();
    }
    /* 48000 / 4096 : 11.71875 Hertz
     * ------------------------------------
     *     count    : Hertz :    Range
     * ------------------------------------
     * 0.  2        : 23    : 0     ~ 23
     * 1.  4        : 47    : 24    ~ 71
     * 2.  8        : 94    : 72    ~ 165
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
    private void UpdateBand( float[] _values )
    {
        int count = 0;
        for ( int i = 0; i < MaxFreqBand; i++ )
        {
            float sumValue = 0f;
            int start = Global.Math.Clamp( i - 1, 0, bandCount );
            int end   = Global.Math.Clamp( i + 1, 0, bandCount );
            for ( int idx = start; idx < end; idx++ )
                sumValue += _values[idx] * power;

            float value = sumValue / ( end - start + 2 );

            alpha[i]  = Global.Math.Max( alpha[i], value );
            alpha[i] -= Global.Math.Lerp( 0f, Global.Math.Abs( alpha[i] - value ), dropAmount * Time.deltaTime );
            alpha[i]  = Global.Math.Clamp01( alpha[i] );

            leftBars[i].color = rightBars[i].color = new Color( 1, 1, 1, alpha[i] );
        }
    }
}
