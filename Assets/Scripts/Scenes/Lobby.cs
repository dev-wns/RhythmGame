using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lobby : Scene
{
    private void Update()
    {
        if ( Input.GetKeyUp( KeyCode.Return ) )
        {
            Change( SceneType.FreeStyle );
        }
    }
}
