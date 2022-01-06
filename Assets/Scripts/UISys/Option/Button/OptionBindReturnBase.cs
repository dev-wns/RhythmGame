using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OptionBindReturnBase : OptionBase, IOptionReturn
{
    private DelKeyAction keyReturnAction;

    protected override void Awake()
    {
        base.Awake();

        keyReturnAction += Return;
    }

    private void Start()
    {
        currentScene?.AwakeBind( actionType, KeyCode.Return );
    }

    public abstract void Return();

    public override void KeyBind()
    {
        base.KeyBind();
        currentScene?.Bind( actionType, KeyCode.Return, keyReturnAction );
    }

    public override void KeyRemove()
    {
        base.KeyRemove();
        currentScene?.Remove( actionType, KeyCode.Return, keyReturnAction );
    }
}
