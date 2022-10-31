using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.IO;

public class VideoPreview : MonoBehaviour
{
    public FreeStyleMainScroll scroller;
    public SoundPitchOption pitchOption;
    private VideoPlayer vp;
    private RawImage image;
    public RenderTexture renderTexture;
    private Coroutine coroutine;
    private float playback;

    private void Awake()
    {
        image = GetComponent<RawImage>();
        vp = GetComponent<VideoPlayer>();
        vp.targetTexture = renderTexture;

        scroller.OnSelectSong += UpdateVideoSample;
        scroller.OnPlaybackUpdate += ( float _playback ) => playback = _playback;
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
            coroutine  = StartCoroutine( LoadVideo( _song ) );
        }
    }

    private void PitchUpdate( float _pitch )
    {
        if ( !vp.isPlaying )
             return;

        vp.playbackSpeed = GameSetting.CurrentPitch;
    }

    private IEnumerator LoadVideo( Song _song )
    {
        ClearRenderTexture();
        vp.url = @$"{_song.videoPath}";
        vp.Prepare();
        
        yield return new WaitUntil( () => vp.isPrepared );
        
        image.enabled = true;
        vp.playbackSpeed = GameSetting.CurrentPitch;

        float spb = ( float )( 60f / _song.medianBpm ) * 1000f;
        float offset = _song.videoOffset > 1f ? _song.videoOffset * .75f :
                       _song.audioOffset > 1f ? _song.audioOffset * .75f :
                       _song.isOnlyKeySound   ? -spb                : 0f;

        vp.time = ( SoundManager.Inst.Position + offset ) * .001f;

        Debug.Log( $" { _song.audioOffset}  { _song.videoOffset}  {offset}" );
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