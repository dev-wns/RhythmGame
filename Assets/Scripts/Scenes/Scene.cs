using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Scene : MonoBehaviour, IKeyBind
{
    private KeyActions keyAction = new KeyActions();


    public void InputLock( bool _isLock ) => keyAction.IsLock = _isLock;

    public void ChangeKeyAction( SceneAction _type ) => keyAction.ChangeAction( _type );

    public void KeyBind( SceneAction _type, StaticSceneKeyAction _action ) => keyAction.Bind( _type, _action );

    protected virtual void Awake()
    {
        Camera.main.orthographicSize = ( Screen.height / ( GlobalSetting.PPU * 2f ) ) * GlobalSetting.PPU;
        KeyBind();
    }

    protected virtual void Update() => keyAction.ActionCheck();

    public abstract void KeyBind();
}
