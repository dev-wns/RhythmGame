using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gear : MonoBehaviour
{
    public GameObject background, left, right;
    private RectTransform rtBackground, rtLeft, rtRight;
    private Transform tfBackground, tfLeft, tfRight;

    private void Awake()
    {
        tfBackground = background.GetComponent<Transform>();
        tfLeft = left.GetComponent<Transform>();
        tfRight = right.GetComponent<Transform>();

        tfLeft.localPosition = new Vector3( GlobalSetting.GearStartPos, .94f, .0f );
        tfRight.localPosition = new Vector3( -GlobalSetting.GearStartPos, .94f, 0f );
        tfBackground.localPosition = new Vector3( 0f, -( Screen.height * .5f * .01f ), 100f );
        tfBackground.localScale = new Vector3( GlobalSetting.GearWidth, Screen.height * .01f, 0f );
    }
}
