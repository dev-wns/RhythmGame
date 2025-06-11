using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.Video;

public enum BackgroundType : byte { None, Video, Sprite, Image, }

public class BGASystem : MonoBehaviour
{
    private InGame scene;
    public RawImage background, foreground;

    [Header( "Video" )]
    public VideoPlayer vp;
    public RenderTexture renderTexture;

    private Color color;
    private BackgroundType type;

    private void Awake()
    {
        NowPlaying.OnPreInit += Initialize;
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnReLoad += OnReLoad;

        color = new Color( 1f, 1f, 1f, GameSetting.BGAOpacity * .01f );

        foreground.enabled = false;
        ClearRenderTexture();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        ClearRenderTexture();

        NowPlaying.OnPreInit -= Initialize;
    }

    private void ClearRenderTexture()
    {
        RenderTexture rt = RenderTexture.active;
        RenderTexture.active = vp.targetTexture;
        GL.Clear( true, true, Color.black );
        RenderTexture.active = rt;
    }

    private void Initialize()
    {
        if ( GameSetting.BGAOpacity == 0 )
        {
            transform.root.gameObject.SetActive( false );
            return;
        }

        type = NowPlaying.CurrentSong.hasVideo    ? BackgroundType.Video  :
               DataStorage.Backgrounds.Count > 0 ||
               DataStorage.Foregrounds.Count > 0  ? BackgroundType.Sprite :
                                                    BackgroundType.Image;

        switch ( type )
        {
            case BackgroundType.Video:
            {
                scene.OnPause     += OnPause;


                StartCoroutine( UpdateVideo() );
            } break;

            case BackgroundType.Sprite:
            {
                foreground.gameObject.SetActive( true );

                StartCoroutine( UpdateSprites( background, SpriteType.Background ) );
                StartCoroutine( UpdateSprites( foreground, SpriteType.Foreground ) );
            } break;

            case BackgroundType.Image:
            {
                // 이미 프리스타일에서 로딩된 이미지 사용
                if ( DataStorage.Inst.TryGetTexture( NowPlaying.CurrentSong.imageName, out Texture2D texture ) )
                {
                    background.texture = texture;
                    background.color   = color;
                    background.rectTransform.sizeDelta = Global.Screen.GetRatio( texture );
                }
                else
                {
                    transform.root.gameObject.SetActive( false );
                }
            } break;
        }
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

        if ( _isPause ) vp.Pause();
        else StartCoroutine( WaitVideoTime() );
    }

    private IEnumerator WaitVideoTime()
    {
        yield return new WaitUntil( () => NowPlaying.Playback > vp.time );
        vp.Play();
    }

    private IEnumerator UpdateVideo()
    {
        vp.enabled       = true;
        vp.url           = @$"{NowPlaying.CurrentSong.videoPath}";
        vp.playbackSpeed = GameSetting.CurrentPitch;
        vp.targetTexture = renderTexture;

        background.texture = renderTexture;
        background.color   = color;

        vp.Prepare();
        yield return new WaitUntil( () => vp.isPrepared );

        double videoOffset = 0d < DataStorage.Samples.Count ? DataStorage.Samples[0].time : 0d;
        yield return new WaitUntil( () => videoOffset <= NowPlaying.Playback );
        vp.Play();
    }

    private IEnumerator UpdateSprites( RawImage _renderer, SpriteType _type )
    {
        ReadOnlyCollection<SpriteSample> sprites = _type == SpriteType.Background ? DataStorage.Backgrounds :
                                                   _type == SpriteType.Foreground ? DataStorage.Foregrounds : null;
        if ( sprites == null )
             yield break;

        int index           = 0;
        SpriteSample sprite = 0 < sprites.Count ? sprites[index] : new SpriteSample();
        WaitUntil waitStart = new WaitUntil( () => sprite.start <= NowPlaying.Playback );
        WaitUntil waitEnd   = new WaitUntil( () => sprite.end   <= NowPlaying.Playback );

        // 첫번쨰 스프라이트가 시작될때 활성화 시킨다.
        yield return waitStart;
        _renderer.enabled = true;
        _renderer.color   = color;

        while ( index < sprites.Count )
        {
            yield return waitStart;
            if ( DataStorage.Inst.TryGetTexture( sprite.name, out Texture2D texture ) )
            {
                _renderer.texture = texture;
                _renderer.rectTransform.sizeDelta = Global.Screen.GetRatio( texture );
            }

            yield return waitEnd;
            if ( ++index < sprites.Count )
                 sprite = sprites[index];
        }

        _renderer.enabled = false;
    }
}

