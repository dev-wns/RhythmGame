namespace WNS.Time.Control
{
    using UnityEngine;

    public class TransformMove : BaseTransform
    {
        public TransformMove( Transform _tf, Vector3 _end, float _duration )
                              : base( _tf, _end, _duration )
                              => Restart();

        public override bool Process()
        {
            elapsed += ( offset * Time.deltaTime ) / duration;
            tf.position = elapsed;

            NextSequence?.Process();
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
            offset = end - tf.position;
        }
    }
}