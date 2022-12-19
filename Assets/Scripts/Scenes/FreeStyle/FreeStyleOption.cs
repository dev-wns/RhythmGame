using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FreeStyleOption : OptionController
{
    //private CanvasGroup group;

    protected override void Awake()
    {
        base.Awake();

        //if ( !TryGetComponent( out group ) )
        //    Debug.LogError( $"FreeStyle Option CanvasGroup is null" );
    }

    private void Back()
    {
        DOTween.Clear();
        //group.alpha = 1f;
        //DOTween.To( () => 1f, x => group.alpha = x, 0f, Global.Const.OptionFadeDuration ).OnComplete( () => gameObject.SetActive( false ) );

        //CurrentScene.ChangeAction( ActionType.Main );
        SoundManager.Inst.Play( SoundSfxType.MenuHover );
        // SoundManager.Inst.FadeIn( SoundManager.Inst.Volume * .5f, .5f );
        SoundManager.Inst.FadeVolume( SoundManager.Inst.GetVolume( ChannelType.BGM ), SoundManager.Inst.Volume, .5f );
    }

    private void ScrollDown()
    {
        PrevMove();
        SoundManager.Inst.Play( SoundSfxType.MenuSelect );
    }

    private void ScrollUp()
    {
        NextMove();
        SoundManager.Inst.Play( SoundSfxType.MenuSelect );
    }

    //public override void KeyBind()
    //{
    //    CurrentScene.Bind( ActionType.GameOption, KeyCode.UpArrow,   ScrollDown );
    //    CurrentScene.Bind( ActionType.GameOption, KeyCode.DownArrow, ScrollUp );
    //    CurrentScene.Bind( ActionType.GameOption, KeyCode.Escape,    Back );
    //    CurrentScene.Bind( ActionType.GameOption, KeyCode.Space,     Back );
    //}
}
