using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Judgement : MonoBehaviour
{
    private RectTransform rt;
    private void Awake()
    {
        rt = transform as RectTransform;
        rt.anchoredPosition = new Vector3( 0f, GlobalSetting.JudgeLine, -1f );
        rt.sizeDelta = new Vector3( GlobalSetting.GearWidth, GlobalSetting.JudgeHeight, 1f );
    }
}
