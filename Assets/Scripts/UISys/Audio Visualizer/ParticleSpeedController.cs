using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSpeedController : MonoBehaviour
{
    [Header("Base")]
    public AudioVisualizer visualizer;

    [Header("Particle")]
    private  ParticleSystem particle;
    private ParticleSystem.MainModule mainModule;
    [Min(0f)] public float power = 1f;

    private void Awake()
    {
        if ( !TryGetComponent( out particle ) )
             Debug.LogWarning( "ParticleSystem Component is not found." );

        mainModule = particle.main;
        StartCoroutine( ParticleInit() );
    }

    private void SpeedUpdate( float _amount )
    {
        mainModule.simulationSpeed = _amount * power;
    }

    protected IEnumerator ParticleInit()
    {
        mainModule.simulationSpeed = 1000;
        yield return YieldCache.WaitForSeconds( .1f );
        visualizer.OnUpdateBass += SpeedUpdate;
    }
}
