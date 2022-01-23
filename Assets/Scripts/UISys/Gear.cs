using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

public class Gear : MonoBehaviour
{
    public GameObject background, left, right;

    private void Awake()
    {
        var rtLeft = left.transform as RectTransform;
        rtLeft.anchoredPosition = new Vector3(  GameSetting.GearStartPos, -Screen.height * .5f, 0f );

        var rtRight = right.transform as RectTransform;
        rtRight.anchoredPosition = new Vector3( -GameSetting.GearStartPos, -Screen.height * .5f, 0f );

        var rtBackground = background.transform as RectTransform;
        rtBackground.anchoredPosition = Vector3.zero;
        rtBackground.sizeDelta        = new Vector3( GameSetting.GearWidth, Screen.height, 0f );
    }
}
