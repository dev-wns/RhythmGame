using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEffectSystem : NumberAtlasBase
{
    private InputSystem inputSystem;
    private SpriteRenderer rdr;
    private readonly float lifeTime = .5f;
    
    private float changeTime;
    private float playback;
    private int currentIndex = 0;
    private bool isStop;
    private static float depth;
    private static Color halfAlpha = new Color( 1f, 1f, 1f, .75f );

    protected override void Awake()
    {
        base.Awake();
        rdr         = GetComponent<SpriteRenderer>();
        inputSystem = GetComponentInParent<InputSystem>();
        inputSystem.OnHitNote += HitEffect;

        changeTime = lifeTime / sprites.Count;
        rdr.color = Color.clear;
    }

    private void HitEffect()
    {
        playback = 0f;
        currentIndex = 0;
        rdr.sprite = sprites[currentIndex];
        isStop = false;

        transform.localPosition = new Vector3( 0f, 0f, depth -= .00001f );
        rdr.color = halfAlpha;
    }

    private void Update()
    {
        if ( isStop ) return;

        playback += Time.deltaTime;
        if ( playback >= changeTime )
        {
            if ( currentIndex + 1 < sprites.Count )
            {
                rdr.sprite = sprites[++currentIndex];
                playback = 0;
            }
            else
            {
                isStop = true;
                rdr.color = Color.clear;
            }
        }
    }
}
