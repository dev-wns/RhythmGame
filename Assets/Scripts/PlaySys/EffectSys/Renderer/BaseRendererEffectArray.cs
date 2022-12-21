using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseRendererEffectArray : BaseEffect
{
    protected SpriteRenderer[] rdr;
    protected float start, end;
    protected float elapsed;
    protected float offset;
    protected bool isFadeIn;
    protected bool isDuplicateValue;

    public BaseRendererEffectArray( SpriteRenderer[] _rdr, float _start, float _end, float _duration )
                                    : base( _duration )
    {
        rdr   = _rdr;
        start = _start;
        end   = _end;
    }

    public BaseRendererEffectArray( List<SpriteRenderer> _rdr, float _start, float _end, float _duration )
                                    : base( _duration )
    {
        rdr   = _rdr.ToArray();
        start = _start;
        end   = _end;
    }
}