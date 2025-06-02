using System.Collections.Generic;
using UnityEngine;

public class HitEffectSystem : MonoBehaviour
{
    private Lane lane;
    private NoteType type;

    private float time;
    public List<Sprite> spritesN = new List<Sprite>();
    private float offsetN;
    public List<Sprite> spritesL = new List<Sprite>();
    private float offsetL;

    private readonly float HitEffectFramePerSecond = 1f / 75f;

    private SpriteRenderer rdr;
    private int curIndex = 0;
    private bool isPlay;
    private KeyState inputType;

    protected void Awake()
    {
        lane = GetComponentInParent<Lane>();
        rdr = GetComponent<SpriteRenderer>();

        if ( ( GameSetting.CurrentVisualFlag & VisualFlag.TouchEffect ) != 0 )
        {
            lane.OnLaneInitialize += Initialize;

            offsetN = HitEffectFramePerSecond; // lifeTime / spritesN.Count;
            offsetL = HitEffectFramePerSecond; // lifeTime / spritesL.Count;

            rdr.color = Color.clear;
        }
        else
        {
            gameObject.SetActive( false );
        }
    }

    private void UpdatePosition()
    {
        transform.position = new Vector3( GameSetting.NoteStartPos + ( GameSetting.NoteWidth * lane.Key ) + ( GameSetting.NoteBlank * lane.Key ) + GameSetting.NoteBlank, GameSetting.JudgePos, 90f );
    }

    private void Initialize( int _key )
    {
        lane.InputSys.OnHitNote += HitEffect;
        lane.InputSys.OnStopEffect += SetCurrentInput;

        UpdatePosition();
        float size = GameSetting.NoteWidth * 2;
        transform.localScale = new Vector2( size, size );
    }

    private void SetCurrentInput() => inputType = KeyState.Up;

    private void HitEffect( NoteType _noteType, KeyState _inputType )
    {
        type = _noteType;
        inputType = _inputType;

        switch ( type )
        {
            case NoteType.Default:
            {
                Play();
                rdr.sprite = spritesN[0];
                curIndex = 0;
            }
            break;

            case NoteType.Slider:
            {
                if ( inputType == KeyState.Down )
                {
                    Play();
                    curIndex = 0;
                    rdr.sprite = spritesL[0];
                }
            }
            break;
        }

    }

    private void Update()
    {
        if ( !isPlay ) return;

        time += Time.deltaTime;

        switch ( type )
        {
            case NoteType.Default:
            {
                if ( time > offsetN )
                {
                    if ( curIndex < spritesN.Count - 1 ) rdr.sprite = spritesN[++curIndex];
                    else Stop();

                    time = 0f;
                }
            }
            break;

            case NoteType.Slider:
            {
                if ( time > offsetL )
                {
                    if ( curIndex < spritesL.Count - 1 )
                    {
                        rdr.sprite = spritesL[++curIndex];
                    }
                    else
                    {
                        if ( inputType == KeyState.Up )
                            Stop();
                        else
                        {
                            curIndex = 0;
                            Play();
                        }
                    }

                    time = 0f;
                }
            }
            break;
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
