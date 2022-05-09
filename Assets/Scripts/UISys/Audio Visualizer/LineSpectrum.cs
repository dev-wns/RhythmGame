using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineSpectrum : MonoBehaviour
{
    private AudioVisualizer audioVisualizer;
    private Transform[] visualSpectrums;
    public Transform spectrumPrefab;

    public int numSpectrum;
    public float specWidth;
    public float specBlank;
    public float specPower;

    public Color color = Color.white;

    private void Awake()
    {
        // delegate chain
        audioVisualizer = GameObject.FindGameObjectWithTag( "Visualizer" ).GetComponent<AudioVisualizer>();
        if ( audioVisualizer ) 
             audioVisualizer.UpdateSpectrums += UpdateSpectrum;

        // create spectrum objects
        visualSpectrums = new Transform[numSpectrum];
        float offset   = specWidth + specBlank;
        float startPos = -( offset * numSpectrum * .5f );
        for ( int i = 0; i < numSpectrum; ++i )
        {
            Transform obj = Instantiate( spectrumPrefab, this.transform );
            visualSpectrums[i] = obj.transform;
            visualSpectrums[i].position = new Vector3( startPos + ( offset * i ), 0f, transform.position.z );

            obj.GetComponent<SpriteRenderer>().color = color;
        }
    }

    private void UpdateSpectrum( float[] _values, float _offset )
    {
        float highValue = 1f;
        for ( int i = 0; i < numSpectrum; i++ )
        {
            if ( highValue < _values[i] ) highValue = _values[i];
        }

        for ( int i = 0; i < numSpectrum; i++ )
        {
            float value = _values[i] * 1000f * specPower * _offset;

            float y = visualSpectrums[i].localScale.y;
            float scale = Mathf.Lerp( y, value, .35f );

            Vector3 newScale = new Vector3( specWidth, scale, 1f );
            visualSpectrums[i].localScale = newScale;
        }
    }
}