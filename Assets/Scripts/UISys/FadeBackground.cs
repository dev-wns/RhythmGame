using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FadeBackground : MonoBehaviour
{
    public BackgroundSystem system;
    public float fadeTime = .5f;

    private RectTransform rt;
    private Image image;
    private static float depth = 0f;

    private void Awake()
    {
        image = GetComponent<Image>();
        rt    = transform as RectTransform;
    }
    private void OnDestroy()
    {
        if ( image.sprite )
        {
            if ( image.sprite.texture )
            {
                DestroyImmediate( image.sprite.texture );
            }
            Destroy( image.sprite );
        }
    }

    private Vector3 GetFullScreenRatio( Texture2D _tex )
    {
        float width  = _tex.width;
        float height = _tex.height;

        float offsetX = ( float )Screen.width / _tex.width;
        width  *= offsetX;
        height *= offsetX;

        float offsetY = ( float )Screen.height / height;
        if ( offsetY > 1f )
        {
            width  *= offsetY;
            height *= offsetY;
        }

        return new Vector3( width, height, 1f );
    }


    public void SetInfo( Sprite _sprite )
    {
        rt.anchoredPosition = new Vector3( 0f, 0f, depth += .00001f );
        rt.sizeDelta        = GetFullScreenRatio( _sprite.texture );
        image.color         = new Color( 1f, 1f, 1f, 0f );
        image.sprite        = _sprite;
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
        
        if ( image.sprite )
        {
            if ( image.sprite.texture )
            {
                DestroyImmediate( image.sprite.texture );
            }
            Destroy( image.sprite );
        }

        system.DeSpawn( this );
    }
}
