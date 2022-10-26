using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyExit : SceneOptionBase
{
    public override void KeyBind()
    {
        CurrentScene.Bind( ActionType.Exit, KeyCode.Return, () => CurrentOption.Process() );
        CurrentScene.Bind( ActionType.Exit, KeyCode.Return, () => SoundManager.Inst.Play( SoundSfxType.MenuHover ) );

        CurrentScene.Bind( ActionType.Exit, KeyCode.LeftArrow,  () => PrevMove() );
        CurrentScene.Bind( ActionType.Exit, KeyCode.LeftArrow,  () => SoundManager.Inst.Play( SoundSfxType.MenuSelect ) );

        CurrentScene.Bind( ActionType.Exit, KeyCode.RightArrow, () => NextMove() );
        CurrentScene.Bind( ActionType.Exit, KeyCode.RightArrow, () => SoundManager.Inst.Play( SoundSfxType.MenuSelect ) );

        CurrentScene.Bind( ActionType.Exit, KeyCode.Escape, Cancel );
    }

    public void Cancel()
    {
        CurrentScene.ChangeAction( ActionType.Main );
        gameObject.SetActive( false );
        SoundManager.Inst.Play( SoundSfxType.MenuHover );
    }

    public void Exit()
    {
        Application.Quit();
    }
}
