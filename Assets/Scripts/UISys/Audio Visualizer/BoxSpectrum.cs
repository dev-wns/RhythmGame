using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( SpriteRenderer ) )]
public class BoxSpectrum : MonoBehaviour
{
    public int index;
    public FrequencyBand freqBand;
    public float power;

    private void Awake()
    {
        freqBand.OnFrqBandUpdate += UpdateSpectrum;
    }

    private void UpdateSpectrum( float[] _values )
    {
        if ( index > _values.Length )
            return;

        float offset = Mathf.Lerp( transform.localScale.x, _values[index] * 100f * power, .25f );
        transform.localScale = new Vector3( offset, offset, 1f );
    }
}