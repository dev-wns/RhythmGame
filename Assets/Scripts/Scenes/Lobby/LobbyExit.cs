using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyExit : SceneOptionBase
{
    public override void KeyBind()
    {
        currentScene.Bind( SceneAction.Exit, KeyCode.LeftArrow,  () => PrevMove() );
        currentScene.Bind( SceneAction.Exit, KeyCode.LeftArrow,  () => SoundManager.Inst.PlaySfx( SoundSfxType.Move ) );

        currentScene.Bind( SceneAction.Exit, KeyCode.RightArrow, () => NextMove() );
        currentScene.Bind( SceneAction.Exit, KeyCode.RightArrow, () => SoundManager.Inst.PlaySfx( SoundSfxType.Move ) );

        currentScene.Bind( SceneAction.Exit, KeyCode.Escape, () => currentScene.ChangeAction( SceneAction.Main ) );
        currentScene.Bind( SceneAction.Exit, KeyCode.Escape, () => gameObject.SetActive( false ) );
        currentScene.Bind( SceneAction.Exit, KeyCode.Escape, () => SoundManager.Inst.PlaySfx( SoundSfxType.Escape ) );
    }

    public void Cancel()
    {
        currentScene.ChangeAction( SceneAction.Main );
        gameObject.SetActive( false );
    }

    public void Exit()
    {
        Application.Quit();
    }
}
