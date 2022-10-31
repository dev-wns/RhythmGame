using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class FreeStylePreview : MonoBehaviour
{
    public FreeStyleMainScroll scroller;
    protected RawImage previewImage;
    protected RectTransform tf { get; private set; }
    protected Vector2 sizeCache { get; private set; }

    protected virtual void Awake()
    {
        tf = transform as RectTransform;
        sizeCache = tf.sizeDelta;

        scroller.OnSelectSong += UpdatePreview;
        if ( !TryGetComponent( out previewImage ) )
             Debug.LogError( "PreviewBGA RawImage Component is not found." );
    }

    protected abstract void UpdatePreview( Song _song );
}
