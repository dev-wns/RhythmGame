using System.Collections.Generic;
using UnityEngine;

public class FreqBand : MonoBehaviour
{
    [Header( "[Visualizer]" )]
    [SerializeField] private AudioVisualizer visualizer;
    [Range( 1,  10   )] [SerializeField] private int bandCount;
    [Range( 0,  4086 )] [SerializeField] private int startIndex;
    [Range( 10, 4096 )] [SerializeField] private int avgRange;
    [Min( 0f )]         [SerializeField] private float power;
    [SerializeField, Title] private int size;

    [Header( "[Renderer]" )]
    [Min( 0f )] [SerializeField] private float dropAmount;
    [Min( 0f )] [SerializeField] private float riseAmount;

    [SerializeField] private List<SpriteRenderer> leftBars  = new ();
    [SerializeField] private List<SpriteRenderer> rightBars = new ();

    private float[] bands;
    private int[]   bandStart;
    private int[]   bandEnd;
    private float[] buffer;

    [Header( "[Option]" )]
    [SerializeField] private bool isReverse;
    private readonly int AverageRange = 1;


    private void Awake()
    {
        visualizer.OnUpdate += UpdateBand;
        bands  = new float[bandCount];
        buffer = new float[bandCount];

        if ( isReverse ) rightBars.Reverse();
        else              leftBars.Reverse();

        bandStart = new int[bandCount];
        bandEnd   = new int[bandCount];

        int endIndex = Global.Math.Clamp( startIndex + avgRange, 0, 4096 );
        size         = Global.Math.Clamp( endIndex - startIndex, 0, 4096 );
        for ( int i = 0; i < bandCount; i++ )
        {
            float p0 = ( float )i / bandCount;
            float p1 = ( float )( i + 1 ) / bandCount;

            int start = Mathf.FloorToInt( Mathf.Pow( size, p0 ) ) - 1; // Index 0부터 시작
            int end   = Mathf.FloorToInt( Mathf.Pow( size, p1 ) ) - 1; 

            start = Global.Math.Clamp( start, 0, size - 1 );
            end   = Global.Math.Clamp( end, start + 1, size );

            bandStart[i] = startIndex + start;
            bandEnd[i]   = startIndex + end;
        }
    }

    private void OnDestroy()
    {
        visualizer.OnUpdate -= UpdateBand;
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


    private void UpdateBand( float[][] _values )
    {
         for ( int i = 0; i < bandCount; i++ )
         {
             int start = bandStart[i];
             int end   = bandEnd[i];
         
             float sumValue = 0f;
             for ( int j = start; j < end; j++ )
             {
                 float mono = ( _values[0][j] + _values[1][j] ) *.5f;
                 sumValue += mono;
             }
         
             bands[i] = sumValue / ( end - start ) * power;
         }
         
         for ( int i = 0; i < bandCount; i++ )
         {
             float sumValue = 0f;
             int start = Global.Math.Clamp( i - AverageRange,     0, bandCount - 1 );
             int end   = Global.Math.Clamp( i + AverageRange + 1, 0, bandCount - 1 );
             for ( int j = start; j < end; j++ )
                   sumValue += bands[j];
         
             float value = sumValue / ( end - start );
             if ( value > buffer[i] ) buffer[i] += riseAmount * Global.Math.Max( 0f, value - buffer[i] ) * Time.deltaTime;
             else                     buffer[i] -= dropAmount * Global.Math.Max( 0f, buffer[i] - value ) * Time.deltaTime;
             buffer[i] = Global.Math.Clamp01( buffer[i] );
         
             leftBars[i].color = rightBars[i].color = new Color( 1f, 1f, 1f, buffer[i] );
         }
    }
}
