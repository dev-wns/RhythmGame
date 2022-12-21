using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectTransformMoveEffect : BaseRectTransformEffect
{
    public RectTransformMoveEffect( RectTransform _tf, Vector2 _end, float _duration )
                                    : base( _tf, _end, _duration )
                                    => Restart();

    public override bool Process()
    {
        elapsed            += ( offset * Time.deltaTime ) / duration;
        tf.anchoredPosition = elapsed;
        if ( end.x < tf.anchoredPosition.x || end.y < tf.anchoredPosition.y )
        {
            tf.anchoredPosition = end;
            OnCompleted?.Invoke();
            return true;
        }

        return false;
    }

    public override void Restart()
    {
        elapsed = tf.anchoredPosition;
        offset  = end - tf.anchoredPosition;
    }
}
