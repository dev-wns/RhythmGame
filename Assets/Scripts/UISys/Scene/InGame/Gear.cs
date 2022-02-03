using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gear : MonoBehaviour
{
    public RectTransform bgRT, leftRT, rightRT;
    private Image background;

    private void Awake()
    {
        background = bgRT.GetComponent<Image>();

        leftRT.anchoredPosition  = new Vector3(  GameSetting.GearStartPos, -Screen.height * .5f, 0f );
        rightRT.anchoredPosition = new Vector3( -GameSetting.GearStartPos, -Screen.height * .5f, 0f );

        bgRT = background.rectTransform;
        bgRT.anchoredPosition = Vector3.zero;
        bgRT.sizeDelta        = new Vector3( GameSetting.GearWidth, Screen.height, 0f );

        bool isEnabled = GameSetting.PanelOpacity <= .1f ? false : true;
        if ( isEnabled )
        {
            background.color = new Color( 0f, 0f, 0f, GameSetting.PanelOpacity * .01f );
        }
        else
        {
            background.gameObject.SetActive( false );
        }
    }
}
