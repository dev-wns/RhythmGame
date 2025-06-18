using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class AudioVisualizer : MonoBehaviour
{
    private float[][] spectrums;
    public Action<float[][]> OnUpdateSpectrums;

    private static readonly int FrameLimit = 144;
    private static readonly float SPF = 1f / FrameLimit;
    private float time;

    private FMOD.DSP fft;

    private void Start()
    {
        if ( !AudioManager.Inst.GetDSP( FMOD.DSP_TYPE.FFT, out fft ) )
             Debug.Log( "Unable to get FFT DSP" );
    }

    private void FixedUpdate()
    {
        if ( AudioManager.Inst.IsStop )
             return;

        fft.getParameterData( ( int )FMOD.DSP_FFT.SPECTRUMDATA, out IntPtr data, out uint length );
        FMOD.DSP_PARAMETER_FFT fftData = ( FMOD.DSP_PARAMETER_FFT )Marshal.PtrToStructure( data, typeof( FMOD.DSP_PARAMETER_FFT ) );
        //fftData.getSpectrum( 0, ref float[] spectrum );
        spectrums = fftData.spectrum;
        if ( fftData.spectrum.Length > 0 )
            OnUpdateSpectrums?.Invoke( spectrums );
    }
}