using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class OptionBase : MonoBehaviour, IOption
{
    public OptionType type { get; protected set; }
    public SceneAction actionType;

    protected Scene currentScene { get; private set; }
    private Outline outline;

    protected virtual void Awake()
    {
        currentScene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Scene>();
        outline = GetComponent<Outline>();

        outline.effectDistance = new Vector2( 5f, -5f );
        outline.effectColor    = Color.yellow;
        ActiveOutline( false );
    }

    public void ActiveOutline( bool _isActive ) => outline.enabled = _isActive;

    public abstract void Process();
}
