using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageFadeEffect : BaseImageEffect
{
    public ImageFadeEffect( Image _image, float _end, float _duration )
                            : base( _image, _end, _duration )
                            => Restart();

    public override bool Process()
    {
        elapsed += ( offset * Time.deltaTime ) / duration;
        Color newColor = image.color;
        newColor.a     = elapsed;
        image.color    = newColor;

        if ( isFadeIn ? elapsed > end :
                        elapsed < end )
        {
            newColor.a  = end;
            image.color = newColor;
            OnCompleted?.Invoke();
            return true;
        }
        return false;
    }

    public override void Restart()
    {
        float alpha = image.color.a;
        offset      = end - alpha;
        isFadeIn    = alpha < end;
        elapsed     = alpha;
    }
}
