using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gear : MonoBehaviour
{
    public RectTransform hint;
    public Image panel;

    private void Awake()
    {
        hint.position = new Vector2( 0f, GameSetting.HintPos );
        //GameSetting.HintPos  = hint.position.y;
        //GameSetting.JudgePos = hint.position.y;

        if ( GameSetting.PanelOpacity <= .01f ? false : true )
        {
            panel.color = new Color( 0f, 0f, 0f, GameSetting.PanelOpacity * .01f );
        }
        else
        {
            panel.gameObject.SetActive( false );
        }
    }
}
