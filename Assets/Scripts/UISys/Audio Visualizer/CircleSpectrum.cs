using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleSpectrum : AudioVisualizer
{
    public Transform spectrumPrefab;
    private Transform[] visualSpectrums;

    public ParticleSystem particleSys;
    public float particleSpeed;
    private ParticleSystem.MainModule particleMain;
    private bool isParticleReady = false;

    public Transform centerImage;
    public float imageSize;

    public int numSpectrum;
    public float spectrumPower;
    public float specWidth;
    public float pumpingPower;

    private readonly int bassRange = 14;

    private void Awake()
    {
        // particle
        if ( particleSys )
             particleMain = particleSys.main;

        // Center Image
        imageSize = imageSize * Screen.width / 1920;
        centerImage.localScale = new Vector3( imageSize, imageSize, 1f );

        // spectrum scale
        float scale = 1 / imageSize;
        transform.localScale = new Vector3( scale, scale, scale );

        // create spectrum objects
        int symmetryColorIdx = numSpectrum;
        float angle = 180f / numSpectrum;
        visualSpectrums = new Transform[numSpectrum * 2];
        for ( int i = 0; i < numSpectrum * 2; ++i )
        {
            Transform obj = Instantiate( spectrumPrefab, this.transform );
            visualSpectrums[i] = obj.transform;
            visualSpectrums[i].rotation = Quaternion.Euler( new Vector3( 0f, 0f, angle + angle * i ) );
            visualSpectrums[i].Translate( transform.up * imageSize * .5f );

            if ( i < numSpectrum ) obj.GetComponent<SpriteRenderer>().color = GetGradationColor( i );
            else                   obj.GetComponent<SpriteRenderer>().color = GetGradationColor( symmetryColorIdx-- );
        }
    }

    private void Start()
    {
        StartCoroutine( ParticleInit() );
    }

    private IEnumerator ParticleInit()
    {
        particleMain.simulationSpeed = 1000f;

        yield return YieldCache.WaitForSeconds( .1f );

        particleMain.simulationSpeed = 1f;
        isParticleReady = true;
    }

    protected override void UpdateSpectrums( float[] _datas )
    {
        float highValue = 1f;
        for ( int i = 0; i < numSpectrum; i++ )
        {
            if ( highValue < _datas[i] ) highValue = _datas[i];
        }

        float average = 0f;
        for ( int i = 0; i < numSpectrum; i++ )
        {
            float value = ( ( _datas[i] / highValue ) * 1000f * spectrumPower );

            //float value = _values[i] * 1000f * spectrumPower;
            float y = visualSpectrums[i].localScale.y;
            float scale = Mathf.Lerp( y, value, .275f ); //Mathf.SmoothStep( y, value, value / y );

            Vector3 newScale = new Vector3( specWidth, scale, 1f );
            visualSpectrums[i].localScale                           = newScale; // left
            visualSpectrums[( numSpectrum * 2 ) - 1 - i].localScale = newScale; // right

            if ( i < bassRange ) average += _datas[i] * ( 1 + i );
        }

        average /= bassRange;

        if ( isParticleReady )
             particleMain.simulationSpeed = average * particleSpeed;


        float clampValue = Mathf.Clamp( average * pumpingPower * 1000f, imageSize, imageSize * 1.5f );
        float scaleValue = Mathf.Lerp( centerImage.localScale.y, clampValue, .15f );
        centerImage.localScale = new Vector3( scaleValue, scaleValue, scaleValue );
    }

    private Color GetGradationColor( int _index )
    {
        int r = 0, g = 0, b = 0;
        float a = ( 1.0f - ( ( 1.0f / numSpectrum * ( numSpectrum - _index ) ) ) ) / 0.25f;
        int X = ( int )Mathf.Floor( a );
        int Y = ( int )Mathf.Floor( 255 * ( a - X ) );
        switch ( X )
        {
            case 0:
                r = 255;
                g = Y;
                b = 0;
                break;
            case 1:
                r = 255 - Y;
                g = 255;
                b = 0;
                break;
            case 2:
                r = 0;
                g = 255;
                b = Y;
                break;
            case 3:
                r = 0;
                g = 255 - Y;
                b = 255;
                break;
            case 4:
                r = Y;
                g = 0;
                b = 255;
                break;
            case 5:
                r = 255;
                g = 0;
                b = 255;
                break;
        }

        return new Color( r / 255.0f, g / 255.0f, b / 255.0f, 1.0f );
    }
}
