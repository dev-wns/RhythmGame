using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class VideoSample : MonoBehaviour
{
    public FreeStyleMainScroll scroller;
    private VideoPlayer vp;
    private RawImage image;
    public RenderTexture renderTexture;
    private Coroutine coroutine;

    private void Awake()
    {
        vp = GetComponent<VideoPlayer>();
        image = GetComponent<RawImage>();
        vp.targetTexture = renderTexture;
        image.texture    = renderTexture;

        scroller.OnSelectSong += UpdateVideoSample;
    }

    private void UpdateVideoSample( Song _song )
    {
        if ( coroutine != null )
        {
            StopCoroutine( coroutine );
            coroutine = null;
        }

        if ( _song.hasVideo )
        {
            image.enabled = true;
            float time   = _song.previewTime;
            float length = _song.totalTime / GameSetting.CurrentPitch;
            time = time <= 0 ? ( length * GameSetting.CurrentPitch ) * Mathf.PI * .1f : time;
            coroutine = StartCoroutine( LoadVideo( ( _song.videoOffset + time ) * .001f, _song.videoPath ) );
        }
        else
        {
            vp.Stop();
            image.enabled = false;
        }
    }

    private IEnumerator LoadVideo( float _time, string _path )
    {
        ClearRenderTexture();
        vp.url = @$"{_path}";
        vp.Prepare();

        yield return new WaitUntil( () => vp.isPrepared );
        if ( _time > vp.length )
        {
            vp.playbackSpeed = 1.05f;
            vp.time = 0f;
        }
        else
        {
            vp.playbackSpeed = 1f;
            vp.time = _time;
        }
        vp.Play();
    }

    private void ClearRenderTexture()
    {
        RenderTexture rt = RenderTexture.active;
        RenderTexture.active = vp.targetTexture;
        GL.Clear( true, true, Color.black );
        RenderTexture.active = rt;
    }
}
