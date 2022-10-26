using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OptionButtonEvent : OptionButton
{
    public UnityEvent ButtonEvent;

    protected override void Awake()
    {
        base.Awake();

        ButtonEvent ??= new UnityEvent();
    }

    public override void Process()
    {
        ButtonEvent?.Invoke();
    }
}
