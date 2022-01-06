using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyOption : SceneScrollOption
{
    protected override void Awake()
    {
        base.Awake();

        currentScene.AwakeBind( SceneAction.LobbyOption, KeyCode.Return );
        currentScene.AwakeBind( SceneAction.LobbyOption, KeyCode.LeftArrow );
        currentScene.AwakeBind( SceneAction.LobbyOption, KeyCode.RightArrow );
    }

    public override void KeyBind()
    {
        currentScene.Bind( SceneAction.LobbyOption, KeyCode.UpArrow,   () => PrevMove() );
        currentScene.Bind( SceneAction.LobbyOption, KeyCode.UpArrow,   () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.MOVE ) );
        currentScene.Bind( SceneAction.LobbyOption, KeyCode.DownArrow, () => NextMove() );
        currentScene.Bind( SceneAction.LobbyOption, KeyCode.DownArrow, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.MOVE ) );
        
        currentScene.Bind( SceneAction.LobbyOption, KeyCode.Escape, () => currentScene.ChangeAction( SceneAction.Lobby ) );
        currentScene.Bind( SceneAction.LobbyOption, KeyCode.Escape, () => gameObject.SetActive( false ) );
        currentScene.Bind( SceneAction.LobbyOption, KeyCode.Escape, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.ESCAPE ) );
        
        currentScene.Bind( SceneAction.LobbyOption, KeyCode.Space, () => currentScene.ChangeAction( SceneAction.Lobby ) );
        currentScene.Bind( SceneAction.LobbyOption, KeyCode.Space, () => gameObject.SetActive( false ) );
        currentScene.Bind( SceneAction.LobbyOption, KeyCode.Space, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.ESCAPE ) );
    }

    public void ShowKeySetting( GameObject _obj )
    {
        _obj.SetActive( true );
    }
}
