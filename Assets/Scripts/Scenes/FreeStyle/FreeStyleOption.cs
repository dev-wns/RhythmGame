using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeStyleOption : SceneScrollOption
{
    private void Back()
    {
        gameObject.SetActive( false );
        SoundManager.Inst.UseLowEqualizer( false );
        CurrentScene.ChangeAction( SceneAction.Main );
        SoundManager.Inst.Play( SoundSfxType.MenuHover );
        SoundManager.Inst.FadeIn( SoundManager.Inst.GetVolume( ChannelType.BGM ) * .35f, .5f );
    }

    public override void KeyBind()
    {
        CurrentScene.Bind( SceneAction.Option, KeyCode.UpArrow, () => PrevMove() );
        CurrentScene.Bind( SceneAction.Option, KeyCode.UpArrow, () => SoundManager.Inst.Play( SoundSfxType.MenuSelect ) );

        CurrentScene.Bind( SceneAction.Option, KeyCode.DownArrow, () => NextMove() );
        CurrentScene.Bind( SceneAction.Option, KeyCode.DownArrow, () => SoundManager.Inst.Play( SoundSfxType.MenuSelect ) );
        
        CurrentScene.Bind( SceneAction.Option, KeyCode.Escape, Back );
        CurrentScene.Bind( SceneAction.Option, KeyCode.Space, Back );
    }
}
