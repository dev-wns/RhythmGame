using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public enum BackgroundType : byte { None, Video, Sprite, Image, }

public class BGASystem : MonoBehaviour
{
    private InGame scene;
    public RawImage background, foreground;

    [Header( "Video" )]
    public VideoPlayer vp;
    public RenderTexture renderTexture;

    //public event Action<BackgroundType> OnInitialize;
    //public event Action<int/* texture */, int /* duplicate */, int/* bg */, int /* fg */> OnUpdateData;

    private List<SpriteSample> backgrounds = new List<SpriteSample>();
    private List<SpriteSample> foregrounds = new List<SpriteSample>();

    private Color color;
    private BackgroundType type;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnSystemInitialize += Initialize;
        scene.OnReLoad           += OnReLoad;
        scene.OnUpdatePitch      += UpdatePitch;

        color = new Color( 1f, 1f, 1f, GameSetting.BGAOpacity * .01f );

        background.color = Color.clear;
        foreground.color = Color.clear;
        ClearRenderTexture();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        ClearRenderTexture();
    }

    private void ClearRenderTexture()
    {
        RenderTexture rt = RenderTexture.active;
        RenderTexture.active = vp.targetTexture;
        GL.Clear( true, true, Color.black );
        RenderTexture.active = rt;
    }

    private void Initialize() => StartCoroutine( Load() );

    private IEnumerator Load()
    {
        if ( GameSetting.BGAOpacity == 0 )
        {
            transform.root.gameObject.SetActive( false );
            NowPlaying.IsLoadBGA = true;
            yield break;
        }

        type = NowPlaying.CurrentSong.hasVideo           ? BackgroundType.Video  :
               NowPlaying.CurrentChart.sprites.Count > 0 ? BackgroundType.Sprite :
                                                           BackgroundType.Image;

        switch ( type )
        {
            case BackgroundType.Video:
            {
                scene.OnGameStart += PlayVideo;
                scene.OnPause     += OnPause;

                vp.enabled         = true;
                vp.url             = @$"{NowPlaying.CurrentSong.videoPath}";
                vp.playbackSpeed   = GameSetting.CurrentPitch;
                vp.targetTexture   = renderTexture;
                background.texture = renderTexture;
                background.color   = color;

                vp.Prepare();
                yield return new WaitUntil( () => vp.isPrepared );

                if ( vp.isPlaying ) vp.Play();
                vp.Pause();
                vp.frame = 0;
            } break;

            case BackgroundType.Sprite:
            {
                foreground.gameObject.SetActive( true );
                scene.OnGameStart += SpriteProcess;

                var sprites = NowPlaying.CurrentChart.sprites;
                for ( int i = 0; i < sprites.Count; i++ )
                {
                    yield return StartCoroutine( DataStorage.Inst.LoadTexture( sprites[i] ) );

                    if ( sprites[i].type == SpriteType.Background ) backgrounds.Add( sprites[i] );
                    else                                            foregrounds.Add( sprites[i] );
                }
            } break;

            case BackgroundType.Image:
            {
                if ( System.IO.File.Exists( NowPlaying.CurrentSong.imagePath ) )
                {
                    string name = NowPlaying.CurrentSong.imageName;
                    yield return StartCoroutine( DataStorage.Inst.LoadTexture( new SpriteSample( name ) ) );

                    if ( DataStorage.Inst.TryGetTexture( name, out Texture2D texture ) )
                    {
                        background.texture = texture;
                        background.color   = color;
                        background.rectTransform.sizeDelta = Global.Screen.GetRatio( texture );
                    }
                }
                else
                {
                    transform.root.gameObject.SetActive( false );
                }
            } break;
        }

        NowPlaying.IsLoadBGA = true;
    }

    private void PlayVideo()
    {
        background.texture = renderTexture;
        StartCoroutine( WaitVideo() );
    }

    private IEnumerator WaitVideo()
    {
        yield return new WaitUntil( () => NowPlaying.CurrentSong.videoOffset <= NowPlaying.Playback );
        vp.Play();
    }

    private void UpdatePitch( float _pitch )
    {
        if ( type != BackgroundType.Video )
             return;

        vp.playbackSpeed = _pitch;
    }

    private void OnReLoad()
    {
        switch ( type )
        {
            case BackgroundType.None:
            case BackgroundType.Image:
            break;

            case BackgroundType.Video:
            {
                ClearRenderTexture();
                
                if ( !vp.isPlaying ) 
                     vp.Play();

                vp.Pause();
                vp.frame = 0;
            }
            break;

            case BackgroundType.Sprite:
            {
                background.texture = Texture2D.blackTexture;
                foreground.texture = Texture2D.blackTexture;
            }
            break;
        }
    }

    private void OnPause( bool _isPause )
    {
        if ( type != BackgroundType.Video )
            return;

        if ( _isPause ) vp?.Pause();
        else StartCoroutine( WaitVideoTime() );
    }

    private IEnumerator WaitVideoTime()
    {
        yield return new WaitUntil( () => NowPlaying.Playback > NowPlaying.SaveTime - NowPlaying.WaitPauseTime );
        vp.Play();
    }

    private void SpriteProcess()
    {
        StartCoroutine( PlaySprites( background, backgrounds ) );
        StartCoroutine( PlaySprites( foreground, foregrounds ) );
    }

    private IEnumerator PlaySprites( RawImage _renderer, List<SpriteSample> _samples )
    {
        int index           = 0;
        DataStorage  datas  = DataStorage.Inst;
        SpriteSample sprite = 0 < _samples.Count ? _samples[index]    : new SpriteSample();

        WaitUntil waitStart = new WaitUntil( () => sprite.start <= NowPlaying.Playback );
        WaitUntil waitEnd   = new WaitUntil( () => sprite.end   <= NowPlaying.Playback );

        // 첫번쨰 스프라이트가 시작될때 Alpha값을 활성화 시킨다.
        yield return waitStart;
        _renderer.color = color;

        while ( index < _samples.Count )
        {
            yield return waitStart;
            if ( DataStorage.Inst.TryGetTexture( sprite.name, out Texture2D texture ) )
            {
                _renderer.texture = texture;
                _renderer.rectTransform.sizeDelta = Global.Screen.GetRatio( texture );
            }

            yield return waitEnd;
            if ( ++index < _samples.Count )
                 sprite = _samples[index];
        }
    }
}

