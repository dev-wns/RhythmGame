using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiPlay : Scene
{
    public GameObject loginCanvas;
    public PlayerInfo playerInfo;

    protected override void Awake()
    {
        base.Awake();

        if ( NowPlaying.UserInfo is null )
             loginCanvas?.SetActive( true );
        else
        {
            playerInfo.UpdateUserInfo( NowPlaying.UserInfo.Value );
            loginCanvas?.SetActive( false );
        }
    }

    public void EnableMultiPlayCanvas() => EnableCanvas( ActionType.MultiPlay, loginCanvas );

    public void MoveToRoom()
    {
        AudioManager.Inst.Play( SFX.MainClick );
        LoadScene( SceneType.Room );
    }

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
