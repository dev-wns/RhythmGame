using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeStyleScrollSong : SceneScrollOption
{
    public OptionBase songPrefab; // sound infomation prefab

    public delegate void DelSelectSong( Song _song );
    public event DelSelectSong OnSelectSong;

    protected override void Awake()
    {
        base.Awake();

        SelectPosition( GameManager.Inst.CurrentSongIndex );
    }

    protected override void Start()
    {
        base.Start();

        OptionProcess();
    }

    protected override void CreateContents()
    {
        contents.Capacity = GameManager.Inst.Songs.Count;
        for ( int i = 0; i < GameManager.Inst.Songs.Count; i++ )
        {
            // scrollview song contents
            var song = Instantiate( songPrefab, content );
            var info = song.GetComponent<SongInfomation>();
            info.SetInfo( GameManager.Inst.Songs[i] );

            contents.Add( song );
        }
    }

    public override void PrevMove()
    {
        base.PrevMove();
        if ( !IsLoop && IsDuplicate ) return;

        OptionProcess();
    }

    public override void NextMove()
    {
        base.NextMove();
        if ( !IsLoop && IsDuplicate ) return;

        OptionProcess();
    }

    private void OptionProcess()
    {
        GameManager.Inst.SelectSong( curIndex );
        OnSelectSong( GameManager.Inst.CurrentSong );
    }


    public override void KeyBind()
    {
        currentScene.Bind( SceneAction.FreeStyle, KeyCode.UpArrow, () => PrevMove() );
        currentScene.Bind( SceneAction.FreeStyle, KeyCode.UpArrow, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.MOVE ) );

        currentScene.Bind( SceneAction.FreeStyle, KeyCode.DownArrow, () => NextMove() );
        currentScene.Bind( SceneAction.FreeStyle, KeyCode.DownArrow, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.MOVE ) );
    }
}
