using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FadeBackground : MonoBehaviour
{
    public BackgroundChanger system;
    public float fadeTime = .5f;

    private RectTransform rt;
    private Image image;
    private bool isDefault;
    private static float depth = 0f;

    private void Awake()
    {
        image = GetComponent<Image>();
        rt    = transform as RectTransform;
    }

    private void OnDestroy() => SpriteRelease();

    private void SpriteRelease()
    {
        if ( !isDefault && image.sprite )
        {
            if ( image.sprite.texture )
            {
                DestroyImmediate( image.sprite.texture );
            }
            Destroy( image.sprite );
        }
    }

    public void SetInfo( Sprite _sprite, bool _isDefault = true )
    {
        isDefault = _isDefault;
        rt.anchoredPosition = new Vector3( 0f, 0f, depth += .00001f );
        rt.sizeDelta        = Global.Math.GetScreenRatio( _sprite.texture, new Vector2( Screen.width, Screen.height ) );
        image.color = new Color( 1f, 1f, 1f, 0f );
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

        SpriteRelease();
        system.DeSpawn( this );
    }
}
