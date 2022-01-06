using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollHide : ScrollOption
{
    [Header( "ScrollHide" )]
    public int maxActiveNumber;
    private int activeIndex = 0; // 0 ~ maxActiveNumber
    private double valueOffset;

    protected override void CreateContents()
    {
        if ( contents.Count == 0 )
        {
            for ( int i = 0; i < content.childCount; i++ )
            {
                contents.Add( content.GetChild( i ).GetComponent<OptionBase>() );
            }
        }
    }

    protected override void Start()
    {
        base.Start();

        valueOffset = 1d / Mathf.Abs( maxActiveNumber - contents.Count );

        int minIndex = curIndex - activeIndex;
        int maxIndex = curIndex + Mathf.Abs( activeIndex - maxActiveNumber );
        for ( int i = 0; i < contents.Count; i++ )
        {
            var rt = contents[i].transform as RectTransform;

            rt.anchorMin = new Vector2( .5f, 1f );
            rt.anchorMax = new Vector2( .5f, 1f );

            if ( i < minIndex || i > maxIndex - 1 )
                contents[i].gameObject.SetActive( false );
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

            if ( curIndex > -1 )
            {
                contents[curIndex].gameObject?.SetActive( true );
                contents[curIndex + maxActiveNumber].gameObject?.SetActive( false );
            }
        }
        else activeIndex -= 1;
    }

    public override void NextMove()
    {
        base.NextMove();
        if ( !IsLoop && IsDuplicate ) return;

        if ( activeIndex + 1 >= maxActiveNumber )
        {
            scrollBar.value -= ( float )valueOffset;
            scrollRect.verticalNormalizedPosition = scrollBar.value;

            if ( curIndex > maxActiveNumber - 1 )
            {
                contents[curIndex].gameObject?.SetActive( true );
                contents[curIndex - maxActiveNumber].gameObject?.SetActive( false );
            }
        }
        else activeIndex += 1;
    }
}
