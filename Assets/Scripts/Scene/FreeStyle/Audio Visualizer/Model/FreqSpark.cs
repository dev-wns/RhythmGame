using UnityEngine;

[RequireComponent( typeof( RectTransform ) )]
public class FreqSpark : MonoBehaviour
{
    [Header( "- Visualizer -" )]
    public AudioVisualizer visualizer;
    public int   startIndex;
    public int   freqCount;
    public float power;

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


    private void Awake()
    {
        visualizer.OnUpdate += UpdateSpark;
        rdr.positionCount    = freqCount * 4;
        buffer               = new float[freqCount];
        positions            = new Vector3[rdr.positionCount];

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

    private void UpdateSpark( float[] _values )
    {
        for ( int i = 0; i < freqCount; i++ )
        {
            float value  = Global.Math.Clamp( ( _values[startIndex + i] - ( visualizer.Average * discardOffset ) ) * power, 0f, maxHeight );
            if ( isNormalized )
            {
                float sumValue = 0f;
                int start = Global.Math.Clamp( i - NormalizedRange, 0, freqCount - 1 );
                int end   = Global.Math.Clamp( i + NormalizedRange, 0, freqCount - 1 );

                for ( int idx = start; idx <= end; idx++ )
                      sumValue += Global.Math.Clamp( ( _values[startIndex + idx] - ( visualizer.Average * discardOffset ) ) * power, 0f, maxHeight );

                value = ( sumValue / ( end - start + 1 ) );
            }

            buffer[i] += buffer[i] < value ? Global.Math.Lerp( 0f, Global.Math.Abs( buffer[i] - value ), riseAmount * Time.fixedDeltaTime ) :
                                            -Global.Math.Lerp( 0f, Global.Math.Abs( buffer[i] - value ), dropAmount * Time.fixedDeltaTime );
            buffer[i] = Mathf.Min( buffer[i] < value ? value : buffer[i], maxHeight );

            // UI °»½Å
            int index  = isReverse ? freqCount - i - 1 : i;
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
