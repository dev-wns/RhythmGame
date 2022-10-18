using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LobbyKeySetting : SceneOptionBase
{
    public GameObject keySettingCanvas;
    private List<KeySettingOption> tracks = new List<KeySettingOption>();
    private KeyCode curKeyCode;

    protected override void CreateOptions()
    {
        foreach ( var option in options )
        {
            tracks.Add( option.GetComponent<KeySettingOption>() );
        }
    }

    private void OnEnable()
    {
        CurrentScene.ChangeAction( ActionType.SubOption );
    }

    private void Process( KeyCode _key )
    {
        if ( KeySetting.Inst.IsAvailableKey( _key ) )
        {
            for ( int i = 0; i < tracks.Count; i++ )
            {
                if ( KeySetting.Inst.Keys[( GameKeyAction )i] == _key )
                     tracks[i].Change( KeyCode.None );
            }

            SoundManager.Inst.Play( SoundSfxType.MenuSelect );
            tracks[CurrentIndex].Change( _key );

            NextMove();
        }
    }

    public override void KeyBind()
    {
        CurrentScene.Bind( ActionType.SubOption, KeyCode.LeftArrow, () => PrevMove() );
        CurrentScene.Bind( ActionType.SubOption, KeyCode.LeftArrow, () => SoundManager.Inst.Play( SoundSfxType.MenuSelect ) );

        CurrentScene.Bind( ActionType.SubOption, KeyCode.RightArrow, () => NextMove() );
        CurrentScene.Bind( ActionType.SubOption, KeyCode.RightArrow, () => SoundManager.Inst.Play( SoundSfxType.MenuSelect ) );

        CurrentScene.Bind( ActionType.SubOption, KeyCode.Escape, () => CurrentScene.ChangeAction( ActionType.Option ) );
        CurrentScene.Bind( ActionType.SubOption, KeyCode.Escape, () => keySettingCanvas.SetActive( false ) );
        CurrentScene.Bind( ActionType.SubOption, KeyCode.Escape, () => SoundManager.Inst.Play( SoundSfxType.MenuHover ) );
    }

    private void Update()
    {
        foreach ( var key in KeySetting.Inst.AvailableKeys )
        {
            var keyCode = key.Key;
            if ( Input.GetKeyDown( keyCode ) )
            {
                curKeyCode = keyCode;
                Process( curKeyCode );
                break;
            }
        }
    }
}
