using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HitEffectSystem : MonoBehaviour
{
    public Lane lane;

    private NoteType type;

    public List<Sprite> spritesN = new List<Sprite>();
    private float timeN = 0f;
    public List<Sprite> spritesL = new List<Sprite>();
    private float timeL = 0f;

    private float lifeTime = .1f;

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

            timeN = lifeTime / spritesN.Count;
            timeL = lifeTime / spritesL.Count;

            rdr.enabled = true;
            rdr.color = Color.clear;

            StartCoroutine( Process() );
        }
        else
        {
            enabled = false;
        }
    }
    private void Start()
    {
        ( NowPlaying.CurrentScene as InGame ).OnShowGearKey += UpdatePosition;
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


        if ( !isKeyUp ) Play();
    }

    private IEnumerator Process()
    {
        WaitUntil waitPlay = new WaitUntil( () => isPlay );
        while ( true )
        {
            yield return waitPlay;

            switch ( type )
            {
                case NoteType.Default:
                {
                    rdr.sprite = spritesN[curIndex];
                    yield return YieldCache.WaitForSeconds( timeN );

                    if ( curIndex < spritesN.Count - 1 ) curIndex++;
                    else                                 Stop();
                }
                break;

                case NoteType.Slider:
                {
                    rdr.sprite = spritesL[curIndex];
                    yield return YieldCache.WaitForSeconds( timeL );

                    if ( curIndex < spritesL.Count - 1 ) curIndex++;
                    else
                    {
                        if ( isKeyUp ) Stop();
                        else
                        {
                            curIndex = 0;
                            Play();
                        }
                    }
                }
                break;
            }
        }
    }

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
