using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( LineRenderer ) )]
public class FrequencyBand : AudioVisualizer
{
    private LineRenderer rdr;
    private Transform[] freqObjects;

    private Vector3[] newPosition;
    private float[] freqBands;

    public int numBands = 11; // max 11
    public float defaultPos = 450f;
    public float bandPower;

    public Action<float[]/* freqBands */> OnFrqBandUpdate;

    private void Awake()
    {
        freqBands = new float[11];

        // create object
        freqObjects = new Transform[numBands - 1];
        newPosition = new Vector3[freqObjects.Length];

        float angle = ( 360f / freqObjects.Length );
        for ( int i = 0; i < freqObjects.Length; i++ )
        {
            GameObject obj = new GameObject( i.ToString() );
            obj.transform.SetParent( this.transform );
            freqObjects[i] = obj.transform;
            freqObjects[i].rotation = Quaternion.Euler( new Vector3( 0f, 0f, angle * i ) );
            freqObjects[i].position = freqObjects[i].transform.up * defaultPos;
        }

        // renderer setting 
        rdr = GetComponent<LineRenderer>();
        rdr.positionCount = newPosition.Length;

        if ( numBands % 2 != 0 )
            transform.rotation = Quaternion.Euler( new Vector3( 0, 0, 270 ) );
    }

    protected override void UpdateSpectrums( float[] _datas )
    {
        int count = 0;
        for ( int i = 0; i < 11; i++ )
        {
            float average = 0f;
            int sampleCount = ( int )Mathf.Pow( 2, i ) * 2;
            if ( i == 11 )
                sampleCount += 2;

            for ( int j = 0; j < sampleCount; j++ )
            {
                average += _datas[count] * ( count + 1 );
                count++;
            }
            freqBands[i] = average / count;
        }

        OnFrqBandUpdate?.Invoke( freqBands );

        for ( int i = 1; i < freqObjects.Length; i++ )
        {
            freqObjects[i].position = Vector3.Slerp( freqObjects[i].position,
                                                   ( freqObjects[i].transform.up * defaultPos ) + ( freqObjects[i].transform.up * freqBands[i + 1] * 1000f * bandPower ), .25f );

            newPosition[i] = transform.position + freqObjects[i].position;
        }

        newPosition[0] = transform.position - freqObjects[3].position;

        rdr.SetPositions( newPosition );
    }
}
