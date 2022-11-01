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
    [Min(0f)]
    public float particlePower = 1f;
    private bool hasParticle;

    [Header("Bass")]
    public bool hasBass;
    [Min(0f)]
    public float bassPower = 1f;
    [Range(1, 256)]
    public int bassRange;

    public float Average { get; private set; }
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

    public float bass;
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
                    Average = sumValue / bassRange;

                    bassAmount = Average * bassPower;
                    Bass = 1f + bassAmount;
                    bass = Bass;
                }

                if ( hasParticle )
                {
                    mainModule.simulationSpeed = bassAmount * particlePower;
                }

                OnUpdateSpectrums?.Invoke( spectrums );
            }
        }
    }

    //protected virtual void Update()
    //{
    //    if ( SoundManager.Inst.IsLoad ) return;

    //    uint length;
    //    IntPtr data;
    //    FMOD.DSP fftWindowDSP;
    //    if ( !SoundManager.Inst.GetDSP( FMOD.DSP_TYPE.FFT, out fftWindowDSP ) )
    //    {
    //        Debug.LogWarning( "FFTWindowData is not Load" );
    //        return;
    //    }
    //    fftWindowDSP.getParameterData( ( int )FMOD.DSP_FFT.SPECTRUMDATA, out data, out length );
    //    FMOD.DSP_PARAMETER_FFT fftData = ( FMOD.DSP_PARAMETER_FFT )Marshal.PtrToStructure( data, typeof( FMOD.DSP_PARAMETER_FFT ) );
    //    spectrums = fftData.spectrum;

    //    float sumValue = 0f;
    //    for ( int i = 0; i < bassRange; i++ )
    //    {
    //        sumValue += ( spectrums[0][i] + spectrums[1][i] ) * .5f;
    //    }
    //    Average = sumValue / bassRange;

    //    float bassAmount = Average * bassPower;
    //    Bass = 1f + bassAmount;

    //    if ( hasParticle )
    //    {
    //        mainModule.simulationSpeed = bassAmount * particlePower;
    //    }

    //    OnUpdateSpectrums?.Invoke( spectrums );
    //}
}