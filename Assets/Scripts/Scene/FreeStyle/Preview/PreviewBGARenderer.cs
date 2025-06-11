using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    private bool isDestroy;
    private double videoOffset = 0d;
    private float pitch = 1f;

    [Header( "Image" )]
    public  Sprite defaultImage;
    private RawImage image;

    [Header( "Video Player" )]
    public RenderTexture renderTexture;
    private VideoPlayer vp;
    private Coroutine   CorUpdateVideo;

    //[Header( "Sprites Player" )]
    //private List<SpriteSample> sprites = new List<SpriteSample>();
    //private Coroutine CorUpdateSprites;
    


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
            //case BackgroundType.Sprite:
            //{
            //    if ( !ReferenceEquals( CorUpdateSprites, null ) )
            //    {
            //        StopCoroutine( CorUpdateSprites );
            //        CorUpdateSprites = null;
            //    }

            //    CorUpdateSprites = StartCoroutine( UpdateSprites() );
            //} break;

            case BackgroundType.Video:
            {
                if ( !ReferenceEquals( CorUpdateVideo, null ) )
                {
                    StopCoroutine( CorUpdateVideo );
                    CorUpdateVideo = null;
                }
                CorUpdateVideo = StartCoroutine( LoadVideo( _song ) );
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
        //if ( _song.hasSprite )
        //{
        //    type = BackgroundType.Sprite;
        //    using ( StreamReader reader = new StreamReader( @$"\\?\{_song.filePath}" ) )
        //    {
        //        string line;
        //        while ( ( line = reader.ReadLine() ) != "[Sprites]" ) { }
        //        while ( ( line = reader.ReadLine() ) != "[Samples]" )
        //        {
        //            SpriteSample sprite;
        //            var split = line.Split( ',' );

        //            sprite.type = ( SpriteType )int.Parse( split[0] );
        //            if ( sprite.type != SpriteType.Background )
        //                continue;

        //            sprite.start = double.Parse( split[1] );
        //            sprite.end   = double.Parse( split[2] );
        //            sprite.name  = split[3];

        //            if ( sprite.start > _song.previewTime - videoOffset )
        //                 sprites.Add( sprite );
        //        }
        //    }

        //    DataStorage.Inst.LoadTextures( sprites );
        //    CorUpdateSprites = StartCoroutine( UpdateSprites() );
        //}
        //else if ( _song.hasVideo )
        if ( _song.hasVideo )
        {
            type = BackgroundType.Video;
            CorUpdateVideo = StartCoroutine( LoadVideo( _song ) );
        }
        else
        {
            type = BackgroundType.Image;
            DataStorage.Inst.LoadTexture( new SpriteSample( _song.imageName ), () =>
            {
                image.enabled = true;
                if ( DataStorage.Inst.TryGetTexture( _song.imageName, out Texture2D texture ) ) image.texture = texture;
                else                                                                            image.texture = DataStorage.Inst.GetDefaultTexture();
                tf.sizeDelta  = Global.Screen.GetRatio( image.texture );
            } );
        }
    }

    //private IEnumerator UpdateSprites()
    //{
    //    int curIndex = 0;
    //    SpriteSample curSample = curIndex < sprites.Count ? sprites[curIndex] : new SpriteSample();

    //    WaitUntil waitSampleStart = new WaitUntil( () => curSample.start <= AudioManager.Inst.Position - videoOffset );
    //    WaitUntil waitSampleEnd   = new WaitUntil( () => curSample.end   <= AudioManager.Inst.Position - videoOffset );
        
    //    yield return waitSampleStart;

    //    // Wait First Texture
    //    yield return new WaitUntil( () =>
    //    {
    //        if ( DataStorage.Inst.TryGetTexture( curSample.name, out Texture2D texture ) )
    //        {
    //            tf.sizeDelta = Global.Screen.GetRatio( texture );
    //            image.enabled = true;
    //            return true;
    //        }
    //        return false;
    //    } );

    //    while ( curIndex < sprites.Count )
    //    {
    //        yield return waitSampleStart;

    //        if ( DataStorage.Inst.TryGetTexture( curSample.name, out Texture2D texture ) )
    //             image.texture = texture;

    //        yield return waitSampleEnd;
    //        if ( ++curIndex < sprites.Count )
    //             curSample = sprites[curIndex];
    //    }
    //}

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
        StopAllCoroutines();
        isDestroy = false;
        image.enabled = false;
        vp.enabled = false;

        switch ( type )
        {
            case BackgroundType.Sprite:
            {
                //foreach ( Texture2D texture in textures.Values )
                //{
                //    DestroyImmediate( texture, true );
                //}
                //textures.Clear();
                //sprites.Clear();
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

                //if ( defaultImage.texture != image.texture )
                //     DestroyImmediate( image.texture, true );
            }
            break;
        }
    }

    public void Despawn()
    {
        isDestroy = true;
    }
}
