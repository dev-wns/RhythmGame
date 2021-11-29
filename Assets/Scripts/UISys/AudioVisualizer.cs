using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using DG.Tweening;

public class AudioVisualizer : MonoBehaviour
{
    public Transform spectrumPrefab;
    public Transform centerImage;

    private FMOD.DSP dsp;
    private Transform[] visualSpectrums;
    public int spectrumCount = 128;

    private readonly int bassRange = 14;
    private float bassPower = 900f;
    private float spectrumPower = 750f;
    private float[][] spectrum;

    private readonly float imageSize = 500f;
    private float specWidth;

    private void Start()
    {
        DOTween.Init();
        specWidth = imageSize * .001f * 2f;

        // DSP setting
        FMODUnity.RuntimeManager.CoreSystem.createDSPByType( FMOD.DSP_TYPE.FFT, out dsp );
        dsp.setParameterInt( ( int )FMOD.DSP_FFT.WINDOWSIZE, 4096 );
        dsp.setParameterInt( ( int )FMOD.DSP_FFT.WINDOWTYPE, ( int )FMOD.DSP_FFT_WINDOW.BLACKMANHARRIS );
        SoundManager.Inst.group.addDSP( FMOD.CHANNELCONTROL_DSP_INDEX.HEAD, dsp );
        SoundManager.Inst.Volume = 0.1f;

        // create spectrum objects
        int symmetryColorIdx = spectrumCount;
        float angle = 180f / spectrumCount;
        visualSpectrums = new Transform[spectrumCount * 2];
        for ( int idx = 0; idx < spectrumCount * 2; ++idx )
        {
            Transform obj = Instantiate( spectrumPrefab, transform );
            obj.rotation = Quaternion.Euler( new Vector3( 0f, 0f, angle + angle * idx ) );
            obj.Translate( transform.up * imageSize * .5f );

            if ( idx < spectrumCount )
                obj.GetComponent<SpriteRenderer>().material.color = GetGradationColor( idx );
            else
                obj.GetComponent<SpriteRenderer>().material.color = GetGradationColor( symmetryColorIdx-- );
            visualSpectrums[idx] = obj;
        }

        // details
        centerImage.localScale = new Vector3( imageSize, imageSize, 1f );
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
            float volume = CurrentVolume();
            for ( int i = 0; i < spectrumCount; ++i )
            {
                float y = visualSpectrums[i].localScale.y;
                float value = spectrum[0][0 + i] * spectrumPower * volume;
                float scale = Mathf.SmoothStep( y, value, .25f );

                Vector3 newScale = new Vector3( specWidth, scale, 1f );
                visualSpectrums[i].localScale                             = newScale; // left
                visualSpectrums[( spectrumCount * 2 ) - 1 - i].localScale = newScale; // right
            }

            float bassAmount = 0f;
            for ( int i = 0; i < bassRange; ++i )
            {
                bassAmount += spectrum[0][i];
            }

            DOTween.Kill( centerImage );
            float values = Mathf.Clamp( bassAmount * bassPower * volume, imageSize, imageSize * 1.5f );
            centerImage.DOScale( new Vector3( values, values, 0f ), .15f );
        }
    }

    private float CurrentVolume()
    {
        float volume = SoundManager.Inst.Volume;
        if ( volume >= 1f ) return 1f;
        else                return ( 1f - volume ) * 10f;
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

    private void OnDestroy()
    {
        DOTween.KillAll();
    }
}
