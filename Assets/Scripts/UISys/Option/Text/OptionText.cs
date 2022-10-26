using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public abstract class OptionText : OptionBindArrowScroll, IOptionReturn
{
    [Header("Text")]
    public List<string> texts;
    public TextMeshProUGUI valueText;
    private Action keyReturnAction;
    public bool isReturnProcess = true;

    protected override void Awake()
    {
        base.Awake();
        keyReturnAction += Return;

        type = OptionType.Text;
        IsLoop = true;

        CreateObject();
        maxCount = texts.Count;
    }

    protected abstract void CreateObject();

    public void Return()
    {
        if ( isReturnProcess )
        {
            SoundManager.Inst.Play( SoundSfxType.MenuClick );
            Process();
        }
    }

    public override void KeyBind()
    {
        base.KeyBind();
        CurrentScene?.Bind( actionType, KeyCode.Return, keyReturnAction );
    }

    public override void KeyRemove()
    {
        base.KeyRemove();
        CurrentScene?.Remove( actionType, KeyCode.Return, keyReturnAction );
    }

    protected void ChangeText( string _text )
    {
        if ( valueText == null ) return;

        valueText.text = _text;
    }

    public override void LeftArrow()
    {
        PrevMove();
        SoundManager.Inst.Play( SoundSfxType.Slider );
        ChangeText( texts[curIndex] );
        if ( !isReturnProcess ) Process();
    }

    public override void RightArrow()
    {
        NextMove();
        SoundManager.Inst.Play( SoundSfxType.Slider );
        ChangeText( texts[curIndex] );
        if ( !isReturnProcess ) Process();
    }
}
