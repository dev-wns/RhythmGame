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
        currentScene?.AwakeBind( actionType, KeyCode.LeftArrow );
        currentScene?.AwakeBind( actionType, KeyCode.RightArrow );
    }

    public abstract void LeftArrow();
    public abstract void RightArrow();

    public override void KeyBind()
    {
        base.KeyBind();
        currentScene?.Bind( actionType, KeyCode.LeftArrow, keyLeftAction );
        currentScene?.Bind( actionType, KeyCode.RightArrow, keyRightAction );
    }

    public override void KeyRemove()
    {
        base.KeyRemove();
        currentScene.Remove( actionType, KeyCode.LeftArrow, keyLeftAction );
        currentScene.Remove( actionType, KeyCode.RightArrow, keyRightAction );
    }
}