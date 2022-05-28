using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Video;

public enum BackgroundType { None, Video, Sprite, Image, }

public class BGASystem : MonoBehaviour
{
    private InGame scene;
    public RawImage background, foreground;

    [Header( "Video" )]
    public VideoPlayer vp;
    public RenderTexture renderTexture;

    public GameDebug gameDebug;

    private struct PlaySpriteSample
    {
        public string name;
        public double start, end;
        public Texture2D tex;
        public PlaySpriteSample( SpriteSample _sample, Texture2D _tex )
        {
            name = _sample.name;
            start = _sample.start;
            end = _sample.end;
            tex = _tex;
        }
    }
    private List<PlaySpriteSample> backgrounds = new List<PlaySpriteSample>();
    private List<PlaySpriteSample> foregrounds = new List<PlaySpriteSample>();
    private Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

    private Color color;

    private int curBackIndex = 0;
    private int curForeIndex = 0;

    private BackgroundType type;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnSystemInitialize += Initialize;
        scene.OnReLoad += ReLoad;

        color = new Color( 1f, 1f, 1f, GameSetting.BGAOpacity * .01f );

        background.color = Color.clear;
        foreground.color = Color.clear;
    }

    private void OnDestroy()
    {
        ClearRenderTexture();
        NowPlaying.Inst.OnStart -= PlayVideo;
        NowPlaying.Inst.OnPause -= OnPause;

        foreach ( var tex in textures )
        {
            DestroyImmediate( tex.Value );
        }
    }

    private void ClearRenderTexture()
    {
        RenderTexture rt = RenderTexture.active;
        RenderTexture.active = vp.targetTexture;
        GL.Clear( true, true, Color.black );
        RenderTexture.active = rt;
    }


    private void Initialize( in Chart _chart )
    {
        type = GameSetting.BGAOpacity <= .1f        ? BackgroundType.None   :
               NowPlaying.Inst.CurrentSong.hasVideo ? BackgroundType.Video  :
               _chart.sprites.Count > 0             ? BackgroundType.Sprite : 
                                                      BackgroundType.Image;

        gameDebug?.SetBackgroundType( type );

        switch ( type )
        {
            case BackgroundType.None:
            {
                gameObject.SetActive( false );
            } break;

            case BackgroundType.Video:
            {
                StartCoroutine( LoadVideo() );
                NowPlaying.Inst.OnStart += PlayVideo;
                NowPlaying.Inst.OnPause += OnPause;

                foreground.gameObject.SetActive( false );
                Debug.Log( "Background Type : Video" );
            } break;

            case BackgroundType.Sprite:
            {
                scene.OnGameStart += SpriteProcess;
                foreground.gameObject.SetActive( true );
                StartCoroutine( LoadSamples( _chart.sprites ) );

                Debug.Log( "Background Type : Sprite" );
            } break;

            case BackgroundType.Image:
            {
                var path = NowPlaying.Inst.CurrentSong.imagePath;
                if ( path == string.Empty )
                {
                    gameObject.SetActive( false );
                }
                else
                {
                    StartCoroutine( LoadBackground( NowPlaying.Inst.CurrentSong.imagePath ) );
                }
                NowPlaying.Inst.IsLoadBackground = true;
                Debug.Log( "Background Type : Image" );
            } break;
        }
    }

    private void PlayVideo()
    {
        background.texture = renderTexture;
        StartCoroutine( WaitVideo() );
    }

    private IEnumerator WaitVideo()
    {
        yield return new WaitUntil( () => NowPlaying.Playback >= NowPlaying.Inst.CurrentSong.videoOffset * .001d );
        vp.Play();
        Debug.Log( $"Start Video // Playback {NowPlaying.Playback} // Offset {NowPlaying.Inst.CurrentSong.videoOffset * .001d}" );
    }

    private void ReLoad()
    {
        ClearRenderTexture();
        switch ( type )
        {
            case BackgroundType.None:
            case BackgroundType.Image:
            break;

            case BackgroundType.Video:
            {
                background.texture = Texture2D.blackTexture;
                vp.frame = 0;
            } break;

            case BackgroundType.Sprite:
            {
                background.texture = Texture2D.blackTexture;
                foreground.texture = Texture2D.blackTexture;
                curBackIndex = 0;
                curForeIndex = 0;
            } break;
        }
    }

    private void OnPause( bool _isPause )
    {
        if ( _isPause ) vp.Pause();
        else
        {
            vp.Play();
        }
    }

    private IEnumerator LoadVideo()
    {
        vp.enabled = true;
        vp.url = @$"{NowPlaying.Inst.CurrentSong.videoPath}";
        vp.targetTexture = renderTexture;
        background.texture = renderTexture;
        background.color = color;
        
        vp.Prepare();
        yield return new WaitUntil( () => vp.isPrepared );

        NowPlaying.Inst.IsLoadBackground = true;
    }

    private void SpriteProcess()
    {
        StartCoroutine( BackProcess() );
        StartCoroutine( ForeProcess() );
    }

    private IEnumerator BackProcess()
    {   
        PlaySpriteSample curSample = new PlaySpriteSample();

        if ( backgrounds.Count > 0 )
             curSample = backgrounds[curBackIndex];

        Debug.Log( "back" + backgrounds.Count );
        WaitUntil waitSampleStart = new WaitUntil( () => curSample.start <= NowPlaying.Playback );
        WaitUntil waitSampleEnd   = new WaitUntil( () => curSample.end   <= NowPlaying.Playback );

        yield return waitSampleStart;
        background.color = color;
        
        while ( curBackIndex < backgrounds.Count )
        {
            yield return waitSampleStart;
            background.texture = curSample.tex;
            background.rectTransform.sizeDelta = Globals.GetScreenRatio( curSample.tex, new Vector2( Screen.width, Screen.height ) );

            yield return waitSampleEnd;
            if ( ++curBackIndex < backgrounds.Count )
                 curSample = backgrounds[curBackIndex];
        }
    }

    private IEnumerator ForeProcess()
    {
        PlaySpriteSample curSample = new PlaySpriteSample();

        if ( foregrounds.Count > 0 )
             curSample = foregrounds[curForeIndex];
        else if ( foregrounds.Count == 0 )
        {
            foreground.gameObject.SetActive( false );
            yield break;
        }

        Debug.Log( "fore" + foregrounds.Count );
        WaitUntil waitSampleStart = new WaitUntil( () => curSample.start <= NowPlaying.Playback );
        WaitUntil waitSampleEnd   = new WaitUntil( () => curSample.end   <= NowPlaying.Playback );

        yield return waitSampleStart;
        foreground.color = color;

        while ( curForeIndex < foregrounds.Count )
        {
            yield return waitSampleStart;
            foreground.texture = curSample.tex;
            foreground.rectTransform.sizeDelta = Globals.GetScreenRatio( curSample.tex, new Vector2( Screen.width, Screen.height ) );

            yield return waitSampleEnd;
            if ( ++curForeIndex < foregrounds.Count )
                 curSample = foregrounds[curForeIndex];
        }
    }

    public IEnumerator LoadSamples( ReadOnlyCollection<SpriteSample> _samples )
    {
        var dir = System.IO.Path.GetDirectoryName( NowPlaying.Inst.CurrentSong.filePath );
        for ( int i = 0; i < _samples.Count; i++ )
        {
            yield return StartCoroutine( LoadSample( dir, _samples[i] ) );
        }

        backgrounds.Sort( delegate ( PlaySpriteSample _A, PlaySpriteSample _B )
        {
            if ( _A.start > _B.start )      return 1;
            else if ( _A.start < _B.start ) return -1;
            else                            return 0;
        } );

        foregrounds.Sort( delegate ( PlaySpriteSample _A, PlaySpriteSample _B )
        {
            if ( _A.start > _B.start )      return 1;
            else if ( _A.start < _B.start ) return -1;
            else                            return 0;
        } );

        NowPlaying.Inst.IsLoadBackground = true;
    }

    public IEnumerator LoadSample( string _dir, SpriteSample _sample )
    {
        Texture2D tex;
        if ( textures.ContainsKey( _sample.name ) )
        {
            tex = textures[_sample.name];
        }
        else
        {
            var path = @System.IO.Path.Combine( _dir, _sample.name );
            if ( !System.IO.File.Exists( path ) ) 
                 yield break;
            
            var ext = System.IO.Path.GetExtension( path );
            if ( ext.Contains( ".bmp" ) )
            {
                BMPLoader loader = new BMPLoader();
                BMPImage img = loader.LoadBMP( path );
                tex = img.ToTexture2D();
            }
            else
            {
                using ( UnityWebRequest www = UnityWebRequestTexture.GetTexture( path ) )
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

            textures.Add( _sample.name, tex );
            gameDebug?.SetBackgroundType( type, textures.Count );
        }

        switch ( _sample.type )
        {
            case SpriteType.Background:
            backgrounds.Add( new PlaySpriteSample( _sample, tex ) );
            break;

            case SpriteType.Foreground:
            foregrounds.Add( new PlaySpriteSample( _sample, tex ) );
            break;
        }

        gameDebug?.SetSpriteCount( backgrounds.Count, foregrounds.Count );
    }

    public IEnumerator LoadBackground( string _path )
    {
        if ( !System.IO.File.Exists( _path ) )
            yield break;
        
        Texture2D tex;
        var ext = System.IO.Path.GetExtension( _path );
        if ( ext.Contains( ".bmp" ) )
        {
            BMPLoader loader = new BMPLoader();
            BMPImage img = loader.LoadBMP( _path );
            tex = img.ToTexture2D();
        }
        else
        {
            using ( UnityWebRequest www = UnityWebRequestTexture.GetTexture( _path ) )
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
        textures.Add( "BackgroundImage", tex );
        background.color = color;
        background.texture = tex;
        background.rectTransform.sizeDelta = Globals.GetScreenRatio( tex, new Vector2( Screen.width, Screen.height ) );
    }
}

