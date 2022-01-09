using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Scene : SceneKeyAction, IKeyBind
{
    protected virtual void Awake()
    {
        Camera.main.orthographicSize = ( Screen.height / ( GlobalSetting.PPU * 2f ) ) * GlobalSetting.PPU;
        
        KeyBind();
        ChangeAction( SceneAction.Main );
    }

    protected virtual void Update() => ActionCheck();

    public abstract void KeyBind();
}
