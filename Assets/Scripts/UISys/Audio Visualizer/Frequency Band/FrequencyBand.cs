using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FrequencyBand : MonoBehaviour
{
    public AudioVisualizer visuzlizer;
    
    public enum FreqType { FreqBand, BandBuffer, }
    public FreqType type = FreqType.FreqBand;

    [Header( "Band" )]
    protected float[] freqBand;
    protected float[] bandBuffer;
    protected float[] bufferDecrease;

    [Range(1f, 100f)]
    public float power;

    public Action<float[]> OnUpdateBand;

    protected virtual void Awake()
    {
        visuzlizer.OnUpdateSpectrums += UpdateFreqBand;
        Initialize();
    }

    protected abstract void Initialize();

    protected abstract void UpdateFreqBand( float[][] _values );
}
