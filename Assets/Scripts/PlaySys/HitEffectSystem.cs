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

    protected override void Awake()
    {
        base.Awake();
        rdr         = GetComponent<SpriteRenderer>();
        inputSystem = GetComponentInParent<InputSystem>();
        inputSystem.OnInputEvent += HitEffect;

        changeTime = lifeTime / sprites.Count;
    }

    private void HitEffect( bool a )
    {
        playback = 0f;
        currentIndex = 0;
        rdr.sprite = sprites[currentIndex];
        isStop = false;
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
                isStop = true;
        }
    }
}
