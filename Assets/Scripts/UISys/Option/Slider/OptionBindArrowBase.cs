using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OptionBindArrowBase : OptionBase, IOptionArrow
{
    private DelKeyAction keyLeftAction, keyRightAction;

    protected override void Awake()
    {
        base.Awake();

        keyLeftAction += LeftArrow;
        keyRightAction += RightArrow;
    }

    private void Start()
    {
        CurrentScene?.AwakeBind( actionType, KeyCode.LeftArrow );
        CurrentScene?.AwakeBind( actionType, KeyCode.RightArrow );
    }

    public abstract void LeftArrow();
    public abstract void RightArrow();

    public override void KeyBind()
    {
        base.KeyBind();
        CurrentScene?.Bind( actionType, KeyCode.LeftArrow, keyLeftAction );
        CurrentScene?.Bind( actionType, KeyCode.RightArrow, keyRightAction );
    }

    public override void KeyRemove()
    {
        base.KeyRemove();
        CurrentScene.Remove( actionType, KeyCode.LeftArrow, keyLeftAction );
        CurrentScene.Remove( actionType, KeyCode.RightArrow, keyRightAction );
    }
}