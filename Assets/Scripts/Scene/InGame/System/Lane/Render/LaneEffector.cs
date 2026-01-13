using System.Collections.Generic;
using UnityEngine;

public class LaneEffector : MonoBehaviour
{
    private int lane;
    private KeyCode uKey;

    [Header( "Lane Effect" )]
    public  SpriteRenderer laneRenderer;
    private readonly float LaneOffset = 1f / .1f;
    private Color          laneColor;
    private float          laneAlpha;

    [Header( "Hit Effect" )]
    public SpriteRenderer  hitRenderer;
    public List<Sprite>    hitSprites = new ();
    private bool           hasHitEffect;
    private int            hitIndex;
    private float          hitOffset;
    private float          hitTimer;
    private bool           isHitLoop;

    [Header( "Input Effect" )]
    public SpriteRenderer inputRenderer;

    public void Initialize( Lane _lane, int _keyCount )
    {
        lane = _lane.Key;
        uKey = _lane.UKey;

        // 레인 이펙트
        if ( GameSetting.CurrentVisualFlag.HasFlag( VisualFlag.LaneEffect ) )
        {
            laneRenderer.transform.position   = new Vector3( transform.position.x, GameSetting.JudgePos, transform.position.z );
            laneRenderer.transform.localScale = new Vector3( GameSetting.NoteWidth, 250f, 1f );

            // 레인 색상 선택
            if (      _keyCount == 4 ) laneColor = lane == 1 || lane == 2 ? Color.blue : Color.red;
            else if ( _keyCount == 6 ) laneColor = lane == 1 || lane == 4 ? Color.blue : Color.red;
            else if ( _keyCount == 7 ) laneColor = lane == 1 || lane == 5 ? Color.blue :
                                                   lane == 3 ? Color.yellow : Color.red;
        }

        // 타격 이펙트
        if ( hasHitEffect = GameSetting.CurrentVisualFlag.HasFlag( VisualFlag.HitEffect ) )
        {
            hitOffset = .16f / hitSprites.Count;
            hitRenderer.transform.localScale = new Vector2( GameSetting.NoteWidth * 2, GameSetting.NoteWidth * 2 );
            hitRenderer.color = Color.clear;
        }
    }

    public void UpdateEffect()
    {
        UpdateInputEffect();
        UpdateLaneEffect();
        UpdateHitEffect();
    }

    public void Clear()
    {
        isHitLoop = false;
        hitTimer  = 0f;
        hitIndex  = 0;

        inputRenderer.color = Color.clear;
        hitRenderer.color   = Color.clear;
        laneRenderer.color  = new Color( laneColor.r, laneColor.g, laneColor.b, 0f );
    }

    private void UpdateInputEffect()
    {
        if      ( Input.GetKeyDown( uKey ) ) inputRenderer.color = Color.gray;
        else if ( Input.GetKeyUp(   uKey ) ) inputRenderer.color = Color.clear;
    }

    private void UpdateLaneEffect()
    {
        float increment = LaneOffset * Time.deltaTime;
        laneAlpha = Input.GetKey( uKey ) ? Global.Math.Clamp( laneAlpha + increment, 0f, 1f ) :
                                           Global.Math.Clamp( laneAlpha - increment, 0f, 1f );
        
        if ( Global.Math.Abs( laneAlpha - laneRenderer.color.a ) > float.Epsilon )
             laneRenderer.color = new Color( laneColor.r, laneColor.g, laneColor.b, laneAlpha );
    }

    public void SetHitEffect( KeyState _keyState, bool _isSlider )
    {
        if ( !hasHitEffect )
             return;

        if ( _keyState == KeyState.Down )
        {
            isHitLoop = _isSlider;
            hitTimer  = 0f;
            hitIndex  = 0;
            hitRenderer.color = Color.white;
        }
        else
        {
            isHitLoop = false;
        }
    }

    private void UpdateHitEffect()
    {
        hitTimer += Time.deltaTime;
        if ( !isHitLoop )
        {
            if ( hitTimer > hitIndex * hitOffset )
            {
                if ( hitIndex < hitSprites.Count )
                     hitRenderer.sprite = hitSprites[hitIndex++];
                else
                     hitRenderer.color = Color.clear;
            }
        }
        else
        {
            if ( hitTimer > hitIndex * hitOffset )
            {
                if ( hitIndex < hitSprites.Count )
                {
                    hitRenderer.sprite = hitSprites[hitIndex++];
                }
                else
                {
                    hitTimer = 0f;
                    hitIndex = 0;
                }
            }
        }
    }
}
