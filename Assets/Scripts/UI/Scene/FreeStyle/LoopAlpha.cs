using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoopAlpha : MonoBehaviour
{
    private Image image;
    public float speed = 1;
    [Range(0f, 1f)]
    public float minAlpha = 0f;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void Update()
    {
        float alpha = minAlpha + ( ( 1f + Mathf.Cos( NowPlaying.GameTime * speed ) ) * .5f * ( 1f - minAlpha ) );
        image.color = new Color( 1f, 1f, 1f, alpha );
    }
}
