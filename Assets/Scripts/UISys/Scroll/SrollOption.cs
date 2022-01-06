using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollOption : ScrollBase
{
    [Header( "ScrollOption" )]
    public List<OptionBase> contents;

    protected ScrollRect scrollRect;
    protected Scrollbar scrollBar;
    protected RectTransform content;

    protected OptionBase curOption { get; private set; }
    protected OptionBase prevOption { get; private set; }

    protected virtual void Awake()
    {
        scrollRect ??= GetComponent<ScrollRect>();
        if ( scrollRect )
        {
            scrollBar = scrollRect.verticalScrollbar;
            content = scrollRect.content;
        }

        CreateContents();
    }

    protected virtual void Start()
    {
        maxCount = contents.Count;
        SelectPosition( 0 );
    }

    protected virtual void CreateContents() 
    {
    }

    protected override void SelectPosition( int _pos )
    {
        base.SelectPosition( _pos );

        if ( maxCount <= 0 ) return;

        curOption = contents[_pos];
    }

    public override void PrevMove()
    {
        base.PrevMove();

        curOption = contents[curIndex];
        prevOption = contents[prevIndex];
    }

    public override void NextMove()
    {
        base.NextMove();

        curOption = contents[curIndex];
        prevOption = contents[prevIndex];
    }
}
