using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreMeterRenderer : MonoBehaviour, IObjectPool<ScoreMeterRenderer>
{
    public ObjectPool<ScoreMeterRenderer> pool { get; set; }

    private Image image;
    private Color colorCache;
    private RectTransform rectTransform;

    private static readonly float Duration = 1f;
    private static readonly float WaitTime = .5f;
    private float time;
    private float alpha;
    private float offset;
    private bool isStart;

    private void Awake()
    {
        image = GetComponent<Image>();
        rectTransform = transform as RectTransform;
    }

    private void Update()
    {
        time += Time.deltaTime;
        if ( !isStart || time < WaitTime )
             return;

        alpha -= ( Time.deltaTime / Duration ) * offset;
        image.color = new Color( colorCache.r, colorCache.g, colorCache.b, alpha );

        if ( alpha <= 0f )
        {
            isStart = false;
            pool.Despawn( this );
        }
    }

    public void SetInfo( Color _color, Vector2 _pos )
    {
        isStart = true;
        time = 0f;
        colorCache = image.color = _color;
        alpha = offset = _color.a;
        rectTransform.anchoredPosition = _pos;
    }
}