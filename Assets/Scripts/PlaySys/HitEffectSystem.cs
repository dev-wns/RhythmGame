using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HitEffectSystem : MonoBehaviour
{
    public Lane lane;
    public List<Sprite> sprites = new List<Sprite>();
    private Image image;
    private readonly float lifeTime = .1f;

    private float changeTime;
    private float playback;
    private int currentIndex = 0;
    private bool isStop = true;

    private RectTransform rt;

    protected void Awake()
    {
        rt = transform as RectTransform;
        image = GetComponent<Image>();
        lane.OnLaneInitialize += Initialize;
        changeTime = lifeTime / sprites.Count;
    }

    private void Initialize( int _key )
    {
        lane.InputSys.OnHitNote += HitEffect;

        rt.position = lane.transform.position;
        rt.sizeDelta = new Vector2( GameSetting.NoteWidth * 2f, GameSetting.NoteWidth * 2f );
    }

    private void HitEffect()
    {
        playback = 0f;
        currentIndex = 0;
        image.sprite = sprites[currentIndex];
        isStop = false;
        image.color = Color.white;
    }

    private void Update()
    {
        if ( isStop ) return;

        playback += Time.deltaTime;
        if ( playback >= changeTime )
        {
            if ( currentIndex + 1 < sprites.Count )
            {
                image.sprite = sprites[++currentIndex];
                playback = 0;
            }
            else
            {
                isStop = true;
                image.color = Color.clear;
            }
        }
    }
}
