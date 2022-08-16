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
    private Outline outline;

    protected virtual void Awake()
    {
        CurrentScene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Scene>();
        outline = GetComponent<Outline>();
        if ( outline )
        {
            //outline.effectDistance = new Vector2( 3f, -3f );
            ActiveOutline( false );
        }
    }

    public void ActiveOutline( bool _isActive )
    {
        if ( outline is null ) return;
        outline.enabled = _isActive;
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
