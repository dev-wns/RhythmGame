using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class SceneScrollOption : HideScroll, IKeyBind
{
    protected Scene currentScene;

    protected override void Awake()
    {
        base.Awake();

        GameObject scene = GameObject.FindGameObjectWithTag( "Scene" );
        currentScene = scene.GetComponent<Scene>();
        KeyBind();
    }

    protected override void Start()
    {
        base.Start();

        OptionProcess();
        while ( curOption.type == OptionType.Title )
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

        if ( curOption.type == OptionType.Title )
        {
            if ( curIndex == 0 ) NextMove();
            else                 PrevMove();
        }
    }

    public override void NextMove()
    {
        base.NextMove();
        if ( !IsLoop && IsDuplicate ) return;

        OptionProcess();

        if ( curOption.type == OptionType.Title )
        {
            if ( curIndex == maxCount ) PrevMove();
            else                        NextMove();
        }
    }

    private void OptionProcess()
    {
        if ( prevOption != null )
        {
            prevOption.KeyRemove();
            prevOption.ActiveOutline( false );
        }

        if ( curOption != null )
        {
            curOption.KeyBind();
            curOption.ActiveOutline( true );
        }
    }

    public abstract void KeyBind();
}
