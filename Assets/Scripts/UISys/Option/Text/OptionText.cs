using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public abstract class OptionText : OptionBindArrowScroll, IOptionReturn
{
    public List<string> texts;
    public TextMeshProUGUI valueText;
    private DelKeyAction keyReturnAction;
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

    private void Start()
    {
        currentScene?.AwakeBind( actionType, KeyCode.Return );
    }

    protected abstract void CreateObject();

    public void Return()
    {
        SoundManager.Inst.PlaySfx( SoundSfxType.Return );
        if ( isReturnProcess ) Process();
    }

    public override void KeyBind()
    {
        base.KeyBind();
        currentScene?.Bind( actionType, KeyCode.Return, keyReturnAction );
    }

    public override void KeyRemove()
    {
        base.KeyRemove();
        currentScene?.Remove( actionType, KeyCode.Return, keyReturnAction );
    }

    protected void ChangeText( string _text )
    {
        if ( valueText == null ) return;

        valueText.text = _text;
    }

    public override void LeftArrow()
    {
        PrevMove();
        SoundManager.Inst.PlaySfx( SoundSfxType.Decrease );
        ChangeText( texts[curIndex] );
        if ( !isReturnProcess ) Process();
    }

    public override void RightArrow()
    {
        NextMove();
        SoundManager.Inst.PlaySfx( SoundSfxType.Increase );
        ChangeText( texts[curIndex] );
        if ( !isReturnProcess ) Process();
    }
}
