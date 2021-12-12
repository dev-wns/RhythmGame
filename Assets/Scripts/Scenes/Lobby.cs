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

        SoundManager.Inst.Load( System.IO.Path.Combine( Application.streamingAssetsPath, "Osu", "1169912 VA - Arkman 6k Collection A7", "Angelic Party.mp3" ) );
        SoundManager.Inst.Play();

        //SoundManager.Inst.AllStop();

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
