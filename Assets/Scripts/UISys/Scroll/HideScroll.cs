using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideScroll : ScrollOption
{
    protected int minShowIndex { get; private set; }
    protected int maxShowIndex { get; private set; }
    
    public int maxShowContentsCount = 3;
    public int numExtraEnable = 2;

    protected override void Awake()
    {
        base.Awake();

        minShowIndex = Mathf.FloorToInt( maxShowContentsCount * .5f );
        maxShowIndex = contents.Count - minIndex - 1;
    }

    private void Start()
    {
        // 화면에 그려지는 객체만 활성화
        for ( int i = 0; i < contents.Count; i++ )
        {
            if ( curIndex - minShowIndex <= i && curIndex + minShowIndex >= i )
                 contents[i].SetActive( true );
            else contents[i].SetActive( false );
        }
    }

    public override void PrevMove()
    {
        base.PrevMove();
        if ( !IsLoop && IsDuplicate ) return;

        if ( minShowIndex <= curIndex )
        {
            contents[curIndex - minShowIndex].SetActive( true );
        }

        if ( maxShowIndex > curIndex + numExtraEnable )
        {
            contents[curIndex + minShowIndex + numExtraEnable + 1].SetActive( false );
        }
    }

    public override void NextMove()
    {
        base.NextMove();
        if ( !IsLoop && IsDuplicate ) return;

        if ( maxShowIndex >= curIndex )
        {
            contents[minShowIndex + curIndex].SetActive( true );
        }

        if ( minShowIndex < curIndex - numExtraEnable )
        {
            contents[curIndex - minShowIndex - numExtraEnable - 1].SetActive( false );
        }
    }
}
