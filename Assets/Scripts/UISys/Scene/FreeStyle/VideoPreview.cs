using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class VideoPreview : MonoBehaviour
{
    public FreeStyleMainScroll scroller;
    public SoundPitchOption pitchOption;
    private VideoPlayer vp;
    private RawImage image;
    public RenderTexture renderTexture;
    private Coroutine coroutine;
    private double playback;

    private void Awake()
    {
        image = GetComponent<RawImage>();
        vp = GetComponent<VideoPlayer>();
        vp.targetTexture = renderTexture;

        scroller.OnSelectSong += UpdateVideoSample;
        scroller.OnPlaybackUpdate += ( double _playback ) => playback = _playback;
        pitchOption.OnPitchUpdate += PitchUpdate;
    }

    private void UpdateVideoSample( Song _song )
    {
        vp.Stop();
        if ( coroutine != null )
        {
            StopCoroutine( coroutine );
            coroutine = null;
        }

        if ( _song.hasVideo )
        {
            image.enabled = false;
            image.texture = renderTexture;
            coroutine = StartCoroutine( LoadVideo( _song.audioOffset * .5f, _song.videoPath ) );
        }
    }

    private void PitchUpdate( float _pitch )
    {
        if ( !vp.isPlaying )
             return;

        vp.playbackSpeed = GameSetting.CurrentPitch;
    }

    private IEnumerator LoadVideo( float _offset, string _path )
    {
        ClearRenderTexture();
        vp.url = @$"{_path}";
        vp.Prepare();

        yield return new WaitUntil( () => vp.isPrepared );

        image.enabled = true;
        vp.playbackSpeed = GameSetting.CurrentPitch;
        vp.time = ( playback + _offset ) * .001f;
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
