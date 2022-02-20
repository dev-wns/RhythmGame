using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent( typeof( RectTransform ) )]
public class ScrollBar : MonoBehaviour
{
    [Range(0f, 1f)]
    public float value;
    public RectTransform handle;

    private float offset;
    private RectTransform rt;
    public bool isSmoothMove = true;

    private void Awake()
    {
        rt = transform as RectTransform;
    }

    public void Initialize( int _size )
    {
        offset = 1f / _size;
    }

    public void UpdateHandle( int _pos )
    {
        float posY = rt.sizeDelta.y - ( handle.sizeDelta.y * .5f );

        if ( isSmoothMove )
            handle.DOAnchorPosY( posY - ( posY * offset * _pos ), .15f );
        else
            handle.anchoredPosition = new Vector2( handle.anchoredPosition.x, posY - ( posY * offset * _pos ) );
    }

    //private virtual void Update()
    //{
    //    // Editor Update
    //    if ( !Application.isPlaying )
    //    {
    //        value = 1f;
    //    }
    //}
}
