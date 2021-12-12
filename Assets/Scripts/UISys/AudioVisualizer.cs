using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using DG.Tweening;

public class AudioVisualizer : MonoBehaviour
{
    public Transform spectrumPrefab;
    public Transform centerImage;

    private Transform[] visualSpectrums;
    public int numSpectrum = 128;
    private float[] spectrumValues;

    private readonly int bassRange = 14;
    private float bassPower = 800f;
    private float spectrumPower = 600f;
    private float[][] spectrum;

    private readonly float imageSize = 500f;
    private float specWidth;
    public static float bassAmount = 0f;

    private void Start()
    {
        DOTween.Init();

        spectrumValues = new float[numSpectrum];

        // create spectrum objects
        int symmetryColorIdx = numSpectrum;
        float angle = 180f / numSpectrum;
        visualSpectrums = new Transform[numSpectrum * 2];
        for ( int idx = 0; idx < numSpectrum * 2; ++idx )
        {
            Transform obj = Instantiate( spectrumPrefab, transform );
            obj.rotation = Quaternion.Euler( new Vector3( 0f, 0f, angle + angle * idx ) );
            obj.Translate( transform.up * imageSize * .5f );

            if ( idx < numSpectrum )
                obj.GetComponent<SpriteRenderer>().material.color = GetGradationColor( idx );
            else
                obj.GetComponent<SpriteRenderer>().material.color = GetGradationColor( symmetryColorIdx-- );
            visualSpectrums[idx] = obj;
        }

        // details
        specWidth = imageSize * .001f * 2f;
        centerImage.localScale = new Vector3( imageSize, imageSize, 1f );

        StartCoroutine( UpdateValue() );
    }

    private IEnumerator UpdateValue()
    {
        DOTween.SetTweensCapacity( numSpectrum, 0 );
        while( true )
        {
            uint length;
            System.IntPtr data;
            SoundManager.Inst.VisualizerDsp.getParameterData( ( int )FMOD.DSP_FFT.SPECTRUMDATA, out data, out length );
            FMOD.DSP_PARAMETER_FFT fftData = ( FMOD.DSP_PARAMETER_FFT )Marshal.PtrToStructure( data, typeof( FMOD.DSP_PARAMETER_FFT ) );
            spectrum = fftData.spectrum;

            float volume = CurrentVolume();
            for ( int i = 0; i < numSpectrum; ++i )
            {
                DOTween.Kill( visualSpectrums[i] );
                DOTween.Kill( visualSpectrums[( numSpectrum * 2 ) - 1 - i] );

                float value = spectrum[0][0 + i] * spectrumPower * volume;

                Vector3 newScale = new Vector3( specWidth, value, 1f );
                visualSpectrums[i].DOScale( newScale, .125f );
                visualSpectrums[( numSpectrum * 2 ) - 1 - i].DOScale( newScale, .125f );
            }

            yield return YieldCache.WaitForSeconds( .1f );
        }
    }

    //private void FixedUpdate()
    //{
    //    uint length;
    //    System.IntPtr data;
    //    SoundManager.Inst.VisualizerDsp.getParameterData( ( int )FMOD.DSP_FFT.SPECTRUMDATA, out data, out length );
    //    FMOD.DSP_PARAMETER_FFT fftData = ( FMOD.DSP_PARAMETER_FFT )Marshal.PtrToStructure( data, typeof( FMOD.DSP_PARAMETER_FFT ) );
    //    spectrum = fftData.spectrum;

    //    if ( fftData.numchannels > 0 )
    //    {
    //        float volume =  CurrentVolume();
    //        for ( int i = 0; i < numSpectrum; ++i )
    //        {

    //            float y = visualSpectrums[i].localScale.y;
    //            float value = ( spectrum[0][0 + i] / 10f ) * spectrumPower * volume * 10f ;
    //            // float value = spectrum[0][0 + i] * spectrumPower * volume;
    //            float scale = Mathf.SmoothStep( y, value, .3141592f );

    //            visualSpectrums[i].DOScale( new Vector3( specWidth, value, 1f ), .2f );
    //            visualSpectrums[( numSpectrum * 2 ) - 1 - i].DOScale( new Vector3( specWidth, value, 1f ), .2f );

    //            //Vector3 newScale = new Vector3( specWidth, value, 1f );
    //            //visualSpectrums[i].localScale                             = newScale; // left
    //            //visualSpectrums[( spectrumCount * 2 ) - 1 - i].localScale = newScale; // right
    //        }

    //        bassAmount = 0f;
    //        for ( int i = 0; i < bassRange; ++i )
    //        {
    //            bassAmount += spectrum[0][i];
    //        }
            
    //        DOTween.Kill( centerImage );
    //        float values = Mathf.Clamp( bassAmount * bassPower * volume, imageSize, imageSize * 1.5f );
    //        centerImage.DOScale( new Vector3( values, values, 0f ), .15f );
    //    }
    //}

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

    private void OnDestroy()
    {
        DOTween.KillAll();
    }
}
