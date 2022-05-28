using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineSpectrum : MonoBehaviour
{
    public AudioVisualizer audioVisualizer;
    public Transform spectrumPrefab;
    private Transform[] spectrums;

    public float specWidth;
    public float specBlank;
    public float specPower;
    public Color color = Color.white;
    private int numSpectrum = 64;
    private List<SpriteRenderer> specRenderer = new List<SpriteRenderer>();


    // buffers
    public float decreasePower = .005f;
    private float[] freqBand64;
    private float[] bandBuffer64;
    private float[] bufferDecrease64;

    private void Awake()
    {
        freqBand64 = new float[64];
        bandBuffer64    = new float[64];
        bufferDecrease64 = new float[64];

        // delegate chain
        audioVisualizer.UpdateSpectrums += UpdateBand64;

        // create spectrum objects
        spectrums = new Transform[numSpectrum];
        float offset   = specWidth + specBlank;
        float startPos = -( offset * numSpectrum * .5f );
        for ( int i = 0; i < numSpectrum; i++ )
        {
            Transform obj = Instantiate( spectrumPrefab, this.transform );
            spectrums[i] = obj.transform;
            //spectrums[i].position = new Vector3( startPos + ( offset * i ), 0f, transform.position.z );
            specRenderer.Add( spectrums[i].GetComponent<SpriteRenderer>() );
            specRenderer[i].sortingOrder = ( int )transform.position.z;

            obj.GetComponent<SpriteRenderer>().color = color;
        }
    }

    private void UpdateBand64( float[] _values, float _offset )
    {
        int count = 0;
        int sampleCount =1;
        int power = 0;

        for ( int i = 0; i < 64; i++ )
        {
            float average = 0f;
            if ( i == 16 || i == 32 || i == 40 || i == 48 || i == 56 )
            {
                power++;
                sampleCount = ( int )Mathf.Pow( 2, power );
                if ( power == 3 )
                     sampleCount -= 2;
            }

            for ( int j = 0; j < sampleCount; j++ )
            {
                average += _values[count] * ( count + 1 );
                count++;
            }

            average /= count;
            freqBand64[i] = average * 80;

            BandBuffer( i );
        }

        UpdateSpectrum();
    }
    
    private void BandBuffer( int _index )
    {
        if ( freqBand64[_index] > bandBuffer64[_index] )
        {
            bandBuffer64[_index] = freqBand64[_index];
            bufferDecrease64[_index] = decreasePower;
        }

        if ( freqBand64[_index] < bandBuffer64[_index] )
        {
            bandBuffer64[_index] -= bufferDecrease64[_index];
            bufferDecrease64[_index] *= 1.2f;
        }
    }

    private void UpdateSpectrum()
    {
        float bufferHighest = 0f;
        for ( int i = 0; i < numSpectrum / 2; i++ )
        {
            if ( bandBuffer64[i] > bufferHighest )
                 bufferHighest = bandBuffer64[i];
        }

        float offset = specWidth + specBlank;
        for ( int i = 0; i < numSpectrum / 2; i++ )
        {
            BandBuffer( i );

            float value = bandBuffer64[i] * specPower;

            float y = spectrums[i].localScale.y;
            float scale = Mathf.Lerp( y, value, .35f );

            Vector3 newScale = new Vector3( specWidth, scale, 1f );
            spectrums[i].localScale                           = newScale; // left
            spectrums[numSpectrum - 1 - i].localScale = newScale; // right

            spectrums[i].position = new Vector3( -offset * i, 0f, transform.position.z );
            spectrums[numSpectrum - 1 - i].position = new Vector3( offset * i, 0f, transform.position.z );

            float normalizeBuffer = bandBuffer64[i] / bufferHighest;
            normalizeBuffer = normalizeBuffer < .25f ? .25f : normalizeBuffer;
            var newColor = new Color( normalizeBuffer, normalizeBuffer, normalizeBuffer, normalizeBuffer );
            specRenderer[i].color                   = newColor; // left
            specRenderer[numSpectrum - 1 - i].color = newColor; // right
        }
    }
}