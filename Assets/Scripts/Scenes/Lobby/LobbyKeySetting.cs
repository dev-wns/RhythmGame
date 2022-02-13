using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LobbyKeySetting : SceneOptionBase
{
    public GameObject keySettingCanvas;

    private void OnEnable()
    {
        CurrentScene.ChangeAction( SceneAction.SubOption );
    }

    public override void KeyBind()
    {
        CurrentScene.Bind( SceneAction.SubOption, KeyCode.UpArrow, () => PrevMove() );
        CurrentScene.Bind( SceneAction.SubOption, KeyCode.UpArrow, () => SoundManager.Inst.Play( SoundSfxType.MenuSelect ) );

        CurrentScene.Bind( SceneAction.SubOption, KeyCode.DownArrow, () => NextMove() );
        CurrentScene.Bind( SceneAction.SubOption, KeyCode.DownArrow, () => SoundManager.Inst.Play( SoundSfxType.MenuSelect ) );

        CurrentScene.Bind( SceneAction.SubOption, KeyCode.Escape, () => CurrentScene.ChangeAction( SceneAction.Option ) );
        CurrentScene.Bind( SceneAction.SubOption, KeyCode.Escape, () => keySettingCanvas.SetActive( false ) );
        CurrentScene.Bind( SceneAction.SubOption, KeyCode.Escape, () => SoundManager.Inst.Play( SoundSfxType.MenuHover ) );
    }
}
