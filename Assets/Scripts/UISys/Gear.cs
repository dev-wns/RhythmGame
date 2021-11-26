using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gear : MonoBehaviour
{
    public GameObject background, left, right;
    private RectTransform rtBackground, rtLeft, rtRight;


    private void Awake()
    {
        rtBackground = background.GetComponent<RectTransform>();
        rtLeft = left.GetComponent<RectTransform>();
        rtRight = right.GetComponent<RectTransform>();

        rtLeft.anchoredPosition = new Vector2( GlobalSetting.GearStartPos, 64f );
        rtRight.anchoredPosition = new Vector2( -GlobalSetting.GearStartPos, 94f );
        rtBackground.sizeDelta = new Vector2( GlobalSetting.GearWidth, 1080f );
    }
}
