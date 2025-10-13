using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;

public class PreviewBGARenderer : MonoBehaviour
{
    public Color color;
    private RectTransform tf;
    private PreviewBGASystem system;

    private readonly float fadeTime = .25f;
    private float fadeOffset;
    private BackgroundType type;
    private double videoOffset = 0d;
    private float pitch = 1f;

    [Header( "Image" )]
    private RawImage image;
    private bool isDefault;
    private string imageName;

    [Header( "Video Player" )]
    public RenderTexture renderTexture;
    private VideoPlayer vp;
    private Coroutine   CorUpdateVideo;


    private Dictionary<string/* name */, Texture2D> textures = new ();
    private List<SpriteSample> sprites = new List<SpriteSample>();
    private Coroutine CorUpdateSprites;



    private void Awake()
    {
        // Sprite

        // Video
        vp = GetComponent<VideoPlayer>();
        vp.targetTexture = renderTexture;

        // Image
        image = GetComponent<RawImage>();
        tf = transform as RectTransform;
        fadeOffset = color.a / fadeTime;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        foreach ( Texture2D texture in textures.Values )
        {
            DestroyImmediate( texture );
        }
    }

    public void UpdatePitch( float _pitch )
    {
        pitch = _pitch;
        vp.playbackSpeed = pitch;
    }

    public void Restart( Song _song )
    {
        if ( type == BackgroundType.Video )
        {
            if ( !ReferenceEquals( CorUpdateVideo, null ) )
            {
                StopCoroutine( CorUpdateVideo );
                CorUpdateVideo = null;
            }
            CorUpdateVideo = StartCoroutine( LoadVideo( _song ) );
        }
        else if ( type == BackgroundType.Sprite )
        {
            if ( !ReferenceEquals( CorUpdateSprites, null ) )
            {
                StopCoroutine( CorUpdateSprites );
                CorUpdateSprites = null;
            }

            CorUpdateSprites = StartCoroutine( UpdateSprites() );
        }
    }

    public void SetInfo( PreviewBGASystem _system, Song _song )
    {
        // Default Setting
        Clear();
        system = _system;
        image.color = color;
        videoOffset = _song.videoOffset;
        
        if ( _song.hasSprite )
        {
            type = BackgroundType.Sprite;
            using ( StreamReader reader = new StreamReader( @$"\\?\{_song.filePath}" ) )
            {
                string line;
                while ( ( line = reader.ReadLine() ) != "[Sprites]" ) { }
                while ( ( line = reader.ReadLine() ) != "[Samples]" )
                {
                    SpriteSample sprite;
                    var split = line.Split( ',' );

                    sprite.type = ( SpriteType ) int.Parse( split[0] );
                    if ( sprite.type != SpriteType.Background )
                         continue;

                    sprite.start = double.Parse( split[1] );
                    sprite.end   = double.Parse( split[2] );
                    sprite.name  = split[3];

                    if ( sprite.start > _song.previewTime )
                         sprites.Add( sprite );
                }
            }

            StartCoroutine( LoadSprites() );
            CorUpdateSprites = StartCoroutine( UpdateSprites() );
        }
        else if ( File.Exists( _song.videoPath ) )
        {
            type = BackgroundType.Video;
            CorUpdateVideo = StartCoroutine( LoadVideo( _song ) );
        }
        else
        {
            type = BackgroundType.Image;
            imageName = _song.imageName;
            StartCoroutine( LoadTexture( new SpriteSample( imageName ), BackgroundType.Image ) );
        }
    }

    public IEnumerator LoadTexture( SpriteSample _sprite, BackgroundType _type )
    {
        var path = Path.Combine( NowPlaying.CurrentSong.directory, _sprite.name );
        if ( !File.Exists( path ) || textures.ContainsKey( _sprite.name ) )
        {
            if ( _type == BackgroundType.Image )
            {
                image.enabled = true;
                isDefault     = true;
                image.texture = DataStorage.Inst.GetDefaultTexture();
                tf.sizeDelta  = Global.Screen.GetRatio( image.texture );
            }
            yield break;
        }

        Texture2D tex;
        if ( Path.GetExtension( path ) == ".bmp" )
        {
            // 비트맵 파일은 런타임에서 읽히지 않음( 외부 도움 )
            BMPLoader bitmapLoader = new BMPLoader();
            BMPImage img = bitmapLoader.LoadBMP( path );
            tex = img.ToTexture2D( TextureFormat.RGB24 );
        }
        else
        {
            // 그 외 JPG, JPEG, PNG 등 이미지 파일 로딩
            using ( UnityWebRequest www = UnityWebRequestTexture.GetTexture( path, true ) )
            {
                www.method = UnityWebRequest.kHttpVerbGET;
                using ( DownloadHandlerTexture handler = new DownloadHandlerTexture() )
                {
                    www.downloadHandler = handler;
                    yield return www.SendWebRequest();

                    if ( www.result == UnityWebRequest.Result.ConnectionError ||
                         www.result == UnityWebRequest.Result.ProtocolError )
                        throw new Exception( www.error );

                    tex = handler.texture;
                }
            }
        }

        if ( _type == BackgroundType.Image )
        {
            image.enabled = true;
            isDefault     = false;
            image.texture = tex;
            tf.sizeDelta  = Global.Screen.GetRatio( image.texture );
        }

        textures.Add( _sprite.name, tex );
    }

    private IEnumerator LoadSprites()
    {
        for ( int i = 0; i < sprites.Count; i++ )
        {
            yield return StartCoroutine( LoadTexture( sprites[i], BackgroundType.Sprite ) );
        }
    }

    private IEnumerator UpdateSprites()
    {
        int curIndex = 0;
        SpriteSample curSample = curIndex < sprites.Count ? sprites[curIndex] : new SpriteSample();

        WaitUntil waitSampleStart = new WaitUntil( () => curSample.start <= AudioManager.Inst.Position - videoOffset );
        WaitUntil waitSampleEnd   = new WaitUntil( () => curSample.end   <= AudioManager.Inst.Position - videoOffset );

        yield return waitSampleStart;


        image.enabled = true;
        while ( curIndex < sprites.Count )
        {
            yield return waitSampleStart;

            if ( textures.TryGetValue( curSample.name, out Texture2D texture ) )
            {
                tf.sizeDelta  = Global.Screen.GetRatio( texture );
                image.texture = texture;
            }

            yield return waitSampleEnd;
            if ( ++curIndex < sprites.Count )
                 curSample = sprites[curIndex];
        }
    }

    #region Load
    private IEnumerator LoadVideo( Song _song )
    {
        double startDelay = System.DateTime.Now.TimeOfDay.TotalMilliseconds;

        vp.enabled = true;
        image.texture = renderTexture;
        vp.url = @$"{_song.videoPath}";
        vp.playbackSpeed = GameSetting.CurrentPitch;
        vp.Prepare();

        yield return  new WaitUntil( () => vp.isPrepared );

        image.enabled = true;
        tf.sizeDelta  = new Vector2( Global.Screen.Width, Global.Screen.Height );

        vp.time = ( _song.previewTime + videoOffset + ( System.DateTime.Now.TimeOfDay.TotalMilliseconds - startDelay ) ) * .001f;
        vp.Play();
    }
    #endregion

    private void Clear()
    {
        ClearRenderTexture();
        tf.SetAsFirstSibling();
        isDefault     = false;
        image.enabled = false;
        vp.enabled    = false;
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
        StopAllCoroutines();
        StartCoroutine( FadeAfterDespawn() );
    }

    private IEnumerator FadeAfterDespawn()
    {
        while ( image.color.a >= 0f )
        {
            Color newColor = image.color;
            newColor.a -= fadeOffset * Time.deltaTime;
            image.color = newColor;

            yield return null;
        }

        if ( isDefault )
        {
            system.DeSpawn( this );
            yield break;
        }

        foreach( Texture2D texture in textures.Values )
        {
            DestroyImmediate( texture );
        }

        textures.Clear();
        sprites.Clear();
        system.DeSpawn( this );
    }
}
