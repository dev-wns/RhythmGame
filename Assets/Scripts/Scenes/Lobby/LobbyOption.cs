using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LobbyOption : OptionController
{
    private CanvasGroup optionGroup;

    protected override void Awake()
    {
        base.Awake();

        if ( !TryGetComponent( out optionGroup ) )
             Debug.LogError( $"Lobby Option CanvasGroup is null" );
    }

    private void Back()
    {
        //CurrentScene.ChangeAction( ActionType.Main );
        DOTween.Clear();
        optionGroup.alpha = 1f;
        DOTween.To( () => 1f, x => optionGroup.alpha = x, 0f, Global.Const.OptionFadeDuration ).OnComplete( () => gameObject.SetActive( false ) );
        SoundManager.Inst.Play( SoundSfxType.MenuHover );
    }

    //public override void KeyBind()
    //{
        //CurrentScene.Bind( ActionType.SystemOption, KeyCode.UpArrow,   () => PrevMove() );
        //CurrentScene.Bind( ActionType.SystemOption, KeyCode.UpArrow,   () => SoundManager.Inst.Play( SoundSfxType.MenuSelect ) );

        //CurrentScene.Bind( ActionType.SystemOption, KeyCode.DownArrow, () => NextMove() );
        //CurrentScene.Bind( ActionType.SystemOption, KeyCode.DownArrow, () => SoundManager.Inst.Play( SoundSfxType.MenuSelect ) );

        //CurrentScene.Bind( ActionType.SystemOption, KeyCode.Escape, Back );
    //}

    public void ShowKeySetting( GameObject _obj )
    {
        _obj.SetActive( true );
    }
}
