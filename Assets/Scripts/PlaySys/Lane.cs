using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Lane : MonoBehaviour
{
    public int Key { get; private set; }
    public NoteSystem  NoteSys  { get; private set; }
    public InputSystem InputSys { get; private set; }

    public event Action<int/*Lane Key*/> OnLaneInitialize;

    public SpriteRenderer keyImage;
    public Sprite keyDefaultSprite, keyPressSprite;
    private SpriteRenderer rdr;
    private Color color;

    private readonly float StartFadeAlpha = .5f;
    private readonly float FadeDuration = .1f;
    private float fadeOffset;
    private float fadeAlpha;
    private bool isEnabled;

    private void Awake()
    {
        NoteSys  = GetComponent<NoteSystem>();
        InputSys = GetComponent<InputSystem>();
        rdr      = GetComponent<SpriteRenderer>();

        if ( ( GameSetting.CurrentVisualFlag & GameVisualFlag.LaneEffect ) != 0 )
             InputSys.OnInputEvent += LaneEffect;

        if ( ( GameSetting.CurrentVisualFlag & GameVisualFlag.ShowGearKey ) != 0 )
             InputSys.OnInputEvent += KeyEffect;

        fadeOffset = StartFadeAlpha / FadeDuration;
    }

    private void LaneEffect( bool _isEnable )
    {
        // rdr.color = _isEnable ? color : Color.clear;
        isEnabled = _isEnable;
        if ( isEnabled )
        {
            rdr.color = color;
            fadeAlpha = StartFadeAlpha;
        }
    }

    private void KeyEffect( bool _isEnable )=> keyImage.sprite = _isEnable ? keyPressSprite : keyDefaultSprite;


    private void Update()
    {
        if ( !isEnabled && fadeAlpha > 0 )
        {
            fadeAlpha -= fadeOffset * Time.deltaTime;
            Color newColor = color;
            newColor.a = fadeAlpha;
            rdr.color = newColor;
        }
    }

    public void SetLane( int _key )
    {
        Key = _key;
        UpdatePosition( _key );
        OnLaneInitialize?.Invoke( Key );

        color = _key == 1 || _key == 4 ? new Color( 0, 0, 1, StartFadeAlpha ) : new Color( 1, 0, 0, StartFadeAlpha );
    }

    public void UpdatePosition( int _key )
    {
        transform.position = new Vector3( GameSetting.NoteStartPos + ( GameSetting.NoteWidth * _key ) + ( GameSetting.NoteBlank * _key ) + GameSetting.NoteBlank, GameSetting.JudgePos, 90f );
        transform.localScale = new Vector3( GameSetting.NoteWidth, ( Screen.height * .13f ), 1f );

        keyImage.transform.position   = new Vector3( transform.position.x, keyImage.transform.position.y, keyImage.transform.position.z );
        keyImage.transform.localScale = new Vector3( transform.localScale.x + GameSetting.NoteBlank, keyImage.transform.localScale.y );
    }
}
