using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyOption : SceneScrollOption
{
    protected override void Awake()
    {
        base.Awake();

        CurrentScene.AwakeBind( SceneAction.Option, KeyCode.Return );
        CurrentScene.AwakeBind( SceneAction.Option, KeyCode.LeftArrow );
        CurrentScene.AwakeBind( SceneAction.Option, KeyCode.RightArrow );
    }

    public override void KeyBind()
    {
        CurrentScene.Bind( SceneAction.Option, KeyCode.UpArrow,   () => PrevMove() );
        CurrentScene.Bind( SceneAction.Option, KeyCode.UpArrow,   () => SoundManager.Inst.Play( SoundSfxType.Move ) );
        CurrentScene.Bind( SceneAction.Option, KeyCode.DownArrow, () => NextMove() );
        CurrentScene.Bind( SceneAction.Option, KeyCode.DownArrow, () => SoundManager.Inst.Play( SoundSfxType.Move ) );
        
        CurrentScene.Bind( SceneAction.Option, KeyCode.Escape, () => CurrentScene.ChangeAction( SceneAction.Main ) );
        CurrentScene.Bind( SceneAction.Option, KeyCode.Escape, () => gameObject.SetActive( false ) );
        CurrentScene.Bind( SceneAction.Option, KeyCode.Escape, () => SoundManager.Inst.Play( SoundSfxType.Escape ) );
        
        CurrentScene.Bind( SceneAction.Option, KeyCode.Space, () => CurrentScene.ChangeAction( SceneAction.Main ) );
        CurrentScene.Bind( SceneAction.Option, KeyCode.Space, () => gameObject.SetActive( false ) );
        CurrentScene.Bind( SceneAction.Option, KeyCode.Space, () => SoundManager.Inst.Play( SoundSfxType.Escape ) );
    }

    public void ShowKeySetting( GameObject _obj )
    {
        _obj.SetActive( true );
    }
}
