using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FadeBackground : MonoBehaviour
{
    public BackgroundSystem system;
    public float fadeTime = .5f;

    private Image image;
    private static float depth = 0f;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void SetInfo( Sprite _sprite )
    {
        transform.localPosition = new Vector3( 0f, 0f, depth += .00001f );
        image.color = new Color( 1f, 1f, 1f, 0f );
        image.sprite = _sprite;
        image.DOFade( 1f, fadeTime );
    }

    public void Despawn()
    {
        StartCoroutine( FadeAfterDespawn() );
    }

    private IEnumerator FadeAfterDespawn()
    {
        image.DOFade( 0f, fadeTime );
        yield return YieldCache.WaitForSeconds( fadeTime );
        system.DeSpawn( this );
    }
}
