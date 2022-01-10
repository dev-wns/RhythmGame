using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class AudioVisualizer : MonoBehaviour
{
    public enum SpectrumSize { _512 = 512, _1024 = 1024, _2048 = 2048, _4096 = 4096 }
    
    private FMOD.DSP dsp;
    public FMOD.DSP_FFT_WINDOW type = FMOD.DSP_FFT_WINDOW.BLACKMANHARRIS;
    public SpectrumSize size        = SpectrumSize._4096;

    public delegate void DelUpdateSpectrums( float[] _values );
    public event DelUpdateSpectrums UpdateSpectrums;

    private void Awake()
    {
        SoundManager.Inst.AddFFT( ( int )size, type, out dsp );
        
        SoundManager.Inst.OnSoundSystemReLoad += 
            () => { SoundManager.Inst.AddFFT( ( int )size, type, out dsp ); };
    }

    private void OnDestroy()
    {
        SoundManager.Inst.RemoveFFT();
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
            UpdateSpectrums( fftData.spectrum[0] );
        }
    }

    private float CurrentVolume()
    {
        float volume = SoundManager.Inst.GetVolume();
        if ( volume >= 1f ) return 1f;
        else                return ( 1f - volume ) * 10f;
    }
}
