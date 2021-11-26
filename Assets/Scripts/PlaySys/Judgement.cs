using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Judgement : MonoBehaviour
{
    private RectTransform rt;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2( 0f, GlobalSetting.JudgeLine );
        rt.sizeDelta = new Vector2( GlobalSetting.GearWidth, 5f );
    }
}
