using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class AudioVisualizer : MonoBehaviour
{
    private float[][] spectrums;
    public Action<float[][]> OnUpdateSpectrums;

    private void LateUpdate()
    {
        if ( SoundManager.Inst.IsLoad )
             return;

        uint length;
        IntPtr data;
        FMOD.DSP fftWindowDSP;
        SoundManager.Inst.GetDSP( FMOD.DSP_TYPE.FFT, out fftWindowDSP );
        fftWindowDSP.getParameterData( ( int )FMOD.DSP_FFT.SPECTRUMDATA, out data, out length );
        FMOD.DSP_PARAMETER_FFT fftData = ( FMOD.DSP_PARAMETER_FFT )Marshal.PtrToStructure( data, typeof( FMOD.DSP_PARAMETER_FFT ) );
        spectrums = fftData.spectrum;
        if ( fftData.spectrum.Length > 0 )
             OnUpdateSpectrums?.Invoke( spectrums );
    }
}