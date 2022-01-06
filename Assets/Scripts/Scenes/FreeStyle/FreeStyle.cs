using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FreeStyle : Scene
{
    //public VerticalScrollSound scrollSound;
    public GameObject optionCanvas;

    #region unity callbacks
    protected override void Awake()
    {
        base.Awake();

        ChangeAction( SceneAction.FreeStyle );
    }

    public void Start()
    {
        //ChangePreview();
    }

    private void ChangePreview()
    {
        //if ( scrollSound.IsDuplicate ) return;

        Song curSong = new Song();//= GameManager.Inst.CurrentSound;

        Globals.Timer.Start();
        {
            SoundManager.Inst.LoadBgm( curSong.audioPath, SOUND_LOAD_TYPE.STREAM );
            SoundManager.Inst.PlayBgm();
        }

        Debug.Log( $"Sound Load {Globals.Timer.End()} ms" );

        // 중간부터 재생
        int time = curSong.previewTime;
        if ( time <= 0 ) SoundManager.Inst.SetPosition( ( uint )( SoundManager.Inst.Length / 3f ) );
        else SoundManager.Inst.SetPosition( ( uint )time );
    }
    #endregion



    public override void KeyBind()
    {
        //Bind( SceneAction.FreeStyle, KeyCode.UpArrow, () => scrollSound.PrevMove() );
        //Bind( SceneAction.FreeStyle, KeyCode.UpArrow, () => ChangePreview() );

        //Bind( SceneAction.FreeStyle, KeyCode.DownArrow, () => scrollSound.NextMove() );
        //Bind( SceneAction.FreeStyle, KeyCode.DownArrow, () => ChangePreview() );

        //Bind( SceneAction.FreeStyle, KeyCode.Return, () => SceneChanger.Inst.LoadScene( SCENE_TYPE.GAME ) );

        Bind( SceneAction.FreeStyle, KeyCode.Space, () => optionCanvas.SetActive( true ) );
        Bind( SceneAction.FreeStyle, KeyCode.Space, () => SoundManager.Inst.UseLowEqualizer( true ) );
        Bind( SceneAction.FreeStyle, KeyCode.Space, () => ChangeAction( SceneAction.FreeStyleOption ) );
              
        Bind( SceneAction.FreeStyle, KeyCode.Escape, () => SceneChanger.Inst.LoadScene( SCENE_TYPE.LOBBY ) );
    }
}