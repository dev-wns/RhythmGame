using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HitEffectSystem : MonoBehaviour
{
    public Lane lane;
    private SpriteRenderer rdr;
    private readonly float lifeTime = .05f;

    public List<Sprite> noteSprites = new List<Sprite>();
    private float noteTime;

    public List<Sprite> sliderSprites = new List<Sprite>();
    private float sliderTime;

    private int curIndex = 0;
    private bool isKeyPress = false;

    private Transform tf;
    private NoteType type;

    protected void Awake()
    {
        tf = transform;
        rdr = GetComponent<SpriteRenderer>();

        if ( ( GameSetting.CurrentVisualFlag & GameVisualFlag.TouchEffect ) != 0 )
        {
            lane.OnLaneInitialize += Initialize;
            noteTime   = lifeTime / noteSprites.Count;
            sliderTime = lifeTime / sliderSprites.Count;
        }
        else
        {
            enabled = false;
        }

        StartCoroutine( Process() );
    }

    private void Initialize( int _key )
    {
        lane.InputSys.OnHitNote += HitEffect;

        tf.position = lane.transform.position;
        tf.localScale = new Vector2( GameSetting.NoteWidth, GameSetting.NoteWidth );
    }

    private void HitEffect( NoteType _type, bool _isKeyPress )
    {
        type = _type;
        curIndex = 0;
        isKeyPress = _isKeyPress;
        rdr.color = isKeyPress ? Color.white : Color.clear;

        switch( _type )
        {
            case NoteType.Default:
            rdr.sprite = noteSprites[curIndex];
            break;

            case NoteType.Slider:
            rdr.sprite = sliderSprites[curIndex];
            break;
        }
    }

    private IEnumerator Process()
    {
        while ( true )
        {
            yield return new WaitUntil( () => isKeyPress );

            switch( type )
            {
                case NoteType.Default:
                yield return YieldCache.WaitForSeconds( noteTime );
                if ( curIndex + 1 < noteSprites.Count )
                {
                    rdr.sprite = noteSprites[++curIndex];
                }
                else
                {
                    isKeyPress = false;
                    rdr.color = Color.clear;
                }
                break;

                case NoteType.Slider:
                yield return YieldCache.WaitForSeconds( sliderTime );
                if ( curIndex + 1 < sliderSprites.Count )
                {
                    rdr.sprite = sliderSprites[++curIndex];
                }
                else
                {
                    curIndex = 0;
                }
                break;
            }
        }
    }

    //private void Update()
    //{
    //    if ( isStop ) return;

    //    playback += Time.deltaTime;
    //    if ( playback >= changeTime )
    //    {
    //        if ( curIndex + 1 < sprites.Count )
    //        {
    //            rdr.sprite = sprites[++curIndex];
    //            playback = 0;
    //        }
    //        else
    //        {
    //            isStop = true;
    //            rdr.color = Color.clear;
    //        }
    //    }
    //}
}
