using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectInterval : BaseEffect
{
    protected float time;

    public EffectInterval( float _t ) : base( _t ) { }

    public override bool Process()
    {
        time += Time.deltaTime;
        return duration < time;
    }

    public override void Restart()
    {
        time = 0f;
    }
}