using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class FrequencyBandImage : MonoBehaviour
{
    private Image image;
    private float cached;
    private RectTransform rt => transform as RectTransform;

    [Header( "Bands" )]
    public FrequencyBand freqBand;
    [Range(0, 9)]
    public int bandIndex;
    [Range(1f, 100f)]
    public float power;

    [Range(0f, 50f)]
    public float decrease;
    public float increase;

    [Header( "Offset" )]
    [Range(0f, 1f)]
    public float colorOffset;
    [Range(0f, 1f)]
    public float scaleOffset;

    private Vector2 sizeCache;

    private void Awake()
    {
        image = GetComponent<Image>();
        freqBand.OnUpdateBand += UpdateImage;

        sizeCache = rt.sizeDelta;
    }

    private void UpdateImage( float[] _values )
    {
        float amount  = _values[bandIndex] * power;
        float diffAbs = Global.Math.Abs( cached - amount );
        float value   = ( diffAbs * decrease ) * Time.deltaTime;
        cached += cached < amount ? value * increase : -value;

        float final = Global.Math.Clamp( cached - .25f, 0f, 1f );
        image.color = new Color( 1, 1, 1, .5f + ( final * colorOffset ) );
        rt.sizeDelta = sizeCache * ( 1f + ( final * scaleOffset ) );
    }
}
