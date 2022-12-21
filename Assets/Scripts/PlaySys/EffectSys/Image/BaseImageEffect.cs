using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseImageEffect : BaseEffect
{
    protected Image image;
    protected float end;
    protected float elapsed;
    protected float offset;
    protected bool  isFadeIn;

    public BaseImageEffect( Image _image, float _end, float _duration )
                            : base( _duration )
    {
        image = _image;
        end   = _end;
    }
}
