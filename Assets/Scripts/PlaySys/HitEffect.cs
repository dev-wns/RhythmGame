using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEffect : MonoBehaviour
{
    [HideInInspector]
    public HitEffectSystem system;
    private Animator anim;
    private static readonly float lifeTime = 1f;
    private float time;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        anim.enabled = false;

        transform.position = Vector3.zero;
    }

    public void SetInfo( HitEffectSystem _system )
    {
        system = _system;
        transform.position = _system.transform.position;;
        time = 0f;
        anim.enabled = true;
    }

    private void OnDisable()
    {
        anim.enabled = false;
    }

    private void Update()
    {
        //time += Time.deltaTime;

        //if ( time > lifeTime )
            //system.Despawn( this );
    }
}
