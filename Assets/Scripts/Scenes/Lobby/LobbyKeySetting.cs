using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LobbyKeySetting : SceneOptionBase
{
    public GameObject keySettingCanvas;

    private void OnEnable()
    {
        currentScene.ChangeAction( SceneAction.KeySetting );
    }

    public override void KeyBind()
    {
        currentScene.Bind( SceneAction.KeySetting, KeyCode.UpArrow, () => PrevMove() );
        currentScene.Bind( SceneAction.KeySetting, KeyCode.UpArrow, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.MOVE ) );

        currentScene.Bind( SceneAction.KeySetting, KeyCode.DownArrow, () => NextMove() );
        currentScene.Bind( SceneAction.KeySetting, KeyCode.DownArrow, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.MOVE ) );

        currentScene.Bind( SceneAction.KeySetting, KeyCode.Escape, () => currentScene.ChangeAction( SceneAction.LobbyOption ) );
        currentScene.Bind( SceneAction.KeySetting, KeyCode.Escape, () => keySettingCanvas.SetActive( false ) );
        currentScene.Bind( SceneAction.KeySetting, KeyCode.Escape, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.ESCAPE ) );
    }
}
