using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public abstract class OptionText : OptionBindArrowScroll
{
    public List<string> texts;
    public TextMeshProUGUI valueText;

    protected override void Awake()
    {
        base.Awake();
        
        type = OptionType.Text;
        IsLoop = true;

        CreateObject();
        maxCount = texts.Count;
    }

    protected abstract void CreateObject();

    protected void ChangeText( string _text )
    {
        if ( valueText == null ) return;

        valueText.text = _text;
    }

    public override void LeftArrow()
    {
        PrevMove();
        SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.DECREASE );
        ChangeText( texts[curIndex] );
        Process();
    }

    public override void RightArrow()
    {
        NextMove();
        SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.INCREASE );
        ChangeText( texts[curIndex] );
        Process();
    }
}
