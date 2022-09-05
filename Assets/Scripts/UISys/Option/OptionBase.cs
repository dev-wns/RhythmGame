using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class OptionBase : MonoBehaviour, IOption, IKeyControl
{
    [Header( "Type" )]
    public SceneAction actionType = SceneAction.Option;
    public OptionType type { get; protected set; }

    protected Scene CurrentScene { get; private set; }
    private GameObject outline;

    protected virtual void Awake()
    {
        CurrentScene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Scene>();

        var outlineTf = transform.Find( "Outline" );
        if ( outlineTf )
        {
            outline = outlineTf.gameObject;
            ActiveOutline( false );
        }
    }

    public void ActiveOutline( bool _isActive )
    {
        if ( outline is null ) return;
        outline.SetActive( _isActive );
    }

    public abstract void Process();

    public virtual void KeyBind()
    {
        CurrentScene ??= GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Scene>();
    }

    public virtual void KeyRemove()
    {
        CurrentScene ??= GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Scene>();
    }
}
