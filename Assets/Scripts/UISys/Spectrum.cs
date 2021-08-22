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

    private float bassPower = 2.5f;
    private float spectrumPower = 100f;

    void Start()
    {
        DOTween.SetTweensCapacity( 500, 50 );

        FMODUnity.RuntimeManager.CoreSystem.createDSPByType( FMOD.DSP_TYPE.FFT, out dsp );
        dsp.setParameterInt( ( int )FMOD.DSP_FFT.WINDOWSIZE, 4096 );
        dsp.setParameterInt( ( int )FMOD.DSP_FFT.WINDOWTYPE, ( int )FMOD.DSP_FFT_WINDOW.BLACKMAN );
        SoundManager.Inst.group.addDSP( FMOD.CHANNELCONTROL_DSP_INDEX.HEAD, dsp );
        SoundManager.Inst.Volume = 0.1f;

        float angle = 180f / spectrumCount ;
        visualSpectrums = new Transform[ spectrumCount * 2 ];
        for ( int idx = 0; idx < spectrumCount * 2; ++idx )
        {
            Transform obj = Instantiate( prefab, transform );
            obj.rotation = Quaternion.Euler( new Vector3( 0f, 0f, angle + angle * idx ) );
            obj.Translate( transform.up );
            visualSpectrums[ idx ] = obj;
        }

        centerImage = transform.parent;
        centerImage.localScale = new Vector3( 2f, 2f, 2f );
    }

    private void FixedUpdate()
    {
        uint length;
        System.IntPtr data;
        dsp.getParameterData( ( int )FMOD.DSP_FFT.SPECTRUMDATA, out data, out length );
        FMOD.DSP_PARAMETER_FFT fftData = ( FMOD.DSP_PARAMETER_FFT )Marshal.PtrToStructure( data, typeof( FMOD.DSP_PARAMETER_FFT ) );

        float[][] spectrum = fftData.spectrum;
        if ( fftData.numchannels > 0 )
        {
            DOTween.KillAll();
            for ( int idx = 0; idx < spectrumCount; ++idx )
            {
                float value = spectrum[ 0 ][ idx ] * spectrumPower * CurrentVolume();

                visualSpectrums[ idx ].DOScaleY( value, .2f );
                visualSpectrums[ ( spectrumCount * 2 ) - 1 - idx ].DOScaleY( value, .2f );
            }

            float bassAmount = 0f;
            for ( int idx = 0; idx < 32; ++idx )
            {
                bassAmount += spectrum[ 0 ][ idx ];
            }

            float values = Mathf.Clamp( bassAmount * bassPower * CurrentVolume(), 2f, 2.75f );
            centerImage.DOScale( new Vector3( values, values, 0f ), .2f );
        }
    }

    private float CurrentVolume()
    {
        float volume = SoundManager.Inst.Volume;
        if ( volume >= 1 ) return 1f;
        else               return ( 1f - volume ) * 10f;
    }
}
