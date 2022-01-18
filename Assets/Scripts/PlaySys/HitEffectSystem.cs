using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEffectSystem : MonoBehaviour
{
    private InputSystem inputSystem;
    private ObjectPool<HitEffect> ePool;
    public HitEffect ePrefab;

    private void Awake()
    {
        inputSystem = GetComponent<InputSystem>();
        inputSystem.OnHitNote += HitEffectSpawn;

        ePool = new ObjectPool<HitEffect>( ePrefab, 10 );
    }

    private void HitEffectSpawn()
    {
        var hitEffect = ePool.Spawn();
        hitEffect.SetInfo( this );
    }

    public void Despawn( HitEffect _hitEffect ) => ePool.Despawn( _hitEffect );
}
