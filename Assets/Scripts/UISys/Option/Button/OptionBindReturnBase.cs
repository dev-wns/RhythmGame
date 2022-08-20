using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OptionBindReturnBase : OptionBase, IOptionReturn
{
    private Action keyReturnAction;

    protected override void Awake()
    {
        base.Awake();

        keyReturnAction += Return;
    }

    private void Start()
    {
        //CurrentScene?.AwakeBind( actionType, KeyCode.Return );
    }

    public abstract void Return();

    public override void KeyBind()
    {
        base.KeyBind();
        CurrentScene?.Bind( actionType, KeyCode.Return, keyReturnAction );
    }

    public override void KeyRemove()
    {
        base.KeyRemove();
        CurrentScene?.Remove( actionType, KeyCode.Return, keyReturnAction );
    }
}
