using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using DG.Tweening;

public class Spectrum : MonoBehaviour
{
    public Transform prefab;
    private Transform centerImage;

    private FMOD.DSP dsp;
    private Transform[] visualSpectrums;
    private readonly short spectrumCount = 128;

    private float bassPower = 3f;
    private float spectrumPower = 150f;
    private float[][] spectrum;

    private void Start()
    {
        // DSP setting
        FMODUnity.RuntimeManager.CoreSystem.createDSPByType( FMOD.DSP_TYPE.FFT, out dsp );
        dsp.setParameterInt( ( int )FMOD.DSP_FFT.WINDOWSIZE, 4096 );
        dsp.setParameterInt( ( int )FMOD.DSP_FFT.WINDOWTYPE, ( int )FMOD.DSP_FFT_WINDOW.BLACKMANHARRIS );
        SoundManager.Inst.group.addDSP( FMOD.CHANNELCONTROL_DSP_INDEX.HEAD, dsp );
        SoundManager.Inst.Volume = 0.1f;

        // create spectrum objects
        int symmetryColorIdx = spectrumCount;
        float angle = 180f / spectrumCount ;
        visualSpectrums = new Transform[ spectrumCount * 2 ];
        for ( int idx = 0; idx < spectrumCount * 2; ++idx )
        {
            Transform obj = Instantiate( prefab, transform );
            obj.rotation = Quaternion.Euler( new Vector3( 0f, 0f, angle + angle * idx ) );
            obj.Translate( transform.up );

            if ( idx < spectrumCount )
                obj.GetComponent<SpriteRenderer>().material.color = GetGradationColor( idx );
            else
                obj.GetComponent<SpriteRenderer>().material.color = GetGradationColor( symmetryColorIdx-- );
            visualSpectrums[ idx ] = obj;
        }

        // detail setting
        centerImage = transform.parent;
        centerImage.localScale = new Vector3( 2f, 2f, 2f );
    }

    private void FixedUpdate()
    {
        uint length;
        System.IntPtr data;
        dsp.getParameterData( ( int )FMOD.DSP_FFT.SPECTRUMDATA, out data, out length );
        FMOD.DSP_PARAMETER_FFT fftData = ( FMOD.DSP_PARAMETER_FFT )Marshal.PtrToStructure( data, typeof( FMOD.DSP_PARAMETER_FFT ) );

        spectrum = fftData.spectrum;
        if ( fftData.numchannels > 0 )
        {
            for ( int idx = 0; idx < spectrumCount; ++idx )
            {
                float value = spectrum[ 0 ][ 10 + idx ] * spectrumPower * CurrentVolume();
                float y = visualSpectrums[idx].localScale.y;
                Vector3 scale = Vector3.Lerp( new Vector3(.1f, y, .1f ), new Vector3(.1f, value, .1f ), .225f );
                visualSpectrums[idx].localScale = scale;
                visualSpectrums[( spectrumCount * 2 ) - 1 - idx].localScale = scale;
            }

            float bassAmount = 0f;
            for ( int idx = 0; idx < 32; ++idx )
            {
                bassAmount += spectrum[ 0 ][ idx ];
            }

            DOTween.Kill( centerImage );
            float values = Mathf.Clamp( bassAmount * bassPower * CurrentVolume(), 2f, 2.5f );
            centerImage.DOScale( new Vector3( values, values, 0f ), .15f );
        }
    }

    private float CurrentVolume()
    {
        float volume = SoundManager.Inst.Volume;
        if ( volume >= 1 ) return 1f;
        else               return ( 1f - volume ) * 10f;
    }
    private Color GetGradationColor( int _index )
    {
        int r = 0, g = 0, b = 0;
        float a = ( 1.0f - ( ( 1.0f / spectrumCount * ( spectrumCount - _index ) ) ) ) / 0.25f;
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
