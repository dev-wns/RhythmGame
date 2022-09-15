using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FreeStyleOption : SceneScrollOption
{
    private CanvasGroup group;

    protected override void Awake()
    {
        base.Awake();

        if ( !TryGetComponent<CanvasGroup>( out group ) )
             Debug.LogError( $"FreeStyle Option CanvasGroup is null" );

    }

    private void Back()
    {
        DOTween.Clear();
        group.alpha = 1f;
        DOTween.To( () => 1f, x => group.alpha = x, 0f, GlobalConst.OptionFadeDuration ).OnComplete( () => gameObject.SetActive( false ) );

        //SoundManager.Inst.UseLowEqualizer( false );
        CurrentScene.ChangeAction( SceneAction.Main );
        SoundManager.Inst.Play( SoundSfxType.MenuHover );
        SoundManager.Inst.FadeIn( SoundManager.Inst.GetVolume( ChannelType.BGM ) * .5f, .5f );
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
