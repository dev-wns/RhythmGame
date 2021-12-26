using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OptionBase : VerticalScrollBase, IKeyBind
{
    protected Scene currentScene;
    public GameObject subOptionCanvas;

    protected override void Awake()
    {
        base.Awake();
        currentScene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Scene>();
        KeyBind();
    }

    public abstract void Process();

    public abstract void KeyBind();
}
