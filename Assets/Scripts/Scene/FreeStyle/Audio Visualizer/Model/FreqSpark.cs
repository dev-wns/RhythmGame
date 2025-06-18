using UnityEngine;

[RequireComponent( typeof( RectTransform ) )]
[RequireComponent( typeof( LineRenderer ) )]
public class FreqSpark : MonoBehaviour
{
    public MultipleFreqBand freqBand;
    public float maxHeight;

    [Header("LineRenderer")]
    private LineRenderer rdr;
    private Vector3[] positions;
    private Vector2   startPos;
    private Vector2   pos;
    private int maxCount;

    public float posOffset;

    [Header("Frequency")]
    public float sclOffset;
    public float riseAmount;
    public float dropAmount;
    private float[] buffer;
    private int     freqCount;

    [Header( "Normalize" )]
    public bool isNormalized;
    public int NormalizedRange;

    public bool isReverse;
    public bool isAxisX;

    private void Awake()
    {

        freqBand.OnUpdateBand += UpdateLineRenderer;
        freqCount = freqBand.freqCount;

        buffer = new float[freqCount];
        maxCount = freqCount * 4;

        rdr = GetComponent<LineRenderer>();
        rdr.positionCount = maxCount;

        positions = new Vector3[rdr.positionCount];
        startPos = transform.position;
        pos = ( transform as RectTransform ).anchoredPosition;

        for ( int i = 0; i < freqCount * 2; i++ )
        {
            positions[i] = isAxisX ? new Vector2( ( startPos.x - ( posOffset * .5f ) - ( posOffset * ( ( freqCount * 2 ) - i - 1 ) ) ), pos.y )
                                                       : new Vector2( pos.x, ( startPos.y - ( posOffset * .5f ) - ( posOffset * ( ( freqCount * 2 ) - i - 1 ) ) ) );
            positions[( freqCount * 2 ) + i] = isAxisX ? new Vector2( ( startPos.x + ( posOffset * .5f ) + ( posOffset * i ) ), pos.y )
                                                       : new Vector2( pos.x, ( startPos.y + ( posOffset * .5f ) + ( posOffset * i ) ) );
        }
    }

    private void UpdateLineRenderer( float[] _values )
    {
        for ( int i = 0; i < freqCount; i++ )
        {
            int index = isReverse ? freqCount - i - 1 : i;
            float value = Global.Math.Clamp( _values[i] - Global.Math.Lerp( 0f, freqBand.Average, sclOffset ), 0f, maxHeight );

            if ( isNormalized )
            {
                float sumValue = 0f;
                int start = Global.Math.Clamp( i - NormalizedRange, 0, freqCount );
                int end   = Global.Math.Clamp( i + NormalizedRange, 0, freqCount );
                for ( int idx = start; idx < end; idx++ )
                {
                    sumValue += Global.Math.Clamp( _values[idx] - Global.Math.Lerp( 0f, freqBand.Average, sclOffset ), 0f, maxHeight );
                }

                value = Global.Math.Clamp( sumValue / ( end - start + 1 ), 0f, maxHeight );
            }

            buffer[i] += buffer[i] < value ? Global.Math.Lerp( 0f, Global.Math.Abs( buffer[i] - value ), riseAmount * Time.deltaTime ) :
                                            -Global.Math.Lerp( 0f, Global.Math.Abs( buffer[i] - value ), dropAmount * Time.deltaTime );

            buffer[i] = Mathf.Min( buffer[i] < value ? value : buffer[i], maxHeight );

            // UI °»½Å
            int number = ( index * 2 );
            positions[( freqCount * 2 ) - number]     = isAxisX ? new Vector3( positions[( freqCount * 2 ) - number].x, pos.y - buffer[i] )
                                                                : new Vector3( pos.x - buffer[i], positions[( freqCount * 2 ) - number].y );
            positions[( freqCount * 2 ) - number - 1] = isAxisX ? new Vector3( positions[( freqCount * 2 ) - number - 1].x, pos.y + buffer[i] )
                                                                : new Vector3( pos.x + buffer[i], positions[( freqCount * 2 ) - number - 1].y );

            positions[( freqCount * 2 ) + number]     = isAxisX ? new Vector3( positions[( freqCount * 2 ) + number].x, pos.y - buffer[i] )
                                                                : new Vector3( pos.x - buffer[i], positions[( freqCount * 2 ) + number].y );
            positions[( freqCount * 2 ) + number + 1] = isAxisX ? new Vector3( positions[( freqCount * 2 ) + number + 1].x, pos.y + buffer[i] )
                                                                : new Vector3( pos.x + buffer[i], positions[( freqCount * 2 ) + number + 1].y );
        }

        rdr.SetPositions( positions );
    }
}
