using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeStyleOption : SceneScrollOption
{
    public override void KeyBind()
    {
        currentScene.Bind( SceneAction.Option, KeyCode.UpArrow, () => PrevMove() );
        currentScene.Bind( SceneAction.Option, KeyCode.UpArrow, () => SoundManager.Inst.PlaySfx( SoundSfxType.Move ) );

        currentScene.Bind( SceneAction.Option, KeyCode.DownArrow, () => NextMove() );
        currentScene.Bind( SceneAction.Option, KeyCode.DownArrow, () => SoundManager.Inst.PlaySfx( SoundSfxType.Move ) );

        currentScene.Bind( SceneAction.Option, KeyCode.Escape, () => gameObject.SetActive( false ) );
        currentScene.Bind( SceneAction.Option, KeyCode.Escape, () => SoundManager.Inst.UseLowEqualizer( false ) );
        currentScene.Bind( SceneAction.Option, KeyCode.Escape, () => currentScene.ChangeAction( SceneAction.Main ) );
        currentScene.Bind( SceneAction.Option, KeyCode.Escape, () => SoundManager.Inst.PlaySfx( SoundSfxType.Escape ) );

        currentScene.Bind( SceneAction.Option, KeyCode.Space, () => gameObject.SetActive( false ) );
        currentScene.Bind( SceneAction.Option, KeyCode.Space, () => SoundManager.Inst.UseLowEqualizer( false ) );
        currentScene.Bind( SceneAction.Option, KeyCode.Space, () => currentScene.ChangeAction( SceneAction.Main ) );
        currentScene.Bind( SceneAction.Option, KeyCode.Space, () => SoundManager.Inst.PlaySfx( SoundSfxType.Escape ) );
    }
}
