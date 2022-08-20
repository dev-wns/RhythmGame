using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OptionBindArrowBase : OptionBase, IOptionArrow
{
    private Action leftDownAction, rightDownAction;
    private Action leftHoldAction, rightHoldAction;
    private Action UpAction;

    private static float PressUpdateTime = .05f;
    private static float PressWaitTime   = .5f;
    private bool isPress = false;
    private float time;

    protected override void Awake()
    {
        base.Awake();

        leftDownAction  += LeftArrow;
        rightDownAction += RightArrow;

        leftHoldAction  += LeftPress;
        rightHoldAction += RightPress;

        UpAction += KeyUp;
    }

    public abstract void LeftArrow();
    public abstract void RightArrow();

    private void LeftPress()
    {
        time += Time.deltaTime;
        if ( time >= PressWaitTime ) 
             isPress = true;

        if ( isPress && time >= PressUpdateTime )
        {
            time = 0f;
            LeftArrow();
        }
    }

    private void RightPress()
    {
        time += Time.deltaTime;
        if ( time >= PressWaitTime )
             isPress = true;

        if ( isPress && time >= PressUpdateTime )
        {
            time = 0f;
            RightArrow();
        }
    }

    private void KeyUp()
    {
        time = 0f;
        isPress = false;
    }

    public override void KeyBind()
    {
        base.KeyBind();
        CurrentScene?.Bind( actionType, KeyType.Down, KeyCode.LeftArrow,  leftDownAction );
        CurrentScene?.Bind( actionType, KeyType.Down, KeyCode.RightArrow, rightDownAction );

        CurrentScene?.Bind( actionType, KeyType.Hold, KeyCode.LeftArrow,  leftHoldAction );
        CurrentScene?.Bind( actionType, KeyType.Hold, KeyCode.RightArrow, rightHoldAction );

        CurrentScene?.Bind( actionType, KeyType.Up, KeyCode.LeftArrow,  UpAction );
        CurrentScene?.Bind( actionType, KeyType.Up, KeyCode.RightArrow, UpAction );
    }

    public override void KeyRemove()
    {
        base.KeyRemove();
        CurrentScene?.Remove( actionType, KeyType.Down, KeyCode.LeftArrow,  leftDownAction );
        CurrentScene?.Remove( actionType, KeyType.Down, KeyCode.RightArrow, rightDownAction );

        CurrentScene?.Remove( actionType, KeyType.Hold, KeyCode.LeftArrow,  leftHoldAction );
        CurrentScene?.Remove( actionType, KeyType.Hold, KeyCode.RightArrow, rightHoldAction );

        CurrentScene?.Remove( actionType, KeyType.Up, KeyCode.LeftArrow,  UpAction );
        CurrentScene?.Remove( actionType, KeyType.Up, KeyCode.RightArrow, UpAction );

        KeyUp();
    }
}