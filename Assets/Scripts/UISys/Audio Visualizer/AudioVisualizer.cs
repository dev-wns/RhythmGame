using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class AudioVisualizer : MonoBehaviour
{
    public enum SpectrumSize { _512 = 512, _1024 = 1024, _2048 = 2048, _4096 = 4096, _8192 = 8192 }
    
    private FMOD.DSP dsp;
    public FMOD.DSP_FFT_WINDOW type = FMOD.DSP_FFT_WINDOW.BLACKMANHARRIS;
    public SpectrumSize size        = SpectrumSize._4096;
    public event System.Action<float[] /* values */> UpdateSpectrums;

    private void Awake()
    {
        AddFFT();
        SoundManager.Inst.OnSoundSystemReLoad += AddFFT;
        SoundManager.Inst.OnRelease += RemoveDSP;
    }

    private void RemoveDSP() => SoundManager.Inst.RemoveDSP( ref dsp );
    private void AddFFT() => SoundManager.Inst.AddFFT( ( int )size, type, out dsp );

    private void OnDestroy()
    {
        SoundManager.Inst.RemoveDSP( ref dsp );
        SoundManager.Inst.OnSoundSystemReLoad -= AddFFT;
        SoundManager.Inst.OnRelease           -= RemoveDSP;
    }

    private void FixedUpdate()
    {
        if ( SoundManager.Inst.IsLoad ) return;

        uint length;
        System.IntPtr data;
        dsp.getParameterData( ( int )FMOD.DSP_FFT.SPECTRUMDATA, out data, out length );
        FMOD.DSP_PARAMETER_FFT fftData = ( FMOD.DSP_PARAMETER_FFT )Marshal.PtrToStructure( data, typeof( FMOD.DSP_PARAMETER_FFT ) );
        
        if ( fftData.numchannels > 0 )
        {
            UpdateSpectrums?.Invoke( fftData.spectrum[0] );
        }
    }
}
