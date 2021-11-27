using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Judgement : MonoBehaviour
{
    private void Awake()
    {
        transform.position = new Vector3( 0f, GlobalSetting.JudgeLine );
        transform.localScale = new Vector3( GlobalSetting.GearWidth, GlobalSetting.JudgeHeight, 1f );
    }
}
