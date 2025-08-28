using System;
using UnityEngine;

[RequireComponent( typeof( RectTransform ) )]
public class FreqSpark : MonoBehaviour
{
    [Header( "- Visualizer -" )]
    public AudioVisualizer visualizer;
    public int   startIndex;
    public int   freqCount;
    public int   power;

    [Header( "- Renderer -" )]
    public LineRenderer rdr;
    public float maxHeight;
    public float posOffset;
    public float discardOffset;
    public float riseAmount;
    public float dropAmount;
    public bool  isAxisX;
    public bool  isReverse;

    private float[] buffer;
    private Vector3[]   positions;
    private Vector2     startPos;
    private Vector2     pos;

    [Header( "- Normalize -" )]
    public bool isNormalized;
    public int NormalizedRange;

    [Min( 1 )] public int topCount;
    private int[]   topIndexes;
    private float[] topValues;
    private float[] falloff = new float[] { 1f, .87f, .64f, .36f, .19f, .09f };

    private void Awake()
    {
        visualizer.OnUpdate += UpdateSpark;
        buffer               = new float[freqCount];
        positions            = new Vector3[freqCount * 4]; // +값, -값, 좌우반전
        rdr.positionCount    = freqCount * 4;

        topIndexes = new int[topCount];
        topValues  = new float[topCount];

        startPos = transform.position;
        pos      = ( transform as RectTransform ).anchoredPosition;

        for ( int i = 0; i < freqCount * 2; i++ )
        {
            positions[i]                     = isAxisX ? new Vector2( startPos.x - ( posOffset * .5f ) - ( posOffset * ( ( freqCount * 2 ) - i - 1 ) ), pos.y )
                                                       : new Vector2( pos.x, ( startPos.y - ( posOffset * .5f ) - ( posOffset * ( ( freqCount * 2 ) - i - 1 ) ) ) );
            positions[( freqCount * 2 ) + i] = isAxisX ? new Vector2( startPos.x + ( posOffset * .5f ) + ( posOffset * i ), pos.y )
                                                       : new Vector2( pos.x, ( startPos.y + ( posOffset * .5f ) + ( posOffset * i ) ) );
        }
    }

    //private void UpdateSpark( float[] _values )
    //{
    //    for ( int i = 0; i < freqCount; i++ )
    //    {
    //        int   index   = isReverse ? freqCount - i - 1 : i;
    //        float value   = Global.Math.Clamp( _values[startIndex + i] * power, 0f, maxHeight );
    //        float discard = visualizer.Average * discardOffset;
    //        if ( isNormalized )
    //        {
    //            float sumValue = 0f;
    //            int start = Global.Math.Clamp( i - NormalizedRange, 0, freqCount - 1 );
    //            int end   = Global.Math.Clamp( i + NormalizedRange, 0, freqCount - 1 );
    //            for ( int idx = start; idx <= end; idx++ )
    //            {
    //                float __value = ( _values[startIndex + idx] - discard ) * power;
    //                sumValue += Global.Math.Clamp( ( __value * __value ), 0f, maxHeight );
    //            }

    //            value = Global.Math.Clamp( sumValue / ( end - start + 1 ), 0f, maxHeight );
    //        }
    //        buffer[i] = Global.Math.Max( value, buffer[i] );
    //        buffer[i] -= Global.Math.Lerp( 0f, Global.Math.Abs( buffer[i] - value ), dropAmount * Time.deltaTime );

    //        // UI 갱신
    //        int number = ( index * 2 );
    //        positions[( freqCount * 2 ) - number]     = isAxisX ? new Vector3( positions[( freqCount * 2 ) - number].x,     pos.y - buffer[i] )
    //                                                            : new Vector3( pos.x - buffer[i],                           positions[( freqCount * 2 ) - number].y );
    //        positions[( freqCount * 2 ) - number - 1] = isAxisX ? new Vector3( positions[( freqCount * 2 ) - number - 1].x, pos.y + buffer[i] )
    //                                                            : new Vector3( pos.x + buffer[i],                           positions[( freqCount * 2 ) - number - 1].y );

    //        positions[( freqCount * 2 ) + number]     = isAxisX ? new Vector3( positions[( freqCount * 2 ) + number].x,     pos.y - buffer[i] )
    //                                                            : new Vector3( pos.x - buffer[i],                           positions[( freqCount * 2 ) + number].y );
    //        positions[( freqCount * 2 ) + number + 1] = isAxisX ? new Vector3( positions[( freqCount * 2 ) + number + 1].x, pos.y + buffer[i] )
    //                                                            : new Vector3( pos.x + buffer[i],                           positions[( freqCount * 2 ) + number + 1].y );
    //    }

    //    rdr.SetPositions( positions );
    //}

