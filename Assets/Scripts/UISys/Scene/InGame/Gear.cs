using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gear : MonoBehaviour
{
    public Transform hint;
    public Transform panel;
    public Transform sideLeft, sideRight;
    public GameObject keyUI;

    private void Awake()
    {
        // hint.position   = new Vector2( 0f, GameSetting.HintPos );
        UpdateHintPosition();

        if ( GameSetting.PanelOpacity <= .01f ? false : true )
             panel.GetComponent<SpriteRenderer>().color = new Color( 0f, 0f, 0f, GameSetting.PanelOpacity * .01f );
        else
             panel.gameObject.SetActive( false );

        panel.localScale   = new Vector3( GameSetting.GearWidth, Screen.height );
        sideLeft.position  = new Vector3( GameSetting.GearStartPos, 0f );
        sideRight.position = new Vector3( -GameSetting.GearStartPos, 0f );
    }

    private void UpdateHintPosition()
    {
        keyUI.SetActive( GameSetting.CurrentVisualFlag.HasFlag( GameVisualFlag.ShowGearKey ) );
        hint.position = GameSetting.CurrentVisualFlag.HasFlag( GameVisualFlag.ShowGearKey ) ? new Vector2( 0f, GameSetting.HintPos ) : new Vector2( 0f, GameSetting.JudgePos + GameSetting.HintOffset );
    }
}
