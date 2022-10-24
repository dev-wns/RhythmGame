using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class VideoSample : MonoBehaviour
{
    public FreeStyleMainScroll scroller;
    public SoundPitchOption pitchOption;
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
        pitchOption.OnPitchUpdate += PitchUpdate;
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
            float time    = _song.previewTime <= 0 ? _song.totalTime * Mathf.PI * .1f : _song.previewTime;
            coroutine = StartCoroutine( LoadVideo( ( _song.videoOffset + time ) * .001f, _song.videoPath ) );
        }
        else
        {
            vp.Stop();
            image.enabled = false;
        }
    }

    private void PitchUpdate( float _pitch )
    {
        if ( !vp.isPlaying )
             return;

        vp.playbackSpeed = GameSetting.CurrentPitch;
    }

    private IEnumerator LoadVideo( float _time, string _path )
    {
        ClearRenderTexture();
        vp.url = @$"{_path}";
        vp.Prepare();

        yield return new WaitUntil( () => vp.isPrepared );
        vp.playbackSpeed = GameSetting.CurrentPitch;
        vp.time = _time;
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
