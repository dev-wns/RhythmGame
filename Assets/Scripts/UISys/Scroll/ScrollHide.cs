using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollHide : ScrollOption
{
    [Header( "ScrollHide" )]
    public int numMaxActive;
    private int activeIndex = 0; // 0 ~ maxActiveNumber
    private double valueOffset;

    protected override void CreateOptions()
    {
        if ( options.Count == 0 )
        {
            for ( int i = 0; i < content.childCount; i++ )
            {
                options.Add( content.GetChild( i ).GetComponent<OptionBase>() );
            }
        }
    }

    protected virtual void Start()
    {
        valueOffset = 1d / Mathf.Abs( numMaxActive - options.Count );

        int minIndex = currentIndex - activeIndex;
        int maxIndex = currentIndex + Mathf.Abs( activeIndex - numMaxActive );
        for ( int i = 0; i < options.Count; i++ )
        {
            var rt = options[i].transform as RectTransform;

            rt.anchorMin = new Vector2( .5f, 1f );
            rt.anchorMax = new Vector2( .5f, 1f );

            if ( i < minIndex || i > maxIndex - 1 )
                options[i].gameObject.SetActive( false );
        }
    }

    public override void PrevMove()
    {
        base.PrevMove();
        if ( !IsLoop && IsDuplicate ) return;

        if ( activeIndex == 0 )
        {
            scrollBar.value += ( float )valueOffset;
            scrollRect.verticalNormalizedPosition = scrollBar.value;

            if ( currentIndex > -1 )
            {
                options[currentIndex].gameObject?.SetActive( true );
                options[currentIndex + numMaxActive].gameObject?.SetActive( false );
            }
        }
        else activeIndex -= 1;
    }

    public override void NextMove()
    {
        base.NextMove();
        if ( !IsLoop && IsDuplicate ) return;

        if ( activeIndex + 1 >= numMaxActive )
        {
            scrollBar.value -= ( float )valueOffset;
            scrollRect.verticalNormalizedPosition = scrollBar.value;

            if ( currentIndex > numMaxActive - 1 )
            {
                options[currentIndex].gameObject?.SetActive( true );
                options[currentIndex - numMaxActive].gameObject?.SetActive( false );
            }
        }
        else activeIndex += 1;
    }
}
