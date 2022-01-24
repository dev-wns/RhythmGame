using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HitEffectSystem : MonoBehaviour
{
    public List<Sprite> sprites = new List<Sprite>();
    private InputSystem inputSystem;
    private SpriteRenderer rdr;
    private readonly float lifeTime = .1f;

    private float changeTime;
    private float playback;
    private int currentIndex = 0;
    private bool isStop = true;
    private static float depth;

    protected void Awake()
    {
        rdr         = GetComponent<SpriteRenderer>();
        inputSystem = GetComponentInParent<InputSystem>();
        inputSystem.OnHitNote += HitEffect;

        transform.localScale = new Vector2( GameSetting.NoteWidth, GameSetting.NoteWidth );

        changeTime = lifeTime / sprites.Count;
    }

    private void HitEffect()
    {
        playback = 0f;
        currentIndex = 0;
        rdr.sprite = sprites[currentIndex];
        isStop = false;

        //transform.localPosition = new Vector3( 0f, 0f, depth -= .00001f );
        rdr.color = Color.white;
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
