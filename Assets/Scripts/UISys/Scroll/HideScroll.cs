using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideScroll : ScrollBase
{
    protected int minIndex { get; private set; }
    protected int maxIndex { get; private set; }
    
    public int maxShowContentsCount = 3;
    public int numExtraEnable = 2;

    protected override void Awake()
    {
        base.Awake();

        minIndex = Mathf.FloorToInt( maxShowContentsCount * .5f );
        maxIndex = contents.Count - minIndex - 1;
    }

    private void Start()
    {
        // 화면에 그려지는 객체만 활성화
        for ( int i = 0; i < contents.Count; i++ )
        {
            if ( curIndex - minIndex <= i && curIndex + minIndex >= i )
                 contents[i].SetActive( true );
            else contents[i].SetActive( false );
        }
    }

    public override void PrevMove()
    {
        base.PrevMove();
        if ( !IsLoop && IsDuplicate ) return;

        if ( minIndex <= curIndex )
        {
            contents[curIndex - minIndex].SetActive( true );
        }

        if ( maxIndex > curIndex + numExtraEnable )
        {
            contents[minIndex + curIndex + numExtraEnable + 1].SetActive( false );
        }
    }

    public override void NextMove()
    {
        base.NextMove();
        if ( !IsLoop && IsDuplicate ) return;

        if ( maxIndex >= curIndex )
        {
            contents[minIndex + curIndex].SetActive( true );
        }

        if ( minIndex < curIndex - numExtraEnable )
        {
            contents[curIndex - minIndex - numExtraEnable - 1].SetActive( false );
        }
    }
}
