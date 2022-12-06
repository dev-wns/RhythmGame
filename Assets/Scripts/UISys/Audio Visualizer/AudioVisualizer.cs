using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class AudioVisualizer : MonoBehaviour
{
    private float[][] spectrums;

    [Header("Bass")]
    public bool hasBass;
    private float bassCached;
    [Range(0f, 1f)] public float bassIncrease;
    [Range(0f, 1f)] public float bassDecrease;
    [Min(0f)]       public float bassPower = 1f;
    [Range(1, 256)] public int   bassRange;

    public Action<float[][]> OnUpdateSpectrums;
    public Action<float> OnUpdateBass;

    private void Update()
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
        {
            float bassAmount = 0f;
            if ( hasBass )
            {
                float sumValue = 0f;
                for ( int i = 0; i < bassRange; i++ )
                {
                    sumValue += ( spectrums[0][i] + spectrums[1][i] ) * .5f;
                }
                bassAmount = ( sumValue / bassRange ) * bassPower;

                float diffAbs = Global.Math.Abs( bassCached - bassAmount );
                if ( bassCached < bassAmount )
                    bassCached = Mathf.Clamp01( bassCached + Mathf.Lerp( 0f, diffAbs, bassIncrease ) );
                else
                    bassCached = Mathf.Clamp01( bassCached - Mathf.Lerp( 0f, diffAbs, bassDecrease ) );

                OnUpdateBass?.Invoke( bassCached );
            }

            OnUpdateSpectrums?.Invoke( spectrums );
        }
    }
}