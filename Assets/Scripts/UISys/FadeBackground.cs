using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FadeBackground : MonoBehaviour
{
    public Image image;
    private static float depth = 0f;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void OnEnable()
    {
        transform.localPosition = new Vector3( 0f, 0f, depth += .00001f );
        image.color = new Color( 1f, 1f, 1f, 0f );
        image.DOFade( 1f, .5f );
    }

    public void Despawn()
    {
        StartCoroutine( FadeAfterDespawn() );
    }

    private IEnumerator FadeAfterDespawn()
    {
        image.DOFade( 0f, .5f );
        yield return YieldCache.WaitForSeconds( .5f );
        FreeStyle.bgPool.Despawn( this );
    }
}
