using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OptionBase : MonoBehaviour, IOptionA
{
    public OptionType type { get; protected set; }
    public SceneAction actionType;

    protected Scene currentScene { get; private set; }

    protected virtual void Awake()
    {
        currentScene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Scene>();
    }

    public abstract void Process();
}
