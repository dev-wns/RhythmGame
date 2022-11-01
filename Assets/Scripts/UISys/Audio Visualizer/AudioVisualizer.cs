using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class AudioVisualizer : MonoBehaviour
{
    private float[][] spectrums;

    [Header("Particle")]
    public  ParticleSystem particle;
    private ParticleSystem.MainModule mainModule;
    private bool hasParticle;
    [Min(0f)] public float particlePower = 1f;

    [Header("Bass")]
    public bool hasBass;
    private float bassCached;
    [Range(0f, 1f)] public float bassDecrease;
    [Min(0f)]       public float bassPower = 1f;
    [Range(1, 256)] public int   bassRange;

    public float Bass    { get; private set; }
    public Action<float[][]> OnUpdateSpectrums;

    protected virtual void Awake()
    {
        if ( particle )
        {
            mainModule  = particle.main;
            StartCoroutine( ParticleInit() );
        }
        StartCoroutine( FixedSpectrumUpdate() );
    }

    protected IEnumerator ParticleInit()
    {
        mainModule.simulationSpeed = 1000;
        yield return YieldCache.WaitForSeconds( .1f );
        hasParticle = true;
    }

    private IEnumerator FixedSpectrumUpdate()
    {
        float targetFrame = 1f / 144f;
        while ( true )
        {
            yield return YieldCache.WaitForSeconds( targetFrame );
            if ( SoundManager.Inst.IsLoad )
                 continue;

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
                    if ( bassCached < bassAmount ) bassCached = bassAmount;
                    else                           bassCached = Mathf.Clamp01( bassCached - Mathf.Lerp( 0f, diffAbs, bassDecrease ) );
                    Bass = 1f + bassCached;
                }

                if ( hasParticle )
                {
                    mainModule.simulationSpeed = bassAmount * particlePower;
                }

                OnUpdateSpectrums?.Invoke( spectrums );
            }
        }
    }
}