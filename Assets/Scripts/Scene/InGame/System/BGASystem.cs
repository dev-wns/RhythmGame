using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class BGASystem : MonoBehaviour
{
    [SerializeField] private InGame scene;
    public RawImage background, foreground;

    [Header( "Video" )]
    public VideoPlayer vp;
    public RenderTexture renderTexture;

    private Color color;
    private BGA type;

    private void Awake()
    {
        //AudioManager.OnUpdatePitch += UpdatePitch;
        if ( GameSetting.BGAOpacity != 0 )
        {
            scene.OnLoadAsync += Load;
            scene.OnGameStart += GameStart;
            scene.OnPause     += Pause;
            scene.OnClear     += Clear;

            color = new Color( 1f, 1f, 1f, GameSetting.BGAOpacity * .01f );
            ClearRenderTexture();
        }
        else
        {
            transform.root.gameObject.SetActive( false );
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        ClearRenderTexture();

        if ( GameSetting.BGAOpacity != 0 )
        {
            //AudioManager.OnUpdatePitch -= UpdatePitch;
            scene.OnLoadAsync -= Load;
            scene.OnGameStart -= GameStart;
            scene.OnPause     -= Pause;
            scene.OnClear     -= Clear;
        }
    }

    //private void UpdatePitch( float _pitch )
    //{
    //    if ( type == BGA.Video )
    //         vp.playbackSpeed = _pitch;
    //}

    private void ClearRenderTexture()
    {
        RenderTexture rt = RenderTexture.active;
        RenderTexture.active = vp.targetTexture;
        GL.Clear( true, true, Color.black );
        RenderTexture.active = rt;
    }

    private async UniTask Load()
    {
        //if ( GameSetting.BGAOpacity == 0 )
        //{
        //    transform.root.gameObject.SetActive( false );
        //    return;
        //}

        type = Browser.Inst.SelectedSong.hasVideo  ? BGA.Video  :
               DataStorage.Inst.Sprites.Count > 0  ? BGA.Sprite :
                                                     BGA.Image;

        if ( type == BGA.Video )
        {

        }
        else if ( type == BGA.Sprite )
        {
            ReadOnlyCollection<SpriteSample> sprites = DataStorage.Inst.Sprites;
            for ( int i = 0; i < sprites.Count; i++ )
                  await DataStorage.Inst.LoadTextureAsync( sprites[i] );
        }
        else if ( type == BGA.Image )
        {
            // 이미 프리스타일에서 로딩된 이미지 사용
            await DataStorage.Inst.LoadTextureAsync( new SpriteSample( Browser.Inst.SelectedSong.imageName ), () =>
            {
                if ( DataStorage.Inst.GetTexture( Browser.Inst.SelectedSong.imageName, out Texture2D texture ) )
                {
                    background.color = color;
                    background.texture = texture;
                    background.rectTransform.sizeDelta = Global.UI.Screen.GetRatio( texture );
                }
                else
                {
                    transform.root.gameObject.SetActive( false );
                }
            } );
        }
    }

    private void GameStart()
    {
        if ( type == BGA.Video )
        {
            StartCoroutine( UpdateVideo() );
        }
        else if ( type == BGA.Sprite )
        {
            foreground.gameObject.SetActive( true );
            StartCoroutine( UpdateSprites() );
        }
    }

    private void Clear()
    {
        StopAllCoroutines();
        if (type == BGA.Video )
        {
            vp.Stop();
            ClearRenderTexture();
        }
        else if ( type == BGA.Sprite )
        {
            background.texture = Texture2D.blackTexture;
            foreground.texture = Texture2D.blackTexture;
        }
    }

    private void Pause( bool _isPause )
    {
        if ( type == BGA.Video )
        {
            if ( _isPause ) vp.Pause();
            else            vp.Play();
        }
    }

    private IEnumerator UpdateVideo()
    {
        vp.enabled       = true;
        vp.url           = @$"{Browser.Inst.SelectedSong.videoPath}";
        vp.playbackSpeed = GameSetting.SoundPitch;
        vp.targetTexture = renderTexture;

        background.texture = renderTexture;
        background.color   = color;

        vp.Prepare();

        double videoOffset = Browser.Inst.SelectedSong.videoOffset;
        double delay       = Global.Math.Max( 0d, -videoOffset );
        yield return new WaitUntil( () => vp.isPrepared );
        yield return new WaitUntil( () => NowPlaying.Playback >= delay );
        vp.time = Math.Max( 0d, ( ( NowPlaying.Playback - delay ) + videoOffset ) * .001d );
        vp.Play();

        // videoOffset -1000   playback  1000 시작 time     0 ms
        // videoOffset -10000  playback 10000 시작 time     0 ms
        // videoOffset  1000   playback     0 시작 time  1000 ms
        // videoOffset  10000  playback     0 시작 time 10000 ms
    }

    private IEnumerator UpdateSprites()
    {
        var sprites = DataStorage.Inst.Sprites;
        if ( sprites == null )
             yield break;

        int index           = 0;
        SpriteSample sprite = 0 < sprites.Count ? sprites[index] : new SpriteSample();
        WaitUntil waitStart = new WaitUntil( () => sprite.start <= NowPlaying.Playback );
        WaitUntil waitEnd   = new WaitUntil( () => sprite.end   <= NowPlaying.Playback );
        RawImage  rdr       = sprite.type == SpriteType.Background ? background : foreground;
        foreground.enabled  = true; // 전경 사용
        background.color = foreground.color = color;

        while ( index < sprites.Count )
        {
            yield return waitStart;
            if ( DataStorage.Inst.GetTexture( sprite.name, out Texture2D texture ) )
            {
                rdr.texture = texture;
                rdr.rectTransform.sizeDelta = Global.UI.Screen.GetRatio( texture );
            }

            yield return waitEnd;
            if ( ++index < sprites.Count )
            {
                sprite = sprites[index];
                rdr    = sprite.type == SpriteType.Background ? background : foreground;
            }
        }
    }
}

