using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyOption : SceneScrollOption
{
    public override void KeyBind()
    {
        CurrentScene.Bind( SceneAction.Option, KeyCode.UpArrow,   () => PrevMove() );
        CurrentScene.Bind( SceneAction.Option, KeyCode.UpArrow,   () => SoundManager.Inst.Play( SoundSfxType.MenuSelect ) );
        CurrentScene.Bind( SceneAction.Option, KeyCode.DownArrow, () => NextMove() );
        CurrentScene.Bind( SceneAction.Option, KeyCode.DownArrow, () => SoundManager.Inst.Play( SoundSfxType.MenuSelect ) );

        CurrentScene.Bind( SceneAction.Option, KeyCode.Escape, () => CurrentScene.ChangeAction( SceneAction.Main ) );
        CurrentScene.Bind( SceneAction.Option, KeyCode.Escape, () => gameObject.SetActive( false ) );
        CurrentScene.Bind( SceneAction.Option, KeyCode.Escape, () => SoundManager.Inst.Play( SoundSfxType.MenuHover ) );
        
        CurrentScene.Bind( SceneAction.Option, KeyCode.Space, () => CurrentScene.ChangeAction( SceneAction.Main ) );
        CurrentScene.Bind( SceneAction.Option, KeyCode.Space, () => gameObject.SetActive( false ) );
        CurrentScene.Bind( SceneAction.Option, KeyCode.Space, () => SoundManager.Inst.Play( SoundSfxType.MenuHover ) );
    }

    public void ShowKeySetting( GameObject _obj )
    {
        _obj.SetActive( true );
    }
}
