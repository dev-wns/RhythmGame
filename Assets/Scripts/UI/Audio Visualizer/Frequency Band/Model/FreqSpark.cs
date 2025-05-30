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
    private Gradient gradient;
    private GradientAlphaKey[] gradientAlphaKeys;
    private Vector3[] positions;
    private Vector2   startPos;
    private Vector2   endPos;
    private int maxCount;

    [Range(0f, 100f)] public float posOffset;

    [Header("Frequency")]
    private float[] buffer;
    private int     freqCount;
    [Range(1f, 2f)]   public float sclOffset;
    [Range(1f, 100f)] public float dropAmount = 1f;

    [Header( "Normalize" )]
    public bool isNormalized;
    public int NormalizedRange = 2;

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
        endPos = isAxisX ? new Vector2( pos.x + ( scl.x * .5f ) - posOffset, pos.y )
                            : new Vector2( pos.x, pos.y + ( scl.y * .5f ) - posOffset );

        for ( int i = 0; i < freqCount * 2; i++ )
        {
            positions[i] = isAxisX ? new Vector2( ( startPos.x - ( posOffset * .5f ) - ( posOffset * ( ( freqCount * 2 ) - i - 1 ) ) ), pos.y )
                                                       : new Vector2( pos.x, ( startPos.y - ( posOffset * .5f ) - ( posOffset * ( ( freqCount * 2 ) - i - 1 ) ) ) );
            positions[( freqCount * 2 ) + i] = isAxisX ? new Vector2( ( startPos.x + ( posOffset * .5f ) + ( posOffset * i ) ), pos.y )
                                                       : new Vector2( pos.x, ( startPos.y + ( posOffset * .5f ) + ( posOffset * i ) ) );
        }

        gradient = rdr.colorGradient;
        gradientAlphaKeys = rdr.colorGradient.alphaKeys;
    }

    //private void OnEnable()
    //{
    //    DOTween.To( () => 0f, ( float _alpha ) =>
    //        {
    //            var keys = gradient.alphaKeys;
    //            for ( int i = 0; i < keys.Length; i++ )
    //            {
    //                keys[i].alpha = gradientAlphaKeys[i].alpha * _alpha;
    //            }

    //            gradient.alphaKeys = keys;
    //            rdr.colorGradient = gradient;

    //        }, 1f, Global.Const.OptionFadeDuration );
    //}

    //private void OnDisable()
    //{
    //    DOTween.To( () => 1f, ( float _alpha ) =>
    //    {
    //        var keys = gradient.alphaKeys;
    //        for ( int i = 0; i < keys.Length; i++ )
    //        {
    //            keys[i].alpha = gradientAlphaKeys[i].alpha * _alpha;
    //        }

    //        gradient.alphaKeys = keys;
    //        rdr.colorGradient = gradient;

    //    }, 0f, Global.Const.OptionFadeDuration );
    //}

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

            // 최소 감소량 + 데이터의 크기 차이에 따른 감소량
            buffer[i] -= ( 30f * Time.deltaTime ) + ( Global.Math.Abs( buffer[i] - value ) * dropAmount * Time.deltaTime );
            buffer[i] = buffer[i] < value ? value : buffer[i];

            // UI 갱신
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
