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

    [Header("LineRenderer")]
    private LineRenderer rdr;
    private Vector3[] positions;
    private Vector2   startPos;
    private Vector2   endPos;
    private int maxCount;

    [Range(0f, 100f)] public float posRangeOffset;

    [Header("Frequency")]
    private float[] bandBuffer;
    private int     freqCount;

    public float decreasePower = 1f;

    private void Awake()
    {
        freqBand.OnUpdateBand += UpdateLineRenderer;
        freqCount = freqBand.freqCount;

        bandBuffer = new float[freqCount];
        maxCount   = ( freqCount * 2 ) + 4;

        rdr = GetComponent<LineRenderer>();
        rdr.positionCount = maxCount;

        positions = new Vector3[rdr.positionCount];
        startPos  = new Vector2( pos.x - ( scl.x * .5f ) + posRangeOffset, pos.y );
        endPos    = new Vector2( pos.x + ( scl.x * .5f ) - posRangeOffset, pos.y );

        float posOffset = Global.Math.Abs( endPos.x - startPos.x ) / ( maxCount - 1 );

        positions[0] = startPos;
        positions[1] = new Vector2( startPos.x + posOffset, startPos.y );
        for ( int i = 0; i < freqCount * 2; i++ )
        {
            positions[i + 2] = new Vector2( ( startPos.x + posOffset ) + ( posOffset * ( i + 1 ) ), pos.y );
        }
        positions[maxCount - 2] = new Vector2( endPos.x - posOffset, endPos.y );
        positions[maxCount - 1] = endPos;
    }

    private void UpdateLineRenderer( float[] _values )
    {
        float offset = freqBand.Average / Mathf.PI;
        for ( int i = 0; i < freqCount; i++ )
        {
            float value = Global.Math.Clamp( _values[i] - offset, 0f, 1000f );

            bandBuffer[i] = bandBuffer[i] < value ?
                            value : Mathf.Lerp( bandBuffer[i], value, decreasePower * Time.deltaTime );

            int posIndex = ( i * 2 ) + 2;
            positions[posIndex]     = new Vector3( positions[posIndex].x,     pos.y + bandBuffer[i] );
            positions[posIndex + 1] = new Vector3( positions[posIndex + 1].x, pos.y - bandBuffer[i] );
        }

        rdr.SetPositions( positions );
    }
}
