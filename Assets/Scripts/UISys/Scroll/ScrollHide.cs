using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollHide : ScrollOption
{
    [Header( "ScrollHide" )]
    public int numMaxActive;
    private int activeIndex; // 0 ~ maxActiveNumber

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
        int minIndex = CurrentIndex - activeIndex;
        int maxIndex = CurrentIndex + Globals.Abs( activeIndex - numMaxActive );
        for ( int i = 0; i < options.Count; i++ )
        {
            var rt = options[i].transform as RectTransform;

            rt.anchorMin = new Vector2( .5f, 1f );
            rt.anchorMax = new Vector2( .5f, 1f );

            if ( i < minIndex || i > maxIndex - 1 )
                options[i].gameObject.SetActive( false );
        }
    }

    protected override void Select( int _pos )
    {
        base.Select( _pos );

        activeIndex = _pos < numMaxActive ? _pos : numMaxActive - 1;
    }

    public override void PrevMove()
    {
        base.PrevMove();
        if ( !IsLoop && IsDuplicate ) return;

        if ( activeIndex == 0 )
        {
            options[CurrentIndex]?.gameObject.SetActive( true );
            options[CurrentIndex + numMaxActive]?.gameObject.SetActive( false );
        }
        else activeIndex -= 1;
    }

    public override void NextMove()
    {
        base.NextMove();
        if ( !IsLoop && IsDuplicate ) return;

        if ( activeIndex + 1 >= numMaxActive )
        {
            options[CurrentIndex]?.gameObject.SetActive( true );
            options[CurrentIndex - numMaxActive]?.gameObject.SetActive( false );
        }
        else activeIndex += 1;
    }
}
