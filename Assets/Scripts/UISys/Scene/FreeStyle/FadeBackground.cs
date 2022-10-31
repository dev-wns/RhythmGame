using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FadeBackground : MonoBehaviour
{
    public Color color;
    private RectTransform rt;
    private FadeBackgroundSystem system;
    private Image image;
    private bool isDefault;

    private readonly float fadeTime = .25f;
    private bool isPlay;
    private float offset;

    private void Awake()
    {
        image  = GetComponent<Image>();
        rt     = transform as RectTransform;
        offset = color.a / fadeTime;
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

    private void Update()
    {
        if ( !isPlay )
             return;

        Color newColor = image.color;
        newColor.a -= offset * Time.deltaTime;
        image.color = newColor;

        if ( image.color.a <= 0f )
        {
            isPlay = false;
            ClearSprite();
            system.DeSpawn( this );
        }
    }

    public void SetInfo( FadeBackgroundSystem _system, Sprite _sprite, bool _isDefault = true )
    {
        ClearSprite();

        system = _system;
        isDefault = _isDefault;
        rt.sizeDelta = Global.Math.GetScreenRatio( _sprite.texture, new Vector2( Screen.width, Screen.height ) );
        rt.SetAsFirstSibling();
        image.color = color;
        image.sprite = _sprite;
        isPlay = false;
    }

    public void Despawn()
    {
        isPlay = true;
    }
}
