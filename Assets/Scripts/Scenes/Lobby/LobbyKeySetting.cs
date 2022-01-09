using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LobbyKeySetting : SceneOptionBase
{
    public GameObject keySettingCanvas;

    private void OnEnable()
    {
        currentScene.ChangeAction( SceneAction.SubOption );
    }

    public override void KeyBind()
    {
        currentScene.Bind( SceneAction.SubOption, KeyCode.UpArrow, () => PrevMove() );
        currentScene.Bind( SceneAction.SubOption, KeyCode.UpArrow, () => SoundManager.Inst.PlaySfx( SoundSfxType.Move ) );

        currentScene.Bind( SceneAction.SubOption, KeyCode.DownArrow, () => NextMove() );
        currentScene.Bind( SceneAction.SubOption, KeyCode.DownArrow, () => SoundManager.Inst.PlaySfx( SoundSfxType.Move ) );

        currentScene.Bind( SceneAction.SubOption, KeyCode.Escape, () => currentScene.ChangeAction( SceneAction.Option ) );
        currentScene.Bind( SceneAction.SubOption, KeyCode.Escape, () => keySettingCanvas.SetActive( false ) );
        currentScene.Bind( SceneAction.SubOption, KeyCode.Escape, () => SoundManager.Inst.PlaySfx( SoundSfxType.Escape ) );
    }
}
