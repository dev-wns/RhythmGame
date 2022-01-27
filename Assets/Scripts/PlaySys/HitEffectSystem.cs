using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HitEffectSystem : MonoBehaviour
{
    public Lane lane;
    public List<Sprite> sprites = new List<Sprite>();
    private SpriteRenderer rdr;
    private readonly float lifeTime = .1f;

    private float changeTime;
    private float playback;
    private int curIndex = 0;
    private bool isStop = true;

    private Transform tf;

    protected void Awake()
    {
        tf = transform;
        rdr = GetComponent<SpriteRenderer>();
        lane.OnLaneInitialize += Initialize;
        changeTime = lifeTime / sprites.Count;
    }

    private void Initialize( int _key )
    {
        lane.InputSys.OnHitNote += HitEffect;

        tf.position = lane.transform.position;
        tf.localScale = new Vector2( GameSetting.NoteWidth * .75f, GameSetting.NoteWidth * .75f );
    }

    private void HitEffect()
    {
        playback = 0f;
        curIndex = 0;
        rdr.sprite = sprites[curIndex];
        isStop = false;
        rdr.color = Color.white;
    }

    private void Update()
    {
        if ( isStop ) return;

        playback += Time.deltaTime;
        if ( playback >= changeTime )
        {
            if ( curIndex + 1 < sprites.Count )
            {
                rdr.sprite = sprites[++curIndex];
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
