namespace WNS.Time.Control
{
    using UnityEngine;

    public class RectTransformScale : BaseRectTransform
    {
        public RectTransformScale( RectTransform _tf, Vector2 _end, float _duration )
                                   : base( _tf, _end, _duration )
                                   => Restart();

        public override bool Process()
        {
            elapsed += ( offset * Time.deltaTime ) / duration;
            tf.sizeDelta = elapsed;

            NextSequence?.Process();
            if ( end.x < tf.sizeDelta.x || end.y < tf.sizeDelta.y )
            {
                tf.sizeDelta = end;
                OnCompleted?.Invoke();
                return true;
            }
            return false;
        }

        public override void Restart()
        {
            elapsed = tf.sizeDelta;
            offset = end - tf.sizeDelta;
        }
    }
}