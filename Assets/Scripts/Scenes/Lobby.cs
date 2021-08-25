using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class Lobby : Scene
{
    private void Update()
    {
        if ( Input.GetKeyUp( KeyCode.Return ) )
        {
            SceneChanger.Inst.Change( "FreeStyle" );
        }
    }
}
