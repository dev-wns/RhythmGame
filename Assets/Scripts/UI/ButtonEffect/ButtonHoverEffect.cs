using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent( typeof( Image ) )]
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image image;
    public float targetAlpha = 1f;
    public float duration    = .25f;
    public bool isSfxPlay    = false;
    private Color color;

    private void Awake()
    {
        if ( TryGetComponent( out image ) )
        {
            color = image.color;
            image.color = new Color( color.r, color.g, color.b, 0f );
        }
    }

    private void OnEnable()
    {
        image.color = new Color( color.r, color.g, color.b, 0f );
    }

    private void OnDestroy()
    {
        DOTween.Kill( image );
    }

    public void OnPointerEnter( PointerEventData _eventData )
    {
        image.DOFade( targetAlpha, duration );
        if ( isSfxPlay )
             AudioManager.Inst.Play( SFX.MenuHover );
    }

    public void OnPointerExit( PointerEventData _eventData )
    {
        image.DOFade( 0f, duration );
    }
}