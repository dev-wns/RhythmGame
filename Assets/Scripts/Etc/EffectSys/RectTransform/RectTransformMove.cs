namespace WNS.Time.Control
{
    using UnityEngine;

    public class RectTransformMove : BaseRectTransform
    {
        public RectTransformMove( RectTransform _tf, Vector2 _end, float _duration )
                                  : base( _tf, _end, _duration )
                                  => Restart();

        public override bool Process()
        {
            elapsed += ( offset * Time.deltaTime ) / duration;
            tf.anchoredPosition = elapsed;

            NextSequence?.Process();
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
            offset = end - tf.anchoredPosition;
        }
    }
}