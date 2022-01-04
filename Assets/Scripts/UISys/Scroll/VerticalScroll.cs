using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VerticalScroll : ScrollOption
{
    public ScrollRect scrollRect;
    private Scrollbar scrollBar;

    public int maxShowContentsCount;
    public float contentHeight;
    public float spancing;

    private int moveIndex = 0;
    private double valueOffset;

    protected override void CreateContents()
    {
        if ( contents.Count == 0 )
        {
            var viewContent = scrollRect.content;
            for ( int i = 0; i < viewContent.childCount; i++ )
            {
                contents.Add( viewContent.GetChild( i ).gameObject );
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();

        scrollBar = scrollRect.verticalScrollbar;
        scrollRect.content.sizeDelta = new Vector2( 0, ( ( contentHeight + spancing ) * contents.Count ) - spancing );
        valueOffset = 1d / Mathf.Abs( maxShowContentsCount - contents.Count );

        // position setting 
        var startRT = contents[0].transform as RectTransform;
        startRT.anchorMin = new Vector2( .5f, 1f );
        startRT.anchorMax = new Vector2( .5f, 1f );
        startRT.anchoredPosition = new Vector2( 0f, -( contentHeight * .5f ) );

        float startPos = startRT.anchoredPosition.y;
        for ( int i = 1; i < contents.Count; i++ ) 
        {
            var rt = contents[i].transform as RectTransform;

            rt.anchorMin = new Vector2( .5f, 1f );
            rt.anchorMax = new Vector2( .5f, 1f );

            rt.anchoredPosition = new Vector2( 0f, startPos - ( i * ( contentHeight + spancing ) ) );

            if ( i > maxShowContentsCount )
                 contents[i].SetActive( false );
        }
    }

    public override void PrevMove()
    {
        base.PrevMove();
        if ( !IsLoop && IsDuplicate ) return;

        if ( moveIndex == 0 )
        {
            scrollBar.value += ( float )valueOffset;
            scrollRect.verticalNormalizedPosition = scrollBar.value;
            
            if( curIndex > -1 )
            {
                contents[curIndex]?.SetActive( true );
                contents[curIndex + maxShowContentsCount]?.SetActive( false );
            }
        }
        else moveIndex -= 1;
    }

    public override void NextMove()
    {
        base.NextMove();
        if ( !IsLoop && IsDuplicate ) return;

        if ( moveIndex + 1 >= maxShowContentsCount )
        {
            scrollBar.value -= ( float )valueOffset;
            scrollRect.verticalNormalizedPosition = scrollBar.value;

            if ( curIndex > maxShowContentsCount - 1 )
            {
                contents[curIndex]?.SetActive( true );
                contents[curIndex - maxShowContentsCount]?.SetActive( false );
            }
        }
        else moveIndex += 1;
    }
}
