using UnityEngine;

public class FreqBar : MonoBehaviour
{
    [Header( "[Visualizer]" )]
    [SerializeField] private AudioVisualizer visualizer;
    [SerializeField] private int   startIndex;
    [SerializeField] private int   specCount;
    [SerializeField] private float power;

    [Header( "[Renderer]" )]
    [SerializeField] private SpectrumBar prefab;
    [SerializeField] private float width;
    [SerializeField] private float blank;
    [SerializeField] private float alpha;
    [SerializeField] private float riseAmount;
    [SerializeField] private float dropAmount;

    private SpectrumBar[] bars;
    private float[]       buffer;
    private readonly int  AverageRange = 2;

    [Header( "[Option]" )]
    [SerializeField] private bool isReverse;

    private void Awake()
    {
        visualizer.OnUpdate += UpdateSpectrums;
        CreateSpectrum();
    }

    private void OnDestroy()
    {
        visualizer.OnUpdate -= UpdateSpectrums;
    }

    private void CreateSpectrum()
    {
        buffer = new float[specCount];
        bars   = new SpectrumBar[specCount * 2];
        
        int symmetryColorIdx = 0;
        for ( int i = 0; i < specCount * 2; i++ )
        {
            Color color = i < specCount ? GetGradationColor( specCount - i ) : GetGradationColor( specCount - symmetryColorIdx++ );
            Vector3 pos = i < specCount ? new Vector3( -GetIndexToPositionX( i             ), 0f, 0f ) :
                                          new Vector3(  GetIndexToPositionX( i - specCount ), 0f, 0f );
            
            bars[i] = Instantiate( prefab, transform );
            bars[i].Initailze( pos, color );
        }
    }

    private void UpdateSpectrums( float[][] _values )
    {
        for ( int i = 0; i < specCount; i++ )
        {
            float sumValue = 0f;
            int index = isReverse ? startIndex + specCount - i - 1 : startIndex + i;
            int start = Global.Math.Clamp( index - AverageRange,     0, 4095 );
            int end   = Global.Math.Clamp( index + AverageRange + 1, 0, 4095 );
            for ( int j = start; j < end; j++ )
            {
                float mono = ( _values[0][j] + _values[1][j] ) * .5f;
                sumValue += mono * power;
            }

            float value = sumValue / ( end - start );
            if ( value > buffer[i] ) buffer[i] += riseAmount * Global.Math.Max( 0f, value - buffer[i] ) * Time.deltaTime;
            else                     buffer[i] -= dropAmount * Global.Math.Max( 0f, buffer[i] - value ) * Time.deltaTime;
            buffer[i] = Global.Math.Max( 0f, buffer[i] );

            SpectrumBar left  = bars[i];
            SpectrumBar right = bars[specCount + i];
            left.LocalScale = right.LocalScale = new Vector3( width, buffer[i], 1f );
        }
    }

    private float GetIndexToPositionX( int _index )
    {
        return _index == 0 ? ( width + blank ) * .5f : ( ( width + blank ) * ( _index + 1 ) ) - ( ( width + blank ) * .5f );
    }

    private Color GetGradationColor( int _index )
    {
        float a = ( 1f - ( ( 1f / specCount * ( specCount - _index ) ) ) ) / .25f;
        int X   = Mathf.FloorToInt( a );
        int Y   = Mathf.FloorToInt( 255 * ( a - X ) );
        int r   = 0, g = 0, b = 0;
        switch ( X )
        {
            case 0: r = 255;     g = Y;       b = 0;   break;
            case 1: r = 255 - Y; g = 255;     b = 0;   break;
            case 2: r = 0;       g = 255;     b = Y;   break;
            case 3: r = 0;       g = 255 - Y; b = 255; break;
            case 4: r = Y;       g = 0;       b = 255; break;
            case 5: r = 255;     g = 0;       b = 255; break;
        }

        return new Color( r / 255.0f, g / 255.0f, b / 255.0f, alpha );
    }
}