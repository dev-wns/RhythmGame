using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Lane : MonoBehaviour
{
    public int Key { get; private set; }
    public InputSystem InputSys { get; private set; }

    public event Action<int/*Lane Key*/> OnLaneInitialize;

    [Header("Effect")]
    public SpriteRenderer laneEffect;
    private bool ShouldPlayLaneEffect;
    private Color color;

    private readonly float StartFadeAlpha = 1f;
    private readonly float FadeDuration = .25f;
    private float fadeOffset;
    private float fadeAlpha;

    private void Awake()
    {
        InputSys = GetComponent<InputSystem>();

        if ( ( GameSetting.CurrentVisualFlag & GameVisualFlag.LaneEffect ) != 0 )
             InputSys.OnInputEvent += LaneEffect;

        fadeOffset = StartFadeAlpha / FadeDuration;
    }

    private void LaneEffect( InputType _type )
    {
        if ( _type == InputType.Down )
        {
            ShouldPlayLaneEffect = false;
            laneEffect.color = color;
            fadeAlpha = StartFadeAlpha;
        }
        else if ( _type == InputType.Up )
        {
            ShouldPlayLaneEffect = true;
        }
    }

    private void Update()
    {
        if ( ShouldPlayLaneEffect )
        {
            fadeAlpha -= fadeOffset * Time.deltaTime;
            Color newColor = color;
            newColor.a = fadeAlpha;
            laneEffect.color = newColor;
            if ( fadeAlpha < 0 )
                 ShouldPlayLaneEffect = false;
        }
    }

    public void SetLane( int _key )
    {
        Key = _key;
        UpdatePosition( _key );
        OnLaneInitialize?.Invoke( Key );
        
        if ( NowPlaying.CurrentSong.keyCount == 4 )
        {
            color = _key == 1 || _key == 2 ? new Color( 0f, 0f, 1f, StartFadeAlpha ) : new Color( 1f, 0f, 0f, StartFadeAlpha );
        }
        else if ( NowPlaying.CurrentSong.keyCount == 6 )
        {
            color = _key == 1 || _key == 4 ? new Color( 0f, 0f, 1f, StartFadeAlpha ) : new Color( 1f, 0f, 0f, StartFadeAlpha );
        }
        else if ( NowPlaying.CurrentSong.keyCount == 7 )
        {
            color = _key == 1 || _key == 5 ? new Color( 0f, 0f, 1f, StartFadeAlpha ) :
                                 _key == 3 ? new Color( 1f, 1f, 0f, StartFadeAlpha ) : new Color( 1f, 0f, 0f, StartFadeAlpha );
        }
    }

    public void UpdatePosition( int _key )
    {
        transform.position = new Vector3( GameSetting.NoteStartPos + ( GameSetting.NoteWidth * _key ) + ( GameSetting.NoteBlank * _key ) + GameSetting.NoteBlank, GameSetting.JudgePos, 0f );
        
        if ( GameSetting.CurrentVisualFlag.HasFlag( GameVisualFlag.LaneEffect ) )
        {
            laneEffect.transform.position   = new Vector3( transform.position.x, GameSetting.JudgePos, transform.position.z );
            laneEffect.transform.localScale = new Vector3( GameSetting.NoteWidth, 250f, 1f );
        }
        else
        {
            laneEffect.gameObject.SetActive( false );
        }
    }
}
