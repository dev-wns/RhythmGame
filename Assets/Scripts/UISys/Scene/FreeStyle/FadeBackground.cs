using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FadeBackground : MonoBehaviour
{
    public BackgroundChanger system;
    public float fadeTime = 1f;
    private RectTransform rt;
    private Image image;
    private bool isDefault;

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
        rt.sizeDelta = Global.Math.GetScreenRatio( _sprite.texture, new Vector2( Screen.width, Screen.height ) );
        rt.SetAsFirstSibling();
        image.color = Color.white;
        image.sprite = _sprite;
    }

    public void Despawn()
    {
        StartCoroutine( FadeAfterDespawn() );
    }

    private IEnumerator FadeAfterDespawn()
    {
        image.DOFade( 0f, fadeTime );
        yield return new WaitUntil( () => image.color.a < .0001f );
        SpriteRelease();
        system.DeSpawn( this );
    }
}
