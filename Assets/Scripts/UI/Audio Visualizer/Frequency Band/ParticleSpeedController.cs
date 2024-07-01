using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSpeedController : MonoBehaviour
{
    [Header("Base")]
    public FrequencyBand band;

    [Header("Particle")]
    private ParticleSystem particle;
    private ParticleSystem.MainModule mainModule;
    [Min(0f)]        public float power = 1f;
    [Range(0f, 30f)] public float decrease;
    private float buffer;

    private void Awake()
    {
        if ( !TryGetComponent( out particle ) )
             Debug.LogWarning( "ParticleSystem Component is not found." );

        mainModule = particle.main;
        StartCoroutine( ParticleInit() );
    }

    private void SpeedUpdate( float[] _amount )
    {
        if ( buffer < _amount[0] )
            buffer = _amount[0];

        buffer = Global.Math.Clamp( buffer - ( Global.Math.Abs( _amount[0] - buffer ) * decrease * Time.deltaTime ), 0f, 1f );
        // buffer = buffer < _amount[0] ? _amount[0] : Mathf.Lerp( buffer, _amount[0], decrease * Time.deltaTime );
        mainModule.simulationSpeed = buffer * power;
    }

    protected IEnumerator ParticleInit()
    {
        mainModule.simulationSpeed = 1000;
        yield return YieldCache.WaitForSeconds( .1f );
        band.OnUpdateBand += SpeedUpdate;
    }
}
