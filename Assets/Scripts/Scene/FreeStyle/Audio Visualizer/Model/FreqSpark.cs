using System;
using UnityEngine;

[RequireComponent( typeof( RectTransform ) )]
public class FreqSpark : MonoBehaviour
{
    [Header( "[Visualizer]" )]
    [SerializeField] private AudioVisualizer visualizer;
    [SerializeField] private int startIndex;
    [SerializeField] private int freqCount;
    [SerializeField] private int power;

    [Header( "[Renderer]" )]
    [SerializeField] private LineRenderer rdr;
    [SerializeField] private float blank;
    [SerializeField] private float maxHeight;
    [SerializeField] private float riseAmount;
    [SerializeField] private float dropAmount;

    [Min( 1 )] [SerializeField] private int topCount;
    private int[]   topIndexes;
    private float[] topValues;
    private readonly float[] falloff = new float[] { 1f, .87f, .64f, .36f, .19f, .09f };

    [SerializeField] private AnimationCurve curve;

    private float[]   buffer;
    private Vector3[] positions;
    private Vector2   pos;

    [Header( "[Option]" )]
    [SerializeField] private bool isAxisX;
    [SerializeField] private bool isReverse;

    private void Awake()
    {
        visualizer.OnUpdate += UpdateSpark;
        buffer               = new float[freqCount];
        positions            = new Vector3[freqCount * 4]; // +값, -값, 좌우반전
        rdr.positionCount    = freqCount * 4;

        topIndexes = new int[topCount];
        topValues  = new float[topCount];

        pos = ( transform as RectTransform ).anchoredPosition;

        Vector3 startPos = transform.position;
        for ( int i = 0; i < freqCount * 2; i++ )
        {
            positions[i]                     = isAxisX ? new Vector2( startPos.x - ( blank * .5f ) - ( blank * ( ( freqCount * 2 ) - i - 1 ) ), pos.y )
                                                       : new Vector2( pos.x, ( startPos.y - ( blank * .5f ) - ( blank * ( ( freqCount * 2 ) - i - 1 ) ) ) );
            positions[( freqCount * 2 ) + i] = isAxisX ? new Vector2( startPos.x + ( blank * .5f ) + ( blank * i ), pos.y )
                                                       : new Vector2( pos.x, ( startPos.y + ( blank * .5f ) + ( blank * i ) ) );
        }
    }

    private void OnDestroy()
    {
        visualizer.OnUpdate -= UpdateSpark;
    }

    private void UpdateSpark( float[][] _values )
    {
        Array.Fill( topValues, 0f );
        Array.Fill( topIndexes, 0 );

        for ( int i = 0; i < freqCount; i++ )
        {
            int index = startIndex + i;
            float value = ( _values[0][index] + _values[1][index] ) * .5f;
            if ( value <= topValues[topCount - 1] ) continue;

            for ( int j = topCount - 1; j >= 0; j-- )
            {
                if ( j == 0 || value <= topValues[j - 1] )
                {
                    topValues[j]  = value;
                    topIndexes[j] = i;
                    break;
                }

                topValues[j]  = topValues[j - 1];
                topIndexes[j] = topIndexes[j - 1];
            }
        }

        for ( int i = 0; i < freqCount; i++ )
        {
            float value = 0f;
            for ( int t = 0; t < topCount; t++ )
            {
                int idxOffset = Global.Math.Abs( i - topIndexes[t] );
                if ( idxOffset < falloff.Length )
                {
                    float v = Global.Math.Clamp( curve.Evaluate( topValues[t] ) * power, 0f, maxHeight ) * falloff[idxOffset];
                    value = Global.Math.Max( value, v );
                }
            }

            if ( value > buffer[i] ) buffer[i] += riseAmount * ( value - buffer[i] ) * Time.deltaTime ;
            else                     buffer[i] -= dropAmount * ( buffer[i] - value ) * Time.deltaTime ;
            buffer[i] = Global.Math.Clamp( buffer[i], 0f, maxHeight );

            // UI 갱신
            int specIndex = isReverse ? freqCount - i - 1 : i;
            int posIndex  = specIndex * 2;
            int halfCount = freqCount * 2; // total = freqCount * 4
            positions[halfCount - posIndex]     = isAxisX ? new Vector3( positions[halfCount - posIndex].x, pos.y - buffer[i] )
                                                          : new Vector3( pos.x - buffer[i], positions[halfCount - posIndex].y );
            positions[halfCount - posIndex - 1] = isAxisX ? new Vector3( positions[halfCount - posIndex - 1].x, pos.y + buffer[i] )
                                                          : new Vector3( pos.x + buffer[i], positions[halfCount - posIndex - 1].y );

            positions[halfCount + posIndex]     = isAxisX ? new Vector3( positions[halfCount + posIndex].x, pos.y - buffer[i] )
                                                          : new Vector3( pos.x - buffer[i], positions[halfCount + posIndex].y );
            positions[halfCount + posIndex + 1] = isAxisX ? new Vector3( positions[halfCount + posIndex + 1].x, pos.y + buffer[i] )
                                                          : new Vector3( pos.x + buffer[i], positions[halfCount + posIndex + 1].y );
        }

        rdr.SetPositions( positions );
    }
}
