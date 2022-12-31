using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gear : MonoBehaviour
{
    public Transform judge;
    public Transform panel;
    public Transform sideLeft, sideRight;

    private void Start()
    {
        UpdatePosition();

        if ( GameSetting.PanelOpacity <= .01f ? false : true )
             panel.GetComponent<SpriteRenderer>().color = new Color( 0f, 0f, 0f, GameSetting.PanelOpacity * .01f );
        else
             panel.gameObject.SetActive( false );

        panel.localScale   = new Vector3( GameSetting.GearWidth, Screen.height );
        sideLeft.position  = new Vector3( GameSetting.GearStartPos, 0f );
        sideRight.position = new Vector3( -GameSetting.GearStartPos, 0f );
    }

    private void UpdatePosition()
    {
        judge.position   = new Vector2( 0f, GameSetting.JudgePos );
        judge.localScale = new Vector3( GameSetting.GearWidth, judge.localScale.y );
    }
}
