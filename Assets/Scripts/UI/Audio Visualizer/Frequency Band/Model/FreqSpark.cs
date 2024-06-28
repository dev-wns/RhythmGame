using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( RectTransform ) )]
[RequireComponent( typeof( LineRenderer ) )]
public class FreqSpark : MonoBehaviour
{
    public MultipleFreqBand freqBand;
    private RectTransform rt => transform as RectTransform;
    private Vector2 pos => rt.anchoredPosition;
    private Vector2 scl => rt.sizeDelta;
    public float maxHeight;

    [Header("LineRenderer")]
    private LineRenderer rdr;
    private Vector3[] positions;
    private Vector2   startPos;
    private Vector2   endPos;
    private int maxCount;

    [Range(0f, 100f)] public float posOffset;

    [Header("Frequency")]
    private float[] bandBuffer;
    private int     freqCount;

    public float decreasePower = 1f;
    
    [Header( "Normalize" )]
    public bool isNormalized;
    public int NormalizedRange = 2;

    private void Awake()
    {
        freqBand.OnUpdateBand += UpdateLineRenderer;
        freqCount = freqBand.freqCount;

        bandBuffer = new float[freqCount];
        maxCount   = ( freqCount * 2 );

        rdr = GetComponent<LineRenderer>();
        rdr.positionCount = maxCount;

        positions = new Vector3[rdr.positionCount];
        startPos  = transform.position;
        endPos    = new Vector2( pos.x + ( scl.x * .5f ) - posOffset, pos.y );

        for ( int i = 0; i < freqCount; i++ )
        {
            positions[i]             = new Vector2( ( startPos.x - ( posOffset * .5f ) - ( posOffset * ( freqCount - i - 1 ) ) ), pos.y );
            positions[freqCount + i] = new Vector2( ( startPos.x + ( posOffset * .5f ) + ( posOffset * i ) ), pos.y );
        }
    }

    private void UpdateLineRenderer( float[] _values )
    {
        for ( int i = 0; i < freqCount; i++ )
        {
            float value = Global.Math.Clamp( _values[i] - freqBand.Average, 0f, maxHeight );
            if ( isNormalized )
            {
                float sumValue = 0f;
                int start = Global.Math.Clamp( i - NormalizedRange, 0, freqCount );
                int end   = Global.Math.Clamp( i + NormalizedRange, 0, freqCount );
                for ( int idx = start; idx < end; idx++ )
                      sumValue += Global.Math.Clamp( _values[idx] - freqBand.Average, 0f, maxHeight );

                value = sumValue / ( end - start + 1 );
            }

            bandBuffer[i] = bandBuffer[i] < value ? value 
                                                  : Mathf.Lerp( bandBuffer[i], value, decreasePower * Time.deltaTime );

            positions[freqCount - i] = new Vector3( positions[freqCount - i].x, pos.y - bandBuffer[i] );
            positions[freqCount + i] = new Vector3( positions[freqCount + i].x, pos.y - bandBuffer[i] );
        }

        rdr.SetPositions( positions );
    }
}
