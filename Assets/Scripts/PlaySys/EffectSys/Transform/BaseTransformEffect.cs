using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseTransformEffect : BaseEffect
{
    protected Transform tf;
    protected Vector3 end;
    protected Vector3 elapsed;
    protected Vector3 offset;

    public BaseTransformEffect( Transform _tf, Vector3 _end, float _duration )
                                : base( _duration )
    {
        tf      = _tf;
        end     = _end;
    }
}