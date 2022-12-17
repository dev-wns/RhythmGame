using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent( typeof( Image ) )]
public class FreqHeart : MonoBehaviour
{
    private Image image;
    private Vector2 sizeCache;
    private RectTransform rt => transform as RectTransform;

    [Header( "Bands" )]
    public FrequencyBand freqBand;
    [Range(0, 9)]    public int   bandIndex;
    [Range(0f, 50f)] public float decreasePower;
    private float buffer;
    private Color color;

    private void Awake()
    {
        image = GetComponent<Image>();
        freqBand.OnUpdateBand += UpdateImage;

        color = image.color;
        sizeCache = rt.sizeDelta;
    }

    private void UpdateImage( float[] _values )
    {
        buffer = buffer < _values[bandIndex] ? _values[bandIndex] :
                                               Mathf.Lerp( buffer, _values[bandIndex], decreasePower * Time.deltaTime );

        float final = Global.Math.Clamp( buffer - .3141592f, 0f, 1f );
        color.a = .5f + final;
        image.color  = color;
        rt.sizeDelta = sizeCache * ( 1f + final );
    }
}
