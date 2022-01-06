using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionTitle : OptionBase
{
    protected override void Awake()
    {
        base.Awake();

        type = OptionType.Title;
    }

    public override void Process() { }
}
