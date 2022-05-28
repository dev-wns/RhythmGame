using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyMainScroll : SceneOptionBase
{
    public GameObject optionCanvas, exitCanvas;

    public void GotoFreeStyle()
    {
        if ( NowPlaying.Inst.IsParseSongs )
        {
            CurrentScene.LoadScene( SceneType.FreeStyle );
            SoundManager.Inst.Play( SoundSfxType.MainClick );
        }
    }

    public void ShowOptionCanvas()
    {
        optionCanvas.SetActive( true );
        CurrentScene.ChangeAction( SceneAction.Option );
        SoundManager.Inst.Play( SoundSfxType.MenuClick );
    }

    public void ShowExitCanvas()
    {
        exitCanvas.SetActive( true );
        CurrentScene.ChangeAction( SceneAction.Exit );
        SoundManager.Inst.Play( SoundSfxType.MenuClick );
    }

    public override void KeyBind()
    {
        CurrentScene.Bind( SceneAction.Main, KeyCode.Return, () => CurrentOption.Process() );

        CurrentScene.Bind( SceneAction.Main, KeyCode.UpArrow, () => SoundManager.Inst.Play( SoundSfxType.MainSelect ) );
        CurrentScene.Bind( SceneAction.Main, KeyCode.UpArrow, () => PrevMove() );
        
        CurrentScene.Bind( SceneAction.Main, KeyCode.DownArrow, () => SoundManager.Inst.Play( SoundSfxType.MainSelect ) );
        CurrentScene.Bind( SceneAction.Main, KeyCode.DownArrow, () => NextMove() );
    }
}