    private void UpdateSpark( float[] _values )
    {
        Array.Fill( topValues, 0f );
        Array.Fill( topIndexes, 0 );

        // 큰 수 찾기
        for ( int i = 0; i < freqCount; i++ )
        {
            float value = _values[startIndex + i];
            if ( value <= topValues[topCount - 1] )
                continue;

            for ( int j = topCount - 1; j >= 0; j-- )
            {
                if ( j == 0 || value <= topValues[j - 1] )
                {
                    topValues[j] = value;
                    topIndexes[j] = i;
                    break;
                }

                topValues[j] = topValues[j - 1];
                topIndexes[j] = topIndexes[j - 1];
            }
        }

        for ( int i = 0; i < freqCount; i++ )
        {
            float value = 0f;
            for ( int t = 0; t < topCount; t++ )
            {
                int absOffset = Global.Math.Abs( i - topIndexes[t] );
                if ( absOffset <= 5 && absOffset < falloff.Length )
                {
                    float v = Global.Math.Clamp( topValues[t] * power, 0f, maxHeight ) * falloff[absOffset];
                    value = Global.Math.Max( value, v );
                }
            }

            buffer[i] = Global.Math.Max( value, buffer[i] );
            buffer[i] -= Global.Math.Lerp( 0f, Global.Math.Abs( buffer[i] - value ), dropAmount * Time.deltaTime );

            // UI 갱신
            int index  = isReverse ? freqCount - i - 1 : i;
            int number = ( index * 2 );
            positions[( freqCount * 2 ) - number] = isAxisX ? new Vector3( positions[( freqCount * 2 ) - number].x, pos.y - buffer[i] )
                                                                : new Vector3( pos.x - buffer[i], positions[( freqCount * 2 ) - number].y );
            positions[( freqCount * 2 ) - number - 1] = isAxisX ? new Vector3( positions[( freqCount * 2 ) - number - 1].x, pos.y + buffer[i] )
                                                                : new Vector3( pos.x + buffer[i], positions[( freqCount * 2 ) - number - 1].y );

            positions[( freqCount * 2 ) + number] = isAxisX ? new Vector3( positions[( freqCount * 2 ) + number].x, pos.y - buffer[i] )
                                                                : new Vector3( pos.x - buffer[i], positions[( freqCount * 2 ) + number].y );
            positions[( freqCount * 2 ) + number + 1] = isAxisX ? new Vector3( positions[( freqCount * 2 ) + number + 1].x, pos.y + buffer[i] )
                                                                : new Vector3( pos.x + buffer[i], positions[( freqCount * 2 ) + number + 1].y );
        }

        rdr.SetPositions( positions );
    }

    //private void UpdateSpark( float[][] _values )
    //{
    //    int   peakIndex = 0;
    //    float peakValue = float.Epsilon;
    //    for ( int i = 0; i < freqCount; i++ )
    //    {
    //        float value = ( _values[0][startIndex + i] + _values[1][startIndex + i] ) *.5f * power;
    //        if ( value > peakValue )
    //        {
    //            peakValue = value;
    //            peakIndex = i;
    //        }
    //    }

    //    if ( peakValue < 20f )
    //        peakValue = 0f;

    //    for ( int i = 0; i < freqCount; i++ )
    //    {
    //        float value = 0f;
    //        if ( isNormalized )
    //        {
    //            int  absOffset = Global.Math.Abs( i - peakIndex );
    //            if ( absOffset <= 5 && absOffset < falloff.Length )
    //                value = Global.Math.Clamp( peakValue * power, 0f, maxHeight ) * falloff[absOffset];
    //        }

    //        buffer[i] += buffer[i] < value ? Global.Math.Lerp( 0f, Global.Math.Abs( buffer[i] - value ), riseAmount * Time.deltaTime ) :
    //                                        -Global.Math.Lerp( 0f, Global.Math.Abs( buffer[i] - value ), dropAmount * Time.deltaTime );
    //        buffer[i] = Global.Math.Max( 0f, buffer[i] );

    //        // UI 갱신
    //        int index  = isReverse ? freqCount - i - 1 : i;
    //        int number = ( index * 2 );
    //        positions[( freqCount * 2 ) - number] = isAxisX ? new Vector3( positions[( freqCount * 2 ) - number].x, pos.y - buffer[i] )
    //                                                            : new Vector3( pos.x - buffer[i], positions[( freqCount * 2 ) - number].y );
    //        positions[( freqCount * 2 ) - number - 1] = isAxisX ? new Vector3( positions[( freqCount * 2 ) - number - 1].x, pos.y + buffer[i] )
    //                                                            : new Vector3( pos.x + buffer[i], positions[( freqCount * 2 ) - number - 1].y );

    //        positions[( freqCount * 2 ) + number] = isAxisX ? new Vector3( positions[( freqCount * 2 ) + number].x, pos.y - buffer[i] )
    //                                                            : new Vector3( pos.x - buffer[i], positions[( freqCount * 2 ) + number].y );
    //        positions[( freqCount * 2 ) + number + 1] = isAxisX ? new Vector3( positions[( freqCount * 2 ) + number + 1].x, pos.y + buffer[i] )
    //                                                            : new Vector3( pos.x + buffer[i], positions[( freqCount * 2 ) + number + 1].y );
    //    }

    //    rdr.SetPositions( positions );
    //}
}
