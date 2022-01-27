using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeStyleOption : SceneScrollOption
{
    public override void KeyBind()
    {
        CurrentScene.Bind( SceneAction.Option, KeyCode.UpArrow, () => PrevMove() );
        CurrentScene.Bind( SceneAction.Option, KeyCode.UpArrow, () => SoundManager.Inst.PlaySfx( SoundSfxType.Move ) );
        
        CurrentScene.Bind( SceneAction.Option, KeyCode.DownArrow, () => NextMove() );
        CurrentScene.Bind( SceneAction.Option, KeyCode.DownArrow, () => SoundManager.Inst.PlaySfx( SoundSfxType.Move ) );
        
        CurrentScene.Bind( SceneAction.Option, KeyCode.Escape, () => gameObject.SetActive( false ) );
        CurrentScene.Bind( SceneAction.Option, KeyCode.Escape, () => SoundManager.Inst.UseLowEqualizer( false ) );
        CurrentScene.Bind( SceneAction.Option, KeyCode.Escape, () => CurrentScene.ChangeAction( SceneAction.Main ) );
        CurrentScene.Bind( SceneAction.Option, KeyCode.Escape, () => SoundManager.Inst.PlaySfx( SoundSfxType.Escape ) );
        
        CurrentScene.Bind( SceneAction.Option, KeyCode.Space, () => gameObject.SetActive( false ) );
        CurrentScene.Bind( SceneAction.Option, KeyCode.Space, () => SoundManager.Inst.UseLowEqualizer( false ) );
        CurrentScene.Bind( SceneAction.Option, KeyCode.Space, () => CurrentScene.ChangeAction( SceneAction.Main ) );
        CurrentScene.Bind( SceneAction.Option, KeyCode.Space, () => SoundManager.Inst.PlaySfx( SoundSfxType.Escape ) );
    }
}
