using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lobby : Scene
{
    private void ChangeMusic()
    {
        //var datas = GameManager.Datas;
        //NowPlaying.Inst.Initialized( datas[Random.Range( 0, datas.Count )] );

        //NowPlaying.Inst.Play( true );
    }

    protected override void Awake()
    {
        base.Awake();

        //ChangeMusic();
    }

    private void Update()
    {
        //if ( NowPlaying.Playback > NowPlaying.EndTime )
        //    ChangeMusic();

        if ( Input.GetKeyDown( KeyCode.Return ) )
            Change( SceneType.FreeStyle );
    }
}
