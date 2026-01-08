using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class AudioVisualizer : MonoBehaviour
{
    private FMOD.DSP fft;
    private float[][] stereo;

    public Action<float[][]> OnUpdate;

    private void Awake()
    {
        AudioManager.Inst.OnReload += GetFFT;
    }

    private void OnDestroy()
    {
        AudioManager.Inst.OnReload -= GetFFT;
    }

    private void Start()
    {
        GetFFT();

        stereo     = new float[2][];
        stereo[0]  = new float[4096];
        stereo[1]  = new float[4096];
    }

    private void GetFFT()
    {
        if ( !AudioManager.Inst.GetDSP( FMOD.DSP_TYPE.FFT, out fft ) )
             Debug.Log( "Unable to get FFT DSP" );
    }

    private void Update()
    {
        if ( AudioManager.Inst.IsStop )
             return;

        if ( fft.getParameterData( ( int )FMOD.DSP_FFT.SPECTRUMDATA, out IntPtr data, out uint length ) == FMOD.RESULT.OK )
        {
            FMOD.DSP_PARAMETER_FFT fftData = Marshal.PtrToStructure<FMOD.DSP_PARAMETER_FFT>( data );
            if ( fftData.numchannels <= 0 )
                 return;

            fftData.getSpectrum( ref stereo );
            OnUpdate?.Invoke( stereo );
        }
    }
}