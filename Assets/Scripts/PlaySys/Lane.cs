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


    private void Awake()
    {
        NoteSys  = GetComponent<NoteSystem>();
        InputSys = GetComponent<InputSystem>();
        rdr      = GetComponent<SpriteRenderer>();

        if ( ( GameSetting.CurrentVisualFlag & GameVisualFlag.LaneEffect ) != 0 )
        {
            InputSys.OnInputEvent += PlayEffect;
        }

        transform.localScale = new Vector3( GameSetting.NoteWidth, ( Screen.height * .13f ), 1f );
        color = rdr.color;
        color.a = .75f;
        rdr.color = Color.clear;
    }

    private void PlayEffect( bool _isEnable )
    {
        // sprite renderer Enable로 활성화 시키는것보다
        // color 값 변경하는게 6배정도 빠름.

        // laneEffect.enabled = _isEnable;
        rdr.color       = _isEnable ? color : Color.clear;
        keyImage.sprite = _isEnable ? keyPressSprite : keyDefaultSprite;
    }

    public void SetLane( int _key )
    {
        Key = _key;
        transform.position = new Vector3( GameSetting.NoteStartPos + ( GameSetting.NoteWidth * Key ) +
                                        ( GameSetting.NoteBlank * Key ) + GameSetting.NoteBlank,
                                          GameSetting.HintPos, 90f );
        OnLaneInitialize?.Invoke( Key );
    }
}
