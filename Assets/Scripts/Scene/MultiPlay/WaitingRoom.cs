using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingRoom : Scene
{
    public GameObject multiPlay;

    protected override void Start()
    {
        base.Awake();

    }

    public void EnableMultiPlayCanvas() => EnableCanvas( ActionType.MultiPlay, multiPlay );

    public override void Connect()
    {
        
    }

    public override void Disconnect()
    {
        
    }

    public override void KeyBind()
    {
        // MultiPlay
        Bind( ActionType.MultiPlay, KeyCode.Escape, () => { DisableCanvas( ActionType.Main, multiPlay ); } );

    }
}
