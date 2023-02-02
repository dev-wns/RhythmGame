using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[ExecuteAlways]
[RequireComponent( typeof( RectTransform ) )]
public abstract class CustomLayoutGroup : MonoBehaviour, ILayoutController
{
    public float spacing;
    [Serializable]
    protected struct RectOffset
    {
        public float left, right, top, bottom;

        public RectOffset( float _left, float _right, float _top, float _bottom )
        {
            left   = _left;
            right  = _right;
            top    = _top;
            bottom = _bottom;
        }
    }
    [SerializeField]
    protected RectOffset padding;
    [SerializeField] 
    protected TextAnchor childAlignment = TextAnchor.MiddleCenter;
    protected Vector2 anchor;

    [SerializeField]
    protected List<RectTransform> rectChildren = new List<RectTransform>();
    public bool ShouldIncludeDisabledObject { get; private set; }

    public virtual void Initialize( bool _shouldIncludeDisabledObject = false )
    {
        ShouldIncludeDisabledObject = _shouldIncludeDisabledObject;
        SetAlignment();
        rectChildren?.Clear();
        var rt = transform as RectTransform;
        for ( int i = 0; i < rt.childCount; i++ )
        {
            var child = rt.GetChild( i ).transform as RectTransform;
            child.anchorMin = anchor;
            child.anchorMax = anchor;

            rectChildren.Add( child );
        }
    }

    protected virtual void Start()
    {
        if ( Application.isPlaying )
        {
            Initialize();
            SetLayoutHorizontal();
            SetLayoutVertical();
        }
    }

#if UNITY_EDITOR
    protected virtual void Update()
    {
        // Editor Update
        if ( !Application.isPlaying )
        {
            rectChildren?.Clear();

            SetAlignment();
            Initialize();
            SetLayoutHorizontal();
            SetLayoutVertical();
        }
    }
#endif

    public abstract void SetLayoutHorizontal();
    public abstract void SetLayoutVertical();

    private void SetAlignment()
    {
        switch ( childAlignment )
        {
            case TextAnchor.UpperLeft:
            anchor = new Vector2( 0f, 1f );
            break;

            case TextAnchor.UpperCenter:
            anchor = new Vector2( .5f, 1f );
            break;

            case TextAnchor.UpperRight:
            anchor = new Vector2( 1f, 1f );
            break;

            case TextAnchor.MiddleLeft:
            anchor = new Vector2( 0f, .5f );
            break;

            case TextAnchor.MiddleCenter:
            anchor = new Vector2( .5f, .5f );
            break;

            case TextAnchor.MiddleRight:
            anchor = new Vector2( 1f, .5f );
            break;

            case TextAnchor.LowerLeft:
            anchor = new Vector2( 0f, 0f );
            break;

            case TextAnchor.LowerCenter:
            anchor = new Vector2( .5f, 0f );
            break;

            case TextAnchor.LowerRight:
            anchor = new Vector2( 1f, 0f );
            break;
        }
    }
}
