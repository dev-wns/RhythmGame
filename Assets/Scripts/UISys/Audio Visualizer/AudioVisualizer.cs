using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class AudioVisualizer : MonoBehaviour
{
    private FMOD.DSP fftWindowDSP;
    public event System.Action<float[] /* values */> UpdateSpectrums;

    private void Awake()
    {
        SoundManager.Inst.GetDSP( FMOD.DSP_TYPE.FFT, out fftWindowDSP );
        SoundManager.Inst.AddDSP( in fftWindowDSP, ChannelType.BGM );
    }

    private void FixedUpdate()
    {
        if ( SoundManager.Inst.IsLoad ) return;

        uint length;
        System.IntPtr data;
        fftWindowDSP.getParameterData( ( int )FMOD.DSP_FFT.SPECTRUMDATA, out data, out length );
        FMOD.DSP_PARAMETER_FFT fftData = ( FMOD.DSP_PARAMETER_FFT )Marshal.PtrToStructure( data, typeof( FMOD.DSP_PARAMETER_FFT ) );
        
        if ( fftData.numchannels > 0 )
        {
            UpdateSpectrums?.Invoke( fftData.spectrum[0] );
        }
    }
}
