using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OptionBindReturnBase : OptionBase, IOptionReturn, IKeyControl
{
    private DelKeyAction keyReturnAction;

    protected override void Awake()
    {
        base.Awake();

        keyReturnAction += Return;
        currentScene?.AwakeBind( actionType, KeyCode.Return );
    }

    public abstract void Return();

    public void KeyBind()
    {
        currentScene.Bind( actionType, KeyCode.Return, keyReturnAction );
    }

    public void KeyRemove()
    {
        currentScene.Remove( actionType, KeyCode.Return, keyReturnAction );
    }
}
