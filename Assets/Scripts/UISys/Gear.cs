using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

[ExecuteAlways]
public class Gear : MonoBehaviour
{
    public RectTransform background, left, right;

    private void Awake()
    {
        left.anchoredPosition  = new Vector3(  GameSetting.GearStartPos, -Screen.height * .5f, 0f );
        right.anchoredPosition = new Vector3( -GameSetting.GearStartPos, -Screen.height * .5f, 0f );

        background.anchoredPosition = Vector3.zero;
        background.sizeDelta        = new Vector3( GameSetting.GearWidth, Screen.height, 0f );
    }

    private void Update()
    {
        if ( !Application.isPlaying )
        {
            left.anchoredPosition = new Vector3( GameSetting.GearStartPos, -Screen.height * .5f, 0f );
            right.anchoredPosition = new Vector3( -GameSetting.GearStartPos, -Screen.height * .5f, 0f );

            background.anchoredPosition = Vector3.zero;
            background.sizeDelta = new Vector3( GameSetting.GearWidth, Screen.height, 0f );
        }
    }
}
