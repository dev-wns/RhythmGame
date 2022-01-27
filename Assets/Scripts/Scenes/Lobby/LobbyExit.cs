using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyExit : SceneOptionBase
{
    public override void KeyBind()
    {
        CurrentScene.Bind( SceneAction.Exit, KeyCode.LeftArrow,  () => PrevMove() );
        CurrentScene.Bind( SceneAction.Exit, KeyCode.LeftArrow,  () => SoundManager.Inst.PlaySfx( SoundSfxType.Move ) );

        CurrentScene.Bind( SceneAction.Exit, KeyCode.RightArrow, () => NextMove() );
        CurrentScene.Bind( SceneAction.Exit, KeyCode.RightArrow, () => SoundManager.Inst.PlaySfx( SoundSfxType.Move ) );

        CurrentScene.Bind( SceneAction.Exit, KeyCode.Escape, () => CurrentScene.ChangeAction( SceneAction.Main ) );
        CurrentScene.Bind( SceneAction.Exit, KeyCode.Escape, () => gameObject.SetActive( false ) );
        CurrentScene.Bind( SceneAction.Exit, KeyCode.Escape, () => SoundManager.Inst.PlaySfx( SoundSfxType.Escape ) );
    }

    public void Cancel()
    {
        CurrentScene.ChangeAction( SceneAction.Main );
        gameObject.SetActive( false );
    }

    public void Exit()
    {
        Application.Quit();
    }
}
