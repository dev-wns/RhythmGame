using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class SceneOptionBase : ScrollOption, IKeyBind
{
    protected Scene CurrentScene { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        GameObject scene = GameObject.FindGameObjectWithTag( "Scene" );
        CurrentScene = scene.GetComponent<Scene>();
        KeyBind();
    }

    private void Start()
    {
        OptionProcess();
        while ( CurrentOption.type == OptionType.Title )
        {
            base.NextMove();
            OptionProcess();
        }
    }

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
        if ( PreviousOption != null )
        {
            PreviousOption.KeyRemove();
            PreviousOption.ActiveOutline( false );
        }

        if ( CurrentOption != null )
        {
            CurrentOption.KeyBind();
            CurrentOption.ActiveOutline( true );
        }
    }

    public abstract void KeyBind();
}
