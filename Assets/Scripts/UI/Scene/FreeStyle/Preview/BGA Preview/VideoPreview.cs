using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class VideoPreview : FreeStylePreview
{
    public SoundPitchOption pitchOption;
    public RenderTexture renderTexture;
    private VideoPlayer vp;
    private Coroutine coroutine;
    private WaitUntil waitPrepared;

    protected override void Awake()
    {
        base.Awake();
        vp = GetComponent<VideoPlayer>();
        vp.targetTexture = renderTexture;
        waitPrepared = new WaitUntil( () => vp.isPrepared );

        pitchOption.OnPitchUpdate += PitchUpdate;
    }

    protected override void Restart( Song _song )
    {
        if ( _song.hasVideo )
            UpdatePreview( _song );
    }

    protected override void UpdatePreview( Song _song )
    {
        vp.Stop();
        if ( coroutine != null )
        {
            StopCoroutine( coroutine );
            coroutine = null;
        }

        if ( _song.hasVideo )
        {
            previewImage.enabled = false;
            previewImage.texture = renderTexture;
            coroutine = StartCoroutine( LoadVideo( _song ) );
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
        vp.url = @$"{_song.videoName}";
        vp.Prepare();

        yield return waitPrepared;

        float spb = ( float )( 60f / _song.mainBPM ) * 1000f;
        //float offset = _song.videoOffset > 1f ? _song.videoOffset * .75f :
        //               _song.audioOffset > 1f ? _song.audioOffset * .75f :
        //               _song.isOnlyKeySound   ? -spb                : 0f;

        //vp.playbackSpeed = GameSetting.CurrentPitch;
        //vp.time = ( AudioManager.Inst.Position + offset ) * .001f;

        tf.sizeDelta = sizeCache;
        previewImage.enabled = true;
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