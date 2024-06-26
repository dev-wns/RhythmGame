using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ButtonScaler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Range(1f, 2f)]
    public float multiplier;
    public float duration;

    private Vector3 targetSize;
    private Transform tf;

    private void Awake()
    {
        tf = transform;
        targetSize = new Vector3( multiplier, multiplier, 1f );
    }

    private void OnDestroy()
    {
        DOTween.Kill( tf );
    }

    private void OnDisable()
    {
        tf.localScale = Vector3.one;
    }

    public void OnPointerEnter( PointerEventData eventData )
    {
        tf.DOScale( targetSize, duration );
    }

    public void OnPointerExit( PointerEventData eventData )
    {
        tf.DOScale( Vector3.one, duration );
    }
}