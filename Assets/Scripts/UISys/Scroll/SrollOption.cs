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

    protected OptionBase CurrentOption { get; private set; }
    protected OptionBase PreviousOption { get; private set; }

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

        CurrentOption = options[_pos];
    }

    public override void PrevMove()
    {
        base.PrevMove();

        CurrentOption = options[CurrentIndex];
        PreviousOption = options[PreviousIndex];
    }

    public override void NextMove()
    {
        base.NextMove();

        CurrentOption = options[CurrentIndex];
        PreviousOption = options[PreviousIndex];
    }
}
