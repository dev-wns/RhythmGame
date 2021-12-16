using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class AudioVisualizer : MonoBehaviour
{
    public RectTransform spectrumPrefab;
    public RectTransform centerImage;

    private RectTransform[] visualSpectrums;
    public int numSpectrum = 128;

    private float[][] spectrums;
    private readonly int bassRange = 14;
    public float pumpingPower  = 15f;
    public float spectrumPower = 5f;
    public float specWidth = 2f;

    private float imageSize = 500f;

    private void Start()
    {
        // create spectrum objects
        imageSize = imageSize * Screen.width / 1920;

        int symmetryColorIdx = numSpectrum;
        float angle = 180f / numSpectrum;
        visualSpectrums = new RectTransform[numSpectrum * 2];
        for ( int idx = 0; idx < numSpectrum * 2; ++idx )
        {
            Transform obj = Instantiate( spectrumPrefab, transform );
            visualSpectrums[idx] = obj.transform as RectTransform;
            visualSpectrums[idx].rotation = Quaternion.Euler( new Vector3( 0f, 0f, angle + angle * idx ) );
            visualSpectrums[idx].Translate( transform.up * imageSize * .5f );

            if ( idx < numSpectrum )
                obj.GetComponent<Image>().color = GetGradationColor( idx );
            else
                obj.GetComponent<Image>().color = GetGradationColor( symmetryColorIdx-- );
        }

        // details
        centerImage.localScale = new Vector3( imageSize, imageSize, 1f );
    }

    private void FixedUpdate()
    {
        uint length;
        System.IntPtr data;
        SoundManager.Inst.VisualizerDsp.getParameterData( ( int )FMOD.DSP_FFT.SPECTRUMDATA, out data, out length );
        FMOD.DSP_PARAMETER_FFT fftData = ( FMOD.DSP_PARAMETER_FFT )Marshal.PtrToStructure( data, typeof( FMOD.DSP_PARAMETER_FFT ) );
        spectrums = fftData.spectrum;

        if ( fftData.numchannels > 0 )
        {
            float average = 0f;
            for ( int i = 0; i < numSpectrum; ++i )
            {
                float value = spectrums[0][i] * 1000f * spectrumPower;
                float y = visualSpectrums[i].localScale.y;
                float scale = Mathf.Lerp( y, value, ( value / y ) * .5f ); //Mathf.SmoothStep( y, value, value / y );

                Vector3 newScale = new Vector3( specWidth, scale, 1f );
                visualSpectrums[i].localScale                           = newScale; // left
                visualSpectrums[( numSpectrum * 2 ) - 1 - i].localScale = newScale; // right
             
                if ( i < bassRange )
                    average += spectrums[0][i] * ( 1 + i );
            }


            average = ( average / bassRange ) * 1000f;
            float values     = Mathf.Clamp( average * pumpingPower, imageSize, imageSize * 1.5f );
            float scaleValue = Mathf.Lerp( centerImage.localScale.y, values, .15f );
            centerImage.localScale = new Vector3( scaleValue, scaleValue, 1f );
        }
    }

    private float CurrentVolume()
    {
        float volume = SoundManager.Inst.GetVolume();
        if ( volume >= 1f ) return 1f;
        else                return ( 1f - volume ) * 10f;
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
