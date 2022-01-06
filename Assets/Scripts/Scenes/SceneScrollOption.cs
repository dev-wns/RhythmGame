using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class SceneScrollOption : ScrollHide, IKeyBind
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
        while ( currentOption.type == OptionType.Title )
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

        if ( currentOption.type == OptionType.Title )
        {
            if ( currentIndex == 0 ) NextMove();
            else                     PrevMove();
        }
    }

    public override void NextMove()
    {
        base.NextMove();
        if ( !IsLoop && IsDuplicate ) return;

        OptionProcess();

        if ( currentOption.type == OptionType.Title )
        {
            if ( currentIndex == Length ) PrevMove();
            else                          NextMove();
        }
    }

    private void OptionProcess()
    {
        if ( previousOption != null )
        {
            previousOption.KeyRemove();
            previousOption.ActiveOutline( false );
        }

        if ( currentOption != null )
        {
            currentOption.KeyBind();
            currentOption.ActiveOutline( true );
        }
    }

    public abstract void KeyBind();
}
