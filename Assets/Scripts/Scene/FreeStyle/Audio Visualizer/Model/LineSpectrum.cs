using Newtonsoft.Json.Linq;
using NUnit;
using System;
using UnityEngine;

public class LineSpectrum : MonoBehaviour
{
    [Header( "- Visualizer -" )]
    public AudioVisualizer visualizer;
    public int   startIndex;
    public int   specCount;
    public float power;

    [Header( "- Renderer -" )]
    public Transform prefab;
    public float width;
    public float blank;
    public float alpha;
    public int   sortingOrder;
    public float dropAmount;
    public bool  isReverse;

    private float[]     buffer;
    private Transform[] transforms; // 생성된 모델 안의 Transform

    [Header( "- Normalize -" )]
    public bool IsNormalized;
    public int  normalizedRange;

    private void Awake()
    {
        if ( visualizer is not null )
        {
            visualizer.OnUpdate += UpdateSpectrums;
            CreateSpectrumModel();
        }
    }

    private void CreateSpectrumModel()
    {
        buffer = new float[specCount];
        transforms = new Transform[specCount * 2];
        int symmetryColorIdx = 0;

        for ( int i = 0; i < specCount * 2; i++ )
        {
            transforms[i] = Instantiate( prefab, transform );

            var rdr                = transforms[i].GetComponent<SpriteRenderer>();
            rdr.sortingOrder       = sortingOrder;
            rdr.color              = i < specCount ? GetGradationColor( specCount - i ) : GetGradationColor( specCount - symmetryColorIdx++ );
            transforms[i].position = i < specCount ? new Vector3( transform.position.x - GetIndexToPositionX( i ),             transform.position.y, transform.position.z ) :
                                                     new Vector3( transform.position.x + GetIndexToPositionX( i - specCount ), transform.position.y, transform.position.z );
        }
    }

    private void UpdateSpectrums( float[] _values )
    {
        for ( int i = 0; i < specCount; i++ )
        {
            // 인스펙터 상의 스펙트럼 시작위치부터 값을 받아온다.
            int index   = isReverse ? startIndex + specCount - i - 1 : startIndex + i;
            float value = _values[index] * power;
            if ( IsNormalized )
            {
                float sumValue = 0f;
                int start = Global.Math.Clamp( index - normalizedRange, 0, 4096 );
                int end   = Global.Math.Clamp( index + normalizedRange, 0, 4096 );
                for ( int idx = start; idx <= end; idx++ )
                      sumValue += _values[idx] * power;

                value = sumValue / ( end - start + 1 );
            }

            buffer[i] =  Global.Math.Max( value, buffer[i] );
            buffer[i] -= Global.Math.Lerp( 0f, Global.Math.Abs( buffer[i] - value ), dropAmount * Time.deltaTime );

            Transform left  = transforms[i];
            Transform right = transforms[specCount + i];
            left.localScale = right.localScale = new Vector3( width, buffer[i], 1f );
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