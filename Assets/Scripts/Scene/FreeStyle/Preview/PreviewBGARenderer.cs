using System;
using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PreviewBGARenderer : MonoBehaviour, IObjectPool<PreviewBGARenderer>
{
    public ObjectPool<PreviewBGARenderer> pool { get; set; }

    private RectTransform tf;

    private BGA type;
    private string directory;
    private static readonly float FadeDuration = .25f;

    [Header( "[Image]" )]
    private RawImage image;
    private bool isDefault;
    private string imageName;

    [Header( "[Video Player]" )]
    [SerializeField] private RenderTexture renderTextureDesc;
    private RenderTexture renderTexture;
    private VideoPlayer vp;
    private double videoTime;

    [Header( "[Sprite]" )]
    private Dictionary<string/* name */, Texture2D> textures = new ();
    private List<SpriteSample> sprites = new ();
    private CancellationTokenSource spriteCts;
    private Coroutine CorUpdateSprites;

    private bool isLoading = true;

    private void Awake()
    {
        // Video
        vp = GetComponent<VideoPlayer>();
        renderTexture    = new RenderTexture( renderTextureDesc.descriptor );
        vp.targetTexture = renderTexture;

        // Image
        image = GetComponent<RawImage>();
        tf = transform as RectTransform;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        spriteCts?.Cancel();
        spriteCts?.Dispose();
        spriteCts = null;

        vp.targetTexture = null;
        vp?.Stop();

        foreach ( Texture2D texture in textures.Values )
        {
            DestroyImmediate( texture );
        }
        textures.Clear();
        sprites.Clear();
        textures = null;
        sprites  = null;

        if ( renderTexture )
        {
            if ( renderTexture.IsCreated() )
                 renderTexture.Release();

            DestroyImmediate( renderTexture );
            renderTexture = null;
        }
    }

    public void UpdatePitch( float _pitch )
    {
        if ( type == BGA.Video )
             vp.playbackSpeed = _pitch;
    }

    public void Play( Song _song )
    {
        if ( type == BGA.Video )
        {
            image.enabled = true;
            vp.playbackSpeed = GameSetting.SoundPitch;
            vp.time = videoTime;
            vp?.Play();
        }
        else if ( type == BGA.Sprite )
        {
            if ( !ReferenceEquals( CorUpdateSprites, null ) )
            {
                StopCoroutine( CorUpdateSprites );
                CorUpdateSprites = null;
            }

            CorUpdateSprites = StartCoroutine( UpdateSprites() );
        }
        else if ( type == BGA.Image )
        {
            image.enabled = true;
            if ( textures.TryGetValue( imageName, out Texture2D texture ) )
            {
                isDefault = false;
                image.texture = texture;
            }
            else
            {
                isDefault = true;
                image.texture = DataStorage.Inst.GetDefaultTexture();
            }
            tf.sizeDelta  = Global.UI.Screen.GetRatio( image.texture );
        }
    }

    private IEnumerator UpdateSprites()
    {
        int curIndex = 0;
        SpriteSample curSample    = curIndex < sprites.Count ? sprites[curIndex] : new SpriteSample();
        WaitUntil waitSpriteStart = new WaitUntil( () => curSample.start <= AudioManager.Inst.Position );
        WaitUntil waitSpriteEnd   = new WaitUntil( () => curSample.end   <= AudioManager.Inst.Position );

        // 첫 스프라이트 대기
        yield return waitSpriteStart;
        image.enabled = true;

        while ( curIndex < sprites.Count )
        {
            yield return waitSpriteStart;

            if ( textures.TryGetValue( curSample.name, out Texture2D texture ) )
            {
                tf.sizeDelta = Global.UI.Screen.GetRatio( texture );
                image.texture = texture;
            }

            yield return waitSpriteEnd;
            if ( ++curIndex < sprites.Count )
                 curSample = sprites[curIndex];
        }

    }

    public async UniTask LoadAsync( Song _song, CancellationToken _token )
    {
        Clear();

        directory = _song.directory;
        if ( _song.hasSprite )
        {
            type = BGA.Sprite;

            sprites.Clear();
            foreach ( SpriteSample sprite in DataStorage.Inst.Sprites )
            {
                _token.ThrowIfCancellationRequested();
                if ( sprite.start > _song.previewTime )
                     sprites.Add( sprite );
            }

            // 스프라이트는 로딩된만큼만 보여주기 때문에 개인 Cts 사용
            spriteCts = new CancellationTokenSource();
            LoadSpritesAsync( spriteCts.Token ).Forget();
        }
        else if ( File.Exists( _song.videoPath ) )
        {
            type = BGA.Video;
            try     { await LoadVideoAsync( _song, _token ); }
            finally { isLoading = false; }
        }
        else
        {
            type = BGA.Image;
            imageName = _song.imageName;
            try 
            {
                if ( !textures.ContainsKey( imageName ) )
                {
                    Texture2D tex = await DataStorage.Inst.LoadTextureAsync( _song.imagePath, SpriteType.Background, _token );
                    if ( tex != null ) textures.Add( imageName, tex );
                }
            }
            finally { isLoading = false; }
        }
    }

    private async UniTask LoadSpritesAsync( CancellationToken _token )
    {
        try
        {
            for ( int i = 0; i < sprites.Count; i++ )
            {
                _token.ThrowIfCancellationRequested();
                if ( !textures.ContainsKey( sprites[i].name ) &&
                      sprites[i].type == SpriteType.Background )
                {
                    Texture2D tex = await DataStorage.Inst.LoadTextureAsync( directory, sprites[i], _token );
                    if ( tex != null ) 
                         textures.Add( sprites[i].name, tex );
                }

                await UniTask.Yield( _token );
            }
        }
        finally { isLoading = false; }
    }

    private async UniTask LoadVideoAsync( Song _song, CancellationToken _token )
    {
        vp.enabled    = true;
        image.texture = renderTexture;
        vp.url        = @$"{_song.videoPath}";

        tf.sizeDelta  = new Vector2( Global.UI.Screen.Width, Global.UI.Screen.Height );
        videoTime     = Global.Math.Max( 0d, ( _song.previewTime + _song.audioOffset + _song.videoOffset ) * .001d );
        
        vp.Prepare();
        await UniTask.WaitUntil( () => vp.isPrepared, PlayerLoopTiming.Update, _token );
        
        // AudioOffset : 프리스타일에서 노트와 음악의 영점 맞추기
        // VideoOffset : 비디오 재생 지연시간
        // PreviewTime : 해당 위치를 기준으로 계산
    }

    private void Clear()
    {
        ClearRenderTexture();
        tf.SetAsFirstSibling();
        directory     = string.Empty;
        isLoading     = true;
        isDefault     = false;
        image.enabled = false;
        image.color   = Color.white;
        vp.enabled    = false;
        videoTime     = 0d;
    }

    private void ClearRenderTexture()
    {
        RenderTexture rt = RenderTexture.active;
        RenderTexture.active = vp.targetTexture;
        GL.Clear( true, true, Color.black );
        RenderTexture.active = rt;
    }

    public void Despawn()
    {
        spriteCts?.Cancel();
        spriteCts?.Dispose();
        spriteCts = null;

        StopAllCoroutines();
        CorUpdateSprites = null;

        StartCoroutine( FadeAfterDespawn() );
    }

    private IEnumerator FadeAfterDespawn()
    {
        while ( image.color.a > 0f )
        {
            Color newColor = image.color;
            newColor.a -= Time.deltaTime / FadeDuration;
            image.color = newColor;

            yield return null;
        }

        if ( isDefault ) // 기본 이미지셋 사용 중
        {
            pool.Despawn( this );
            yield break;
        }

        yield return new WaitUntil( () => !isLoading );
        foreach( Texture2D texture in textures.Values )
        {
            DestroyImmediate( texture );
        }

        textures.Clear();
        sprites.Clear();
        pool.Despawn( this );
    }
}
