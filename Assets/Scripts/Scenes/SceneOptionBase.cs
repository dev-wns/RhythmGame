using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class SceneOptionBase : ScrollOption, IKeyBind
{
    protected Scene currentScene;

    protected override void Awake()
    {
        base.Awake();

        GameObject scene = GameObject.FindGameObjectWithTag( "Scene" );
        currentScene = scene.GetComponent<Scene>();
        KeyBind();
    }

    public override void PrevMove()
    {
        base.PrevMove();
        if ( !IsLoop && IsDuplicate ) return;

        OptionProcess();
    }

    public override void NextMove()
    {
        base.NextMove();
        if ( !IsLoop && IsDuplicate ) return;

        OptionProcess();
    }

    private void OptionProcess()
    {
        IOption previous = prevOption.GetComponent<IOption>();
        if ( previous != null )
        {
            var keyControl = previous as IKeyControl;
            keyControl.KeyRemove();

            var outline = previous as OptionBase;
            outline.ActiveOutline( false );
        }

        IOption current = curOption.GetComponent<IOption>();
        if ( current != null )
        {
            var keyControl = current as IKeyControl;
            keyControl.KeyBind();

            var outline = current as OptionBase;
            outline.ActiveOutline( true );
        }
    }

    public abstract void KeyBind();
}
