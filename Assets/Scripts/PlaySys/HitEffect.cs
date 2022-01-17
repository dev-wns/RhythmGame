using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEffect : MonoBehaviour
{
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        
        StartCoroutine( PlayAnim() );
    }

    IEnumerator PlayAnim()
    {
        while( true )
        {
            yield return YieldCache.WaitForSeconds( 2f );
            
        }
    }
}
