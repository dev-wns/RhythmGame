namespace WNS.Time.Control
{
    using UnityEngine;

    public class TransformScale : BaseTransform
    {
        public TransformScale( Transform _tf, Vector3 _end, float _duration )
                               : base( _tf, _end, _duration )
                               => Restart();

        public override bool Process()
        {
            elapsed += ( offset * Time.deltaTime ) / duration;
            tf.localScale = elapsed;

            NextSequence?.Process();
            if ( end.x < tf.localScale.x || end.y < tf.localScale.y || end.z < tf.localScale.z )
            {
                tf.localScale = end;
                OnCompleted?.Invoke();
                return true;
            }

            return false;
        }

        public override void Restart()
        {
            elapsed = tf.localScale;
            offset = end - tf.localScale;
        }
    }
}