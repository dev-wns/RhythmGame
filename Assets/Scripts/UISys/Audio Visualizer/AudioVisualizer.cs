using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public abstract class AudioVisualizer : MonoBehaviour
{
    protected abstract void UpdateSpectrums( float[] _datas );

    private void FixedUpdate()
    {
        if ( SoundManager.Inst.IsLoad ) return;

        uint length;
        IntPtr data;
        FMOD.DSP fftWindowDSP;
        if ( !SoundManager.Inst.GetDSP( FMOD.DSP_TYPE.FFT, out fftWindowDSP ) )
             Debug.LogWarning( "FFTWindowData is not Load" );
        fftWindowDSP.getParameterData( ( int )FMOD.DSP_FFT.SPECTRUMDATA, out data, out length );
        FMOD.DSP_PARAMETER_FFT fftData = ( FMOD.DSP_PARAMETER_FFT )Marshal.PtrToStructure( data, typeof( FMOD.DSP_PARAMETER_FFT ) );
        
        if ( fftData.numchannels > 0 )
             UpdateSpectrums( fftData.spectrum[0] );
    }
}
