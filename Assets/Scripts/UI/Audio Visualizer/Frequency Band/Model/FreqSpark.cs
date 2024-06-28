using Coffee.UIEffects;
using DG.Tweening;
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
    private Gradient gradient;
    private GradientAlphaKey[] gradientAlphaKeys;
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
        maxCount   = freqCount * 4;

        rdr = GetComponent<LineRenderer>();
        rdr.positionCount = maxCount;

        positions = new Vector3[rdr.positionCount];
        startPos  = transform.position;
        endPos    = new Vector2( pos.x, pos.y + ( scl.y * .5f ) - posOffset );

        for ( int i = 0; i < freqCount * 2; i++ )
        {
            positions[i]                     = new Vector2( pos.x, ( startPos.y - ( posOffset * .5f ) - ( posOffset * ( ( freqCount * 2 ) - i - 1 ) ) ) );
            positions[( freqCount * 2 ) + i] = new Vector2( pos.x, ( startPos.y + ( posOffset * .5f ) + ( posOffset * i ) ) );
        }

        gradient       = rdr.colorGradient;
        gradientAlphaKeys = rdr.colorGradient.alphaKeys;
    }

    private void OnEnable()
    {
            DOTween.To( () => 0f, ( float _alpha ) =>
                {
                    var keys = gradient.alphaKeys;
                    for ( int i = 0; i < keys.Length; i++ )
                    {
                        keys[i].alpha = gradientAlphaKeys[i].alpha * _alpha;
                    }

                    gradient.alphaKeys = keys;
                    rdr.colorGradient  = gradient;

                }, 1f, Global.Const.OptionFadeDuration );
    }

    private void OnDisable()
    {
        DOTween.To( () => 1f, ( float _alpha ) =>
        {
            var keys = gradient.alphaKeys;
            for ( int i = 0; i < keys.Length; i++ )
            {
                keys[i].alpha = gradientAlphaKeys[i].alpha * _alpha;
            }

            gradient.alphaKeys = keys;
            rdr.colorGradient  = gradient;

        }, 0f, Global.Const.OptionFadeDuration );
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


            int index = ( i * 2 );
            positions[( freqCount * 2 ) - index]     = new Vector3( pos.x - bandBuffer[i], positions[( freqCount * 2 ) - index].y );
            positions[( freqCount * 2 ) - index - 1] = new Vector3( pos.x + bandBuffer[i], positions[( freqCount * 2 ) - index - 1].y );
                                                                                                 
            positions[( freqCount * 2 ) + index]     = new Vector3( pos.x - bandBuffer[i], positions[( freqCount * 2 ) + index].y );
            positions[( freqCount * 2 ) + index + 1] = new Vector3( pos.x + bandBuffer[i], positions[( freqCount * 2 ) + index + 1].y );
        }

        rdr.SetPositions( positions );
    }
}
