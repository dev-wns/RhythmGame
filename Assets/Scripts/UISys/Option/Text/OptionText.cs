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

        valueText.text = _text.Replace( "_", " " ); ;
    }

    public override void LeftArrow()
    {
        PrevMove();
        ChangeText( texts[curIndex] );
        Process();
    }

    public override void RightArrow()
    {
        NextMove();
        ChangeText( texts[curIndex] );
        Process();
    }
}
