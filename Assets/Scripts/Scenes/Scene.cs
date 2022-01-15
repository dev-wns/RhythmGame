using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Scene : SceneKeyAction, IKeyBind
{
    protected virtual void Awake()
    {
        Camera.main.orthographicSize = ( Screen.height / ( GameSetting.PPU * 2f ) ) * GameSetting.PPU;
        
        KeyBind();

        SceneChanger.CurrentScene = this;
        ChangeAction( SceneAction.Main );
    }

    protected virtual void Update() => ActionCheck();

    public abstract void KeyBind();
}
