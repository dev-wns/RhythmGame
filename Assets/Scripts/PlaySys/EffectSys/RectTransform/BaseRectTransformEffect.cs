using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseRectTransformEffect : BaseEffect
{
    protected RectTransform tf;
    protected Vector2 end;
    protected Vector2 elapsed;
    protected Vector2 offset;

    public BaseRectTransformEffect( RectTransform _tf, Vector2 _end, float _duration )
                                    : base( _duration )
    {
        tf = _tf;
        end = _end;
    }
}
