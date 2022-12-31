using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using UnityEngine;
using UnityEngine.UI;

public class Lane : MonoBehaviour
{
    public int Key { get; private set; }
    public NoteSystem  NoteSys  { get; private set; }
    public InputSystem InputSys { get; private set; }

    public event Action<int/*Lane Key*/> OnLaneInitialize;

    [Header("Gear Key")]
    public SpriteRenderer keyImage;
    public Sprite keyDefaultSprite, keyPressSprite;
    //public float gearKeySliceMultiplier;

    [Header("Effect")]
    public SpriteRenderer laneEffect;

    private Color color;

    private readonly float StartFadeAlpha = 1f;
    private readonly float FadeDuration = .15f;
    private float fadeOffset;
    private float fadeAlpha;
    private bool isEnabled;

    private void Awake()
    {
        NoteSys  = GetComponent<NoteSystem>();
        InputSys = GetComponent<InputSystem>();

        if ( ( GameSetting.CurrentVisualFlag & GameVisualFlag.LaneEffect ) != 0 )
             InputSys.OnInputEvent += LaneEffect;

        if ( ( GameSetting.CurrentVisualFlag & GameVisualFlag.ShowGearKey ) != 0 )
        {
            InputSys.OnInputEvent += KeyEffect;
            keyImage.drawMode = SpriteDrawMode.Sliced;
            keyImage.size     = new Vector2( 2, 1 );
        }

        fadeOffset = StartFadeAlpha / FadeDuration;
    }

    private void LaneEffect( bool _isEnable )
    {
        //laneEffect.color = _isEnable ? color : Color.clear;
        isEnabled = _isEnable;
        if ( isEnabled )
        {
            laneEffect.color = color;
            fadeAlpha = StartFadeAlpha;
        }
    }

    private void KeyEffect( bool _isEnable ) => keyImage.sprite = _isEnable ? keyPressSprite : keyDefaultSprite;

    private void Update()
    {
        if ( !isEnabled && fadeAlpha > 0 )
        {
            fadeAlpha -= fadeOffset * Time.deltaTime;
            Color newColor = color;
            newColor.a = fadeAlpha;
            laneEffect.color = newColor;
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
        //color = _key == 1 || _key == 4 ? new Color( 0, 0, 1, StartFadeAlpha ) : new Color( 1, 0, 0, StartFadeAlpha );
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
            keyImage.gameObject.SetActive( false );
        }

        if ( GameSetting.CurrentVisualFlag.HasFlag( GameVisualFlag.ShowGearKey ) )
        {
            keyImage.transform.position = transform.position;
            // new Vector3( transform.position.x, keyImage.transform.position.y, keyImage.transform.position.z );
            
            keyImage.transform.localScale = new Vector3( GameSetting.NoteHeight + GameSetting.NoteBlank, GameSetting.NoteHeight + GameSetting.NoteBlank );
            // new Vector3( GameSetting.NoteWidth + GameSetting.NoteBlank, keyImage.transform.localScale.y );
        }
        else
        {
            keyImage.gameObject.SetActive( false );
        }
    }
}
