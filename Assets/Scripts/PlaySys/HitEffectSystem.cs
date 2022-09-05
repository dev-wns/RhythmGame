using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HitEffectSystem : MonoBehaviour
{
    public Lane lane;

    private NoteType type;

    private float time;
    public List<Sprite> spritesN = new List<Sprite>();
    private float offsetN;
    public List<Sprite> spritesL = new List<Sprite>();
    private float offsetL;

    private float lifeTime = .12f;

    private SpriteRenderer rdr;
    private int curIndex = 0;
    private bool isPlay;
    private bool isKeyUp;

    protected void Awake()
    {
        rdr = GetComponent<SpriteRenderer>();

        if ( ( GameSetting.CurrentVisualFlag & GameVisualFlag.TouchEffect ) != 0 )
        {
            lane.OnLaneInitialize += Initialize;

            offsetN = lifeTime / spritesN.Count;
            offsetL = lifeTime / spritesL.Count;

            rdr.enabled = true;
            rdr.color = Color.clear;

            //StartCoroutine( Process() );
        }
        else
        {
            enabled = false;
        }
    }

    private void UpdatePosition()
    {
        transform.position = new Vector3( GameSetting.NoteStartPos + ( GameSetting.NoteWidth * lane.Key ) + ( GameSetting.NoteBlank * lane.Key ) + GameSetting.NoteBlank,
                                          GameSetting.CurrentVisualFlag.HasFlag( GameVisualFlag.ShowGearKey ) ? GameSetting.HintPos : GameSetting.JudgePos, 90f );
    }

    private void Initialize( int _key )
    {
        lane.InputSys.OnHitNote += HitEffect;

        UpdatePosition();
        transform.localScale = new Vector2( GameSetting.NoteWidth, GameSetting.NoteWidth );
    }

    private void HitEffect( NoteType _type, bool _isKeyUp )
    {
        type = _type;
        isKeyUp = _isKeyUp;
        curIndex = 0;

        switch ( type )
        {
            case NoteType.Default: rdr.sprite = spritesN[0]; break;
            case NoteType.Slider:  rdr.sprite = spritesL[0]; break;
        }

        if ( !isKeyUp ) Play();
    }

    private void Update()
    {
        if ( !isPlay ) return;

        time += Time.deltaTime;

        switch ( type )
        {
            case NoteType.Default:
            {
                if ( time >= offsetN )
                {
                    if ( curIndex < spritesN.Count - 1 ) rdr.sprite = spritesN[++curIndex];
                    else                                 Stop();

                    time = Globals.Abs( time - offsetN );
                }
            }
            break;

            case NoteType.Slider:
            {
                if ( time >= offsetL )
                {
                    if ( curIndex < spritesL.Count - 1 )
                        rdr.sprite = spritesL[++curIndex];
                    else
                    {
                        if ( isKeyUp )
                            Stop();
                        else
                        {
                            curIndex = 0;
                            Play();
                        }
                    }

                    time = Globals.Abs( time - offsetL );
                }
            }
            break;
        }
    }

    //private IEnumerator Process()
    //{
    //    WaitUntil waitPlay = new WaitUntil( () => isPlay );
    //    while ( true )
    //    {
    //        yield return waitPlay;

    //        switch ( type )
    //        {
    //            case NoteType.Default:
    //            {
    //                rdr.sprite = spritesN[curIndex];
    //                yield return YieldCache.WaitForSeconds( timeN );

    //                if ( curIndex < spritesN.Count - 1 ) curIndex++;
    //                else                                 Stop();
    //            }
    //            break;

    //            case NoteType.Slider:
    //            {
    //                rdr.sprite = spritesL[curIndex];
    //                yield return YieldCache.WaitForSeconds( timeL );

    //                if ( curIndex < spritesL.Count - 1 ) curIndex++;
    //                else
    //                {
    //                    if ( isKeyUp ) Stop();
    //                    else
    //                    {
    //                        curIndex = 0;
    //                        Play();
    //                    }
    //                }
    //            }
    //            break;
    //        }
    //    }
    //}

    private void Play()
    {
        isPlay = true;
        rdr.color = Color.white;
    }

    private void Stop()
    {
        curIndex = 0;
        rdr.color = Color.clear;
        isPlay = false;
    }
}
