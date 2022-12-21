using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformMoveEffect : BaseTransformEffect
{
    public TransformMoveEffect( Transform _tf, Vector3 _end, float _duration )
                                : base( _tf, _end, _duration )
                                => Restart();

    public override bool Process()
    {
        elapsed    += ( offset * Time.deltaTime ) / duration;
        tf.position = elapsed;
        if ( end.x < tf.position.x || end.y < tf.position.y || end.z < tf.position.z )
        {
            tf.position = end;
            OnCompleted?.Invoke();
            return true;
        }

        return false;
    }

    public override void Restart()
    {
        elapsed = tf.position;
        offset  = end - tf.position;
    }
}