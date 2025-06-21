using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class AudioVisualizer : MonoBehaviour
{
    [Header( "- Spectrum -" )]
    public int size;
    public int avgStart;
    public int avgRange;

    private FMOD.DSP fft;
    private float[]  spectrums;
    private float[]  bandBuffer;
    private int[]    bandRange;
    private int loopCount;
    public int power;

    public float Average { get; private set; }
    public const int MaxFreqBand = 10;

    public Action<float[]>   OnUpdate;
    public Action<float[]>   OnUpdateBand;

    private void Awake()
    {
        AudioManager.OnReload += Initialize;
    }

    private void OnDestroy()
    {
        AudioManager.OnReload -= Initialize;
    }

    private void Start()
    {
        Initialize();
        spectrums  = new float[size];
        bandBuffer = new float[MaxFreqBand];
        bandRange  = new int  [MaxFreqBand] { 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024 };
        loopCount  = bandRange.Sum();
    }

    private void Initialize()
    {
        if ( !AudioManager.Inst.GetDSP( FMOD.DSP_TYPE.FFT, out fft ) )
              Debug.Log( "Unable to get FFT DSP" );
    }

    /* 48000 / 4096 : 11.71875 Hertz
     * ------------------------------------
     *     count    : Hertz :    Range
     * ------------------------------------
     * 0.  2        : 23    : 0     ~ 23
     * 1.  4        : 47    : 24    ~ 71
     * 2.  8        : 94    : 72    ~ 165
     * 3.  16       : 188   : 165   ~ 352
     * 4.  32       : 375   : 353   ~ 727
     * 5.  64       : 750   : 728   ~ 1477
     * 6.  128      : 1500  : 1478  ~ 2977
     * 7.  256      : 3000  : 2978  ~ 5977
     * 8.  512      : 6000  : 5978  ~ 11977
     * 9.  1024     : 12000 : 11978 ~ 23977
     * 10. 2048 + 2 : 24023 : 23978 ~ 48000
     * Total : 4096
     */

    private void FixedUpdate()
    {
        if ( !fft.hasHandle() && AudioManager.Inst.IsStop )
             return;

        if ( fft.getParameterData( ( int )FMOD.DSP_FFT.SPECTRUMDATA, out IntPtr data, out uint length ) == FMOD.RESULT.OK )
        {
            FMOD.DSP_PARAMETER_FFT fftData = ( FMOD.DSP_PARAMETER_FFT )Marshal.PtrToStructure( data, typeof( FMOD.DSP_PARAMETER_FFT ) );
            if ( fftData.numchannels <= 0 )
                 return;

            fftData.getSpectrum( 0, ref spectrums );

            int   averageCount = 0;
            int   bandCount    = 0;
            int   bandIndex    = 0;
            float bandSum      = 0f;
            float defulatSum   = 0f;
            for ( int i = 0; i < loopCount; i++ )
            {
                // Band
                if ( bandIndex < MaxFreqBand )
                {
                    bandSum += spectrums[i];
                    if ( bandRange[bandIndex] <= ++bandCount )
                    {
                        bandBuffer[bandIndex] = bandSum / bandRange[bandIndex];
                        bandSum    = 0f;
                        bandCount  = 0;
                        bandIndex += 1;
                    }
                }

                if ( i >= avgStart && averageCount < avgRange )
                {
                    defulatSum += spectrums[i];
                    averageCount++;
                }
            }

            Average = defulatSum / avgRange;
            OnUpdateBand?.Invoke( bandBuffer );
            OnUpdate?.Invoke( spectrums );
        }
    }
}