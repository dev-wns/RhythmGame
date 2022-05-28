using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyExit : SceneOptionBase
{
    public override void KeyBind()
    {
        CurrentScene.Bind( SceneAction.Exit, KeyCode.Return, () => CurrentOption.Process() );
        CurrentScene.Bind( SceneAction.Exit, KeyCode.Return, () => SoundManager.Inst.Play( SoundSfxType.MenuHover ) );

        CurrentScene.Bind( SceneAction.Exit, KeyCode.LeftArrow,  () => PrevMove() );
        CurrentScene.Bind( SceneAction.Exit, KeyCode.LeftArrow,  () => SoundManager.Inst.Play( SoundSfxType.MenuSelect ) );

        CurrentScene.Bind( SceneAction.Exit, KeyCode.RightArrow, () => NextMove() );
        CurrentScene.Bind( SceneAction.Exit, KeyCode.RightArrow, () => SoundManager.Inst.Play( SoundSfxType.MenuSelect ) );

        CurrentScene.Bind( SceneAction.Exit, KeyCode.Escape, Cancel );
    }

    public void Cancel()
    {
        CurrentScene.ChangeAction( SceneAction.Main );
        gameObject.SetActive( false );
        SoundManager.Inst.Play( SoundSfxType.MenuHover );
    }

    public void Exit()
    {
        Application.Quit();
    }
}
