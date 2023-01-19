using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollHide : ScrollOption
{
    [Header( "ScrollHide" )]
    public bool isHideOption = true;
    public int numMaxActive;
    private int activeIndex; // 0 ~ maxActiveNumber

    protected virtual void Start()
    {
        if ( !isHideOption ) return;

        if ( numMaxActive == 0 )
             numMaxActive = options.Count;

        int minIndex = CurrentIndex - activeIndex;
        int maxIndex = CurrentIndex + Global.Math.Abs( activeIndex - numMaxActive );
        for ( int i = 0; i < options.Count; i++ )
        {
            if ( i < minIndex || i > maxIndex - 1 )
                 options[i].gameObject.SetActive( false );
        }
    }

    protected override void Select( int _pos )
    {
        base.Select( _pos );
        if ( !isHideOption ) return;

        activeIndex = _pos < numMaxActive ? _pos : numMaxActive - 1;
    }

    public override void PrevMove()
    {
        base.PrevMove();
        if ( !isHideOption || ( !IsLoop && IsDuplicate ) )
             return;

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
        if ( !isHideOption || ( !IsLoop && IsDuplicate ) ) 
             return;

        if ( activeIndex + 1 >= numMaxActive )
        {
            options[CurrentIndex]?.gameObject.SetActive( true );
            options[CurrentIndex - numMaxActive]?.gameObject.SetActive( false );
        }
        else activeIndex += 1;
    }
}
