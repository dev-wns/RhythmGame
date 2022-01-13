using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitScene : MonoBehaviour
{
    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        SceneChanger.Inst.InitSceneChange();
    }
}
