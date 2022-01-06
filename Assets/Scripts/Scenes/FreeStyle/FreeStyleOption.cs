using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeStyleOption : SceneScrollOption
{
    public override void KeyBind()
    {
        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.UpArrow, () => PrevMove() );
        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.UpArrow, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.MOVE ) );

        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.DownArrow, () => NextMove() );
        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.DownArrow, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.MOVE ) );

        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.Escape, () => gameObject.SetActive( false ) );
        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.Escape, () => SoundManager.Inst.UseLowEqualizer( false ) );
        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.Escape, () => currentScene.ChangeAction( SceneAction.FreeStyle ) );
        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.Escape, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.ESCAPE ) );

        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.Space, () => gameObject.SetActive( false ) );
        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.Space, () => SoundManager.Inst.UseLowEqualizer( false ) );
        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.Space, () => currentScene.ChangeAction( SceneAction.FreeStyle ) );
        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.Space, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.ESCAPE ) );
    }
}
