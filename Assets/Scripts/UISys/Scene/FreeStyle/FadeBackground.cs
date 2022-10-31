using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FadeBackground : MonoBehaviour
{
    public float fadeTime = .25f;
    public Color color;
    private RectTransform rt;
    private ImagePreview system;
    private Image image;
    private bool isDefault;
    private WaitUntil waitAlphaZero;

    private void Awake()
    {
        image = GetComponent<Image>();
        rt    = transform as RectTransform;

        waitAlphaZero = new WaitUntil( () => image.color.a < .0001f );
    }

    private void OnDestroy() => ClearSprite();

    private void ClearSprite()
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

    public void SetInfo( ImagePreview _system, Sprite _sprite, bool _isDefault = true )
    {
        system = _system;
        isDefault = _isDefault;
        rt.sizeDelta = Global.Math.GetScreenRatio( _sprite.texture, new Vector2( Screen.width, Screen.height ) );
        rt.SetAsFirstSibling();
        image.color = color;
        image.sprite = _sprite;
    }

    public void Despawn()
    {
        StartCoroutine( FadeAfterDespawn() );
    }

    private IEnumerator FadeAfterDespawn()
    {
        image.DOFade( 0f, fadeTime );
        yield return waitAlphaZero;
        ClearSprite();
        system.DeSpawn( this );
    }
}
