using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingRoom : Scene
{
    public GameObject loginCanvas;

    protected override void Awake()
    {
        base.Awake();

        loginCanvas?.SetActive( NowPlaying.UserInfo is null );
    }

    public void EnableMultiPlayCanvas() => EnableCanvas( ActionType.MultiPlay, loginCanvas );

    public override void Connect()
    {
        
    }

    public override void Disconnect()
    {
        
    }

    public override void KeyBind()
    {
        // MultiPlay
        Bind( ActionType.MultiPlay, KeyCode.Escape, () => { DisableCanvas( ActionType.Main, loginCanvas ); } );

    }
}
