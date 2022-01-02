using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitScene : MonoBehaviour
{
    List<Song> songs = new List<Song>();
    private void Start()
    {
        SceneChanger.Inst.InitSceneChange();
    }
}
