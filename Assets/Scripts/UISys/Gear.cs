using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gear : MonoBehaviour
{
    public GameObject background, left, right;
    private RectTransform tfBackground, tfLeft, tfRight;

    private void Awake()
    {
        tfBackground = background.GetComponent<Transform>() as RectTransform;
        tfLeft  = left.GetComponent<Transform>()  as RectTransform;
        tfRight = right.GetComponent<Transform>() as RectTransform;

        tfLeft.anchoredPosition       = new Vector3(  GlobalSetting.GearStartPos, 94f, 0f );
        tfRight.anchoredPosition      = new Vector3( -GlobalSetting.GearStartPos, 94f, 0f );
        tfBackground.anchoredPosition = new Vector3( 0f, 0f, 0f );
        tfBackground.sizeDelta        = new Vector3( GlobalSetting.GearWidth, Screen.height, 0f );
    }
}
