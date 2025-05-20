using UnityEngine;
using UnityEngine.UI;

public abstract class FreeStylePreview : MonoBehaviour
{
    public FreeStyleMainScroll scroller;
    protected RawImage previewImage;
    protected RectTransform tf { get; private set; }
    protected Vector2 sizeCache { get; private set; }

    private readonly float duration = .25f;
    private bool isPlay;

    protected virtual void Awake()
    {
        scroller.OnSelectSong += UpdatePreview;
        scroller.OnSoundRestart += Restart;
        if ( !TryGetComponent( out previewImage ) )
            Debug.LogError( "PreviewBGA RawImage Component is not found." );

        tf = transform as RectTransform;
        sizeCache = tf.sizeDelta;
    }

    protected virtual void Update()
    {
        if ( !isPlay )
            return;

        float offset = Time.deltaTime / duration;
        Vector3 newScale = tf.localScale;
        newScale.x += offset;
        newScale.y += offset;
        tf.localScale = newScale;

        if ( tf.localScale.x >= 1f )
        {
            isPlay = false;
            tf.localScale = Vector3.one;
        }
    }

    protected void PlayScaleEffect()
    {
        isPlay = true;
        tf.localScale = new Vector3( 0f, 0f, 1f );
    }

    protected abstract void UpdatePreview( Song _song );

    protected abstract void Restart( Song _song );
}
