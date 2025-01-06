using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class AudioVisualizer : MonoBehaviour
{
    private float[][] spectrums;
    public Action<float[][]> OnUpdateSpectrums;

    private void LateUpdate()
    {
        if ( AudioManager.Inst.IsLoad )
             return;

        AudioManager.Inst.GetDSP( FMOD.DSP_TYPE.FFT, out FMOD.DSP fftWindowDSP );
        fftWindowDSP.getParameterData( ( int )FMOD.DSP_FFT.SPECTRUMDATA, out IntPtr data, out uint length );
        FMOD.DSP_PARAMETER_FFT fftData = ( FMOD.DSP_PARAMETER_FFT )Marshal.PtrToStructure( data, typeof( FMOD.DSP_PARAMETER_FFT ) );
        spectrums = fftData.spectrum;
        if ( fftData.spectrum.Length > 0 )
             OnUpdateSpectrums?.Invoke( spectrums );
    }
}