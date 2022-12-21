using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseRendererEffect : BaseEffect
{
    protected SpriteRenderer rdr;
    protected float end;
    protected float elapsed;
    protected float offset;
    protected bool isFadeIn;
    protected bool isDuplicateValue;

    public BaseRendererEffect( SpriteRenderer _rdr, float _end, float _duration )
                               : base( _duration )
    {
        rdr = _rdr;
        end = _end;
    }
}