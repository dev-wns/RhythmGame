using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OptionController : ScrollHide
{
    protected override void Start()
    {
        base.Start();

        OptionProcess();
        while ( CurrentOption.type == OptionType.Title )
                NextMove();
    }

    protected virtual void Update() => CurrentOption?.InputProcess();

    public override void PrevMove()
    {
        base.PrevMove();
        if ( !IsLoop && IsDuplicate ) return;

        OptionProcess();

        if ( CurrentOption.type == OptionType.Title )
        {
            if ( CurrentIndex == 0 ) NextMove();
            else                     PrevMove();
        }
    }

    public override void NextMove()
    {
        base.NextMove();
        if ( !IsLoop && IsDuplicate ) return;

        OptionProcess();

        if ( CurrentOption.type == OptionType.Title )
        {
            if ( CurrentIndex == Length ) PrevMove();
            else                          NextMove();
        }
    }

    private void OptionProcess()
    {
        PreviousOption?.ActiveOutline( false );
        CurrentOption?.ActiveOutline( true );
    }
}
