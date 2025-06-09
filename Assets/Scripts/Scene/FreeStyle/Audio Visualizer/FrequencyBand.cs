using System;
using UnityEngine;

public abstract class FrequencyBand : MonoBehaviour
{
    public AudioVisualizer visuzlizer;

    [Header( "Band" )]
    protected float[] freqBand;
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
