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
    private bool isDefault;

    private readonly float fadeTime = .25f;
    private float fadeOffset;
    private BackgroundType type;
    private bool isDestroy;
    private double videoOffset = 0d;
    private float pitch = 1f;

    [Header( "Image" )]
    public  Sprite defaultImage;
    private RawImage image;

    [Header( "Video Player" )]
    public RenderTexture renderTexture;
    private VideoPlayer vp;
    private WaitUntil waitPrepared;

    [Header( "Sprites Player" )]
    private List<SpriteSample> sprites = new List<SpriteSample>();
    private Dictionary<string/* Sprite Name */, Texture2D> textures = new Dictionary<string, Texture2D>();

    private int spriteIndex; // Sprite Start Index
    private Coroutine CorUpdateSprites;
    private double previewTime;
    


    private void Awake()
    {
        // Sprite

        // Video
        vp = GetComponent<VideoPlayer>();
        vp.targetTexture = renderTexture;
        waitPrepared = new WaitUntil( () => vp.isPrepared );

        // Image
        image = GetComponent<RawImage>();
        tf = transform as RectTransform;
        fadeOffset = color.a / fadeTime;
    }

    private void Update()
    {
        if ( !isDestroy )
             return;

        Color newColor = image.color;
        newColor.a -= fadeOffset * Time.deltaTime;
        image.color = newColor;

        if ( image.color.a <= 0f )
        {
            Clear();
            system.DeSpawn( this );
        }
    }

    private void OnDestroy() => Clear();

    public void UpdatePitch( float _pitch )
    {
        pitch = _pitch;
        vp.playbackSpeed = pitch;
    }

    public void Restart( Song _song )
    {
        switch ( type )
        {
            case BackgroundType.Sprite:
            {
                if ( !ReferenceEquals( CorUpdateSprites, null ) )
                {
                    StopCoroutine( CorUpdateSprites );
                    CorUpdateSprites = null;
                }

                CorUpdateSprites = StartCoroutine( UpdateSprites() );
            } break;

            case BackgroundType.Video:
            {
                vp.time = ( AudioManager.Inst.Position + videoOffset ) * .001f;
                vp.Play();
            } break;
        }

    }

    public void SetInfo( PreviewBGASystem _system, Song _song )
    {
        // Default Setting
        system = _system;
        tf.SetAsFirstSibling();
        image.color = color;
        videoOffset = _song.videoOffset;

        // Update BGA Type
        if ( _song.hasSprite )
        {
            type = BackgroundType.Sprite;
            previewTime = _song.previewTime <= 0 ? _song.totalTime * .314f : _song.previewTime;
            using ( StreamReader reader = new StreamReader( @$"\\?\{_song.filePath}" ) )
            {
                string line;
                while ( ( line = reader.ReadLine() ) != "[Sprites]" ) { }
                while ( ( line = reader.ReadLine() ) != "[Samples]" )
                {
                    SpriteSample sprite;
                    var split = line.Split( ',' );

                    sprite.type = ( SpriteType )int.Parse( split[0] );
                    if ( sprite.type != SpriteType.Background )
                        continue;

                    sprite.start = double.Parse( split[1] );
                    sprite.end   = double.Parse( split[2] );
                    sprite.name  = split[3];

                    if ( sprite.start < previewTime - videoOffset )
                         spriteIndex = sprites.Count;

                    sprites.Add( sprite );
                }
            }

            StartCoroutine( LoadSprites( _song ) );
            CorUpdateSprites = StartCoroutine( UpdateSprites() );
        }
        else if ( _song.hasVideo )
        {
            type = BackgroundType.Video;
            StartCoroutine( LoadVideo( _song ) );
        }
        else
        {
            type = BackgroundType.Image;
            StartCoroutine( LoadImage( _song.imagePath ) );
        }
    }

    private IEnumerator UpdateSprites()
    {
        SpriteSample curSample = new SpriteSample();
        int curIndex = spriteIndex;
        if ( curIndex < sprites.Count )
             curSample = sprites[curIndex];

        WaitUntil waitSampleStart = new WaitUntil( () => curSample.start <= AudioManager.Inst.Position - videoOffset );
        WaitUntil waitSampleEnd   = new WaitUntil( () => curSample.end   <= AudioManager.Inst.Position - videoOffset );
        yield return waitSampleStart;

        // Wait First Texture
        yield return new WaitUntil( () => textures.ContainsKey( curSample.name ) );
        tf.sizeDelta  = Global.Math.GetScreenRatio( textures[curSample.name], new Vector2( Global.Screen.Width, Global.Screen.Height ) );
        image.enabled = true;

        while ( curIndex < sprites.Count )
        {
            yield return waitSampleStart;

            if ( textures.ContainsKey( curSample.name ) )
            {
                image.texture = textures[curSample.name];
            }

            yield return waitSampleEnd;
            if ( ++curIndex < sprites.Count )
                 curSample = sprites[curIndex];
        }
    }

    #region Load
    private IEnumerator LoadSprites( Song _song )
    {
        var dir = Path.GetDirectoryName( _song.filePath );
        for ( int i = spriteIndex; i < sprites.Count; i++ )
        {
            if ( !textures.ContainsKey( sprites[i].name ) )
            {
                Texture2D tex;
                var path = @Path.Combine( dir, sprites[i].name );
                if ( Path.GetExtension( path ).Contains( ".bmp" ) )
                {
                    BMPLoader loader = new BMPLoader();
                    BMPImage img = loader.LoadBMP( path );
                    tex = img.ToTexture2D( TextureFormat.RGB24 );
                }
                else
                {
                    using ( UnityWebRequest www = UnityWebRequestTexture.GetTexture( path, true ) )
                    {
                        www.method = UnityWebRequest.kHttpVerbGET;
                        using ( DownloadHandlerTexture handler = new DownloadHandlerTexture() )
                        {
                            www.downloadHandler = handler;
                            yield return www.SendWebRequest();

                            if ( www.result == UnityWebRequest.Result.ConnectionError ||
                                 www.result == UnityWebRequest.Result.ProtocolError )
                            {
                                Debug.LogError( $"UnityWebRequest Error : {www.error}" );
                                throw new System.Exception( $"UnityWebRequest Error : {www.error}" );
                            }
                            tex = handler.texture;
                        }
                    }
                }
                textures.Add( sprites[i].name, tex );
                yield return null;
            }
        }
    }

    private IEnumerator LoadVideo( Song _song )
    {
        vp.enabled = true;
        image.texture = renderTexture;
        vp.url = @$"{_song.videoPath}";
        vp.playbackSpeed = GameSetting.CurrentPitch;
        vp.Prepare();

        yield return waitPrepared;

        image.enabled = true;
        tf.sizeDelta  = new Vector2( Global.Screen.Width, Global.Screen.Height );

        vp.time = ( AudioManager.Inst.Position + videoOffset ) * .001f;
        vp.Play();

    }
    private IEnumerator LoadImage( string _path )
    {
        bool isExist = System.IO.File.Exists( _path );
        if ( isExist )
        {
            var ext = System.IO.Path.GetExtension( _path );
            if ( ext.Contains( ".bmp" ) )
            {
                BMPLoader loader = new BMPLoader();
                BMPImage img = loader.LoadBMP( _path );
                image.texture = img.ToTexture2D();
            }
            else
            {
                using ( UnityWebRequest www = UnityWebRequestTexture.GetTexture( _path, true ) )
                {
                    www.method = UnityWebRequest.kHttpVerbGET;
                    using ( DownloadHandlerTexture handler = new DownloadHandlerTexture() )
                    {
                        www.downloadHandler = handler;
                        yield return www.SendWebRequest();

                        if ( www.result == UnityWebRequest.Result.ConnectionError ||
                             www.result == UnityWebRequest.Result.ProtocolError )
                        {
                            Debug.LogError( $"UnityWebRequest Error : {www.error}" );
                            throw new System.Exception( $"UnityWebRequest Error : {www.error}" );
                        }

                        image.texture = handler.texture;
                    }
                }
            }
        }
        else
        {
            image.texture = defaultImage.texture;
            isDefault = true;
        }

        tf.sizeDelta = Global.Math.GetScreenRatio( image.texture, new Vector2( Global.Screen.Width, Global.Screen.Height ) );
        image.enabled = true;
    }
    #endregion

    private void Clear()
    {
        StopAllCoroutines();
        isDestroy = false;
        isDefault = false;
        image.enabled = false;
        vp.enabled = false;

        switch ( type )
        {
            case BackgroundType.Sprite:
            {
                foreach ( Texture2D texture in textures.Values )
                {
                    DestroyImmediate( texture, true );
                }
                textures.Clear();
                sprites.Clear();
            }
            break;

            case BackgroundType.Video:
            {
                RenderTexture rt = RenderTexture.active;
                RenderTexture.active = vp.targetTexture;
                GL.Clear( true, true, Color.black );
                RenderTexture.active = rt;
            }
            break;

            case BackgroundType.Image:
            {
                if ( !isDefault && image.texture )
                {
                    DestroyImmediate( image.texture, true );
                }
            }
            break;
        }
    }

    public void Despawn()
    {
        isDestroy = true;
    }
}
