using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class AudioVisualizer : MonoBehaviour
{
    private const int SpectrumRange = 512;
    private float[][] spectrums;

    [Header("Particle")]
    public  ParticleSystem particle;
    private ParticleSystem.MainModule mainModule;
    [Min(0f)]
    public float particlePower = 1f;
    private bool hasParticle;

    [Header("Bass")]
    [Min(0f)]
    public float bassPower = 1f;
    [Range(1, SpectrumRange)]
    public int bassRange;

    //public float Highest { get; private set; }
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
    }

    protected IEnumerator ParticleInit()
    {
        mainModule.simulationSpeed = 1000;
        yield return YieldCache.WaitForSeconds( .1f );
        hasParticle = true;
    }

    protected virtual void Update()
    {
        if ( SoundManager.Inst.IsLoad ) return;

        uint length;
        IntPtr data;
        FMOD.DSP fftWindowDSP;
        if ( !SoundManager.Inst.GetDSP( FMOD.DSP_TYPE.FFT, out fftWindowDSP ) )
        {
            Debug.LogWarning( "FFTWindowData is not Load" );
            return;
        }
        fftWindowDSP.getParameterData( ( int )FMOD.DSP_FFT.SPECTRUMDATA, out data, out length );
        FMOD.DSP_PARAMETER_FFT fftData = ( FMOD.DSP_PARAMETER_FFT )Marshal.PtrToStructure( data, typeof( FMOD.DSP_PARAMETER_FFT ) );
        spectrums = fftData.spectrum;

        float sumValue = 0f;
        for ( int i = 0; i < SpectrumRange; i++ )
        {
            float value = ( spectrums[0][i] + spectrums[1][i] ) *.5f;
            //if ( value > Highest )
            //     Highest = value;

            if ( i < bassRange )
                 sumValue += value;
        }

        Average = sumValue / SpectrumRange;

        float bassAmount = Average * bassPower;
        Bass = 1f + bassAmount;

        if ( hasParticle )
        {
            mainModule.simulationSpeed = bassAmount * particlePower;
        }

        OnUpdateSpectrums?.Invoke( spectrums );
    }
}