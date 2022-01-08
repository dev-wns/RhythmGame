using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollOption : ScrollBase
{
    [Header( "ScrollOption" )]
    public List<OptionBase> options;

    private ScrollRect scrollRect { get; set; }
    protected RectTransform content;

    protected OptionBase currentOption { get; private set; }
    protected OptionBase previousOption { get; private set; }

    protected virtual void Awake()
    {
        scrollRect ??= GetComponent<ScrollRect>();
        if ( scrollRect ) content = scrollRect.content;

        CreateOptions();
        Length = options.Count;
        Select( 0 );
    }

    protected virtual void CreateOptions() { }

    protected override void Select( int _pos )
    {
        base.Select( _pos );

        if ( Length <= 0 ) return;

        currentOption = options[_pos];
    }

    public override void PrevMove()
    {
        base.PrevMove();

        currentOption  = options[currentIndex];
        previousOption = options[previousIndex];
    }

    public override void NextMove()
    {
        base.NextMove();

        currentOption  = options[currentIndex];
        previousOption = options[previousIndex];
    }
}
