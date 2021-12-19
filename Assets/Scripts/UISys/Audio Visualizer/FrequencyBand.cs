using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( LineRenderer ) )]
public class FrequencyBand : MonoBehaviour
{
    private AudioVisualizer audioVisualizer;
    private LineRenderer rdr;
    private Transform[] freqObjects;

    private Vector3[] newPos;
    private float[] freqBands;

    public Sprite sprite;
    public int numBands = 11; // max 11
    public float defaultPos = 450f;
    public float bandPower;

    private void Awake()
    {
        freqBands = new float[numBands];

        // create object
        freqObjects = new Transform[numBands - 1];
        newPos      = new Vector3[freqObjects.Length];

        float angle = angle = ( 360f / freqObjects.Length );
        for ( int i = 0; i < freqObjects.Length; i++ )
        {
            GameObject obj = new GameObject( i.ToString() );
            obj.transform.SetParent( this.transform );
            obj.AddComponent<SpriteRenderer>().sprite = sprite;
            freqObjects[i] = obj.transform;
            freqObjects[i].rotation = Quaternion.Euler( new Vector3( 0f, 0f, angle * i ) );
            freqObjects[i].position = freqObjects[i].transform.up * defaultPos;
        }

        // delegate chain
        audioVisualizer = GameObject.FindGameObjectWithTag( "Visualizer" ).GetComponent<AudioVisualizer>();
        if ( audioVisualizer ) audioVisualizer.UpdateSpectrums += UpdateBand;

        // renderer setting 
        rdr = GetComponent<LineRenderer>();
        rdr.positionCount = newPos.Length;
        rdr.loop = true;

        if ( numBands % 2 != 0 )
            transform.rotation = Quaternion.Euler( new Vector3( 0, 0, 90 ) );
    }

    private void UpdateBand( float[] _values )
    {
        int count = 0;
        for ( int i = 0; i < numBands; i++ )
        {
            float average = 0f;
            int sampleCount = ( int )Mathf.Pow( 2, i ) * 2;
            if ( i == numBands ) sampleCount += 2;

            for ( int j = 0; j < sampleCount; j++ )
            {
                average += _values[count] * ( count + 1 );
                count++;
            }
            freqBands[i] = average / count;
        }
    }

    private void FixedUpdate()
    {
        for ( int i = 0; i < freqObjects.Length; i++ )
        {
            freqObjects[i].position = Vector3.Slerp( freqObjects[i].position,
                                                   ( freqObjects[i].transform.up * defaultPos ) + ( freqObjects[i].transform.up * freqBands[i + 1] * 1000f * bandPower ), .25f );
            newPos[i]        = freqObjects[i].position;

            rdr.SetPositions( newPos );
        }
    }
}
