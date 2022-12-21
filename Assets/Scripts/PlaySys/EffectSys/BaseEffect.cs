using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseEffect
{
    protected float duration;
    public Action OnCompleted { get; set; }

    public BaseEffect( float _duration )
    {
        duration = _duration;
    }

    public abstract bool Process();

    public abstract void Restart();
}

public static class ExtentionMethod
{
    // Transform
    public static BaseEffect DoMove( this Transform _tf, Vector3 _end, float _duration )
                  => new TransformMoveEffect( _tf, _end, _duration );
    public static BaseEffect DoMoveX( this Transform _tf, float _end, float _duration )
                  => new TransformMoveEffect( _tf, new Vector3( _end, _tf.position.y, _tf.position.z ), _duration );
    public static BaseEffect DoMoveY( this Transform _tf, float _end, float _duration )
                  => new TransformMoveEffect( _tf, new Vector3( _tf.position.x, _end, _tf.position.z ), _duration );
    public static BaseEffect DoMoveZ( this Transform _tf, float _end, float _duration )
                  => new TransformMoveEffect( _tf, new Vector3( _tf.position.x, _tf.position.y, _end ), _duration );

    public static BaseEffect DoScale( this Transform _tf, Vector3 _end, float _duration )
                  => new TransformScaleEffect( _tf, _end, _duration );
    public static BaseEffect DoScaleX( this Transform _tf, float _end, float _duration )
                  => new TransformScaleEffect( _tf, new Vector3( _end, _tf.localScale.y, _tf.localScale.z ), _duration );
    public static BaseEffect DoScaleY( this Transform _tf, float _end, float _duration )
                  => new TransformScaleEffect( _tf, new Vector3( _tf.localScale.x, _end, _tf.localScale.z ), _duration );
    public static BaseEffect DoScaleZ( this Transform _tf, float _end, float _duration )
                  => new TransformScaleEffect( _tf, new Vector3( _tf.localScale.x, _tf.localScale.y, _end ), _duration );

    // RectTransform
    public static BaseEffect DoMove( this RectTransform _tf, Vector2 _end, float _duration )
                  => new RectTransformMoveEffect( _tf, _end, _duration );
    public static BaseEffect DoMoveX( this RectTransform _tf, float _end, float _duration )
                  => new RectTransformMoveEffect( _tf, new Vector2( _end, _tf.anchoredPosition.y ), _duration );
    public static BaseEffect DoMoveY( this RectTransform _tf, float _end, float _duration )
                  => new RectTransformMoveEffect( _tf, new Vector2( _tf.anchoredPosition.x, _end ), _duration );

    public static BaseEffect DoScale( this RectTransform _tf, Vector2 _end, float _duration )
                  => new RectTransformScaleEffect( _tf, _end, _duration );
    public static BaseEffect DoScaleX( this RectTransform _tf, float _end, float _duration )
                  => new RectTransformScaleEffect( _tf, new Vector2( _end, _tf.sizeDelta.y ), _duration );
    public static BaseEffect DoScaleY( this RectTransform _tf, float _end, float _duration )
                  => new RectTransformScaleEffect( _tf, new Vector2( _tf.sizeDelta.x, _end ), _duration );

    // Image
    public static BaseEffect DoFade( this Image _image, float _end, float _duration )
                  => new ImageFadeEffect( _image, _end, _duration );

    // SpriteRenderer
    public static BaseEffect DoFade( this SpriteRenderer _rdr, float _end, float _duration )
                  => new RendererFadeEffect( _rdr, _end, _duration );
    public static BaseEffect DoFade( this SpriteRenderer[] _rdr, float _start, float _end, float _duration )
                  => new RendererFadeEffectArray( _rdr, _start, _end, _duration );
    public static BaseEffect DoFade( this List<SpriteRenderer> _rdr, float _start, float _end, float _duration )
                  => new RendererFadeEffectArray( _rdr, _start, _end, _duration );
}