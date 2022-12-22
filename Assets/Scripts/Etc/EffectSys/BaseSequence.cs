namespace WNS.Time.Control
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public abstract class BaseSequence
    {
        protected float duration;
        public Action OnCompleted { get; set; }
        public BaseSequence NextSequence { get; set; }

        public BaseSequence( float _duration )
        {
            duration = _duration;
        }

        public abstract bool Process();

        public abstract void Restart();
    }


    public static class ExtentionMethod
    {
        // Transform
        public static BaseSequence DoMove( this Transform _tf, Vector3 _end, float _duration )
                      => new TransformMove( _tf, _end, _duration );
        public static BaseSequence DoMoveX( this Transform _tf, float _end, float _duration )
                      => new TransformMove( _tf, new Vector3( _end, _tf.position.y, _tf.position.z ), _duration );
        public static BaseSequence DoMoveY( this Transform _tf, float _end, float _duration )
                      => new TransformMove( _tf, new Vector3( _tf.position.x, _end, _tf.position.z ), _duration );
        public static BaseSequence DoMoveZ( this Transform _tf, float _end, float _duration )
                      => new TransformMove( _tf, new Vector3( _tf.position.x, _tf.position.y, _end ), _duration );

        public static BaseSequence DoScale( this Transform _tf, Vector3 _end, float _duration )
                      => new TransformScale( _tf, _end, _duration );
        public static BaseSequence DoScaleX( this Transform _tf, float _end, float _duration )
                      => new TransformScale( _tf, new Vector3( _end, _tf.localScale.y, _tf.localScale.z ), _duration );
        public static BaseSequence DoScaleY( this Transform _tf, float _end, float _duration )
                      => new TransformScale( _tf, new Vector3( _tf.localScale.x, _end, _tf.localScale.z ), _duration );
        public static BaseSequence DoScaleZ( this Transform _tf, float _end, float _duration )
                      => new TransformScale( _tf, new Vector3( _tf.localScale.x, _tf.localScale.y, _end ), _duration );

        // RectTransform
        public static BaseSequence DoMove( this RectTransform _tf, Vector2 _end, float _duration )
                      => new RectTransformMove( _tf, _end, _duration );
        public static BaseSequence DoMoveX( this RectTransform _tf, float _end, float _duration )
                      => new RectTransformMove( _tf, new Vector2( _end, _tf.anchoredPosition.y ), _duration );
        public static BaseSequence DoMoveY( this RectTransform _tf, float _end, float _duration )
                      => new RectTransformMove( _tf, new Vector2( _tf.anchoredPosition.x, _end ), _duration );

        public static BaseSequence DoScale( this RectTransform _tf, Vector2 _end, float _duration )
                      => new RectTransformScale( _tf, _end, _duration );
        public static BaseSequence DoScaleX( this RectTransform _tf, float _end, float _duration )
                      => new RectTransformScale( _tf, new Vector2( _end, _tf.sizeDelta.y ), _duration );
        public static BaseSequence DoScaleY( this RectTransform _tf, float _end, float _duration )
                      => new RectTransformScale( _tf, new Vector2( _tf.sizeDelta.x, _end ), _duration );

        // Image
        public static BaseSequence DoFade( this Image _image, float _end, float _duration )
                      => new ImageFade( _image, _end, _duration );

        // SpriteRenderer
        public static BaseSequence DoFade( this SpriteRenderer _rdr, float _end, float _duration )
                      => new RendererFade( _rdr, _end, _duration );
        public static BaseSequence DoFade( this SpriteRenderer[] _rdr, float _start, float _end, float _duration )
                      => new RendererFadeArray( _rdr, _start, _end, _duration );
        public static BaseSequence DoFade( this List<SpriteRenderer> _rdr, float _start, float _end, float _duration )
                      => new RendererFadeArray( _rdr, _start, _end, _duration );
    }
}