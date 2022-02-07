using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Video;

public class SpriteSystem : MonoBehaviour
{
    private InGame scene;
    public RawImage background, foreground;

    [Header( "Video" )]
    public VideoPlayer vp;
    public RenderTexture renderTexture;
    private bool canDestroyTex = false;

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
    private void PlayVideo()
    {
        background.texture = renderTexture;
        vp.Play();
    }

    private void ReLoad()
    {
        background.texture = Texture2D.blackTexture;
        vp.frame = 0;
    }

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnSystemInitialize += Initialize;

        color = new Color( 1f, 1f, 1f, GameSetting.BGAOpacity * .01f );

        background.color = Color.clear;
        foreground.color = Color.clear;
    }

    private void OnDestroy()
    {
        NowPlaying.Inst.OnStart -= PlayVideo;
        NowPlaying.Inst.OnPause -= OnPause;

        foreach ( var tex in textures )
        {
            DestroyImmediate( tex.Value );
        }
    }

    private IEnumerator LoadVideo()
    {
        background.color = color;
        background.texture = renderTexture;
        vp.targetTexture   = renderTexture;

        vp.url = @$"{NowPlaying.Inst.CurrentSong.videoPath}";
        vp.Prepare();
        while ( !vp.isPrepared )
            yield return null;

        NowPlaying.Inst.IsLoadBackground = false;
    }

    private void Initialize( in Chart _chart )
    {
        bool isEnabled = GameSetting.BGAOpacity <= .1f ? false : true;
        if ( isEnabled )
        {
            bool hasVideo = NowPlaying.Inst.CurrentSong.hasVideo;
            if ( hasVideo )
            {
                // Video
                StartCoroutine( LoadVideo() );
                NowPlaying.Inst.OnStart += PlayVideo;
                NowPlaying.Inst.OnPause += OnPause;
                scene.OnReLoad += ReLoad;

                foreground.gameObject.SetActive( false );
                Debug.Log( "Background Type : Video" );
            }
            else
            {
                bool hasSprites = _chart.sprites.Count > 0 ? true : false;
                if ( hasSprites )
                {
                    // Sprites
                    scene.OnGameStart += Process;
                    StartCoroutine( LoadSamples( _chart.sprites ) );

                    Debug.Log( "Background Type : Sprite" );
                }
                else
                {
                    // Image
                    var path = NowPlaying.Inst.CurrentSong.imagePath;

                    if ( path == string.Empty )
                    {
                        gameObject.SetActive( false );
                    }
                    else
                    {
                        StartCoroutine( LoadBackground( NowPlaying.Inst.CurrentSong.imagePath ) );
                    }
                    foreground.gameObject.SetActive( false );
                    NowPlaying.Inst.IsLoadBackground = false;
                    Debug.Log( "Background Type : Image" );
                }
                vp.enabled = false;
            }
        }
        else
        {
            gameObject.SetActive( false );
        }
    }

    private void OnPause( bool _isPause )
    {
        if ( _isPause ) vp.Pause();
        else vp.Play();
    }

    private void Process()
    {
        StartCoroutine( BackProcess() );
        StartCoroutine( ForeProcess() );
    }

    private IEnumerator BackProcess()
    {   
        int curIndex = 0;
        PlaySpriteSample curSample = new PlaySpriteSample();

        if ( backgrounds.Count > 0 )
             curSample = backgrounds[curIndex];

        Debug.Log( "back" + backgrounds.Count );
        WaitUntil waitSampleStart = new WaitUntil( () => curSample.start <= NowPlaying.Playback );
        WaitUntil waitSampleEnd   = new WaitUntil( () => curSample.end   <= NowPlaying.Playback );

        yield return waitSampleStart;
        background.color = color;
        
        while ( curIndex < backgrounds.Count )
        {
            yield return waitSampleStart;
            background.texture = curSample.tex;
            background.rectTransform.sizeDelta = Globals.GetScreenRatio( curSample.tex, new Vector2( Screen.width, Screen.height ) );

            yield return waitSampleEnd;
            if ( ++curIndex < backgrounds.Count )
                 curSample = backgrounds[curIndex];
        }
    }

    private IEnumerator ForeProcess()
    {
        int curIndex = 0;
        PlaySpriteSample curSample = new PlaySpriteSample();

        if ( foregrounds.Count > 0 )
             curSample = foregrounds[curIndex];
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

        while ( curIndex < foregrounds.Count )
        {
            yield return waitSampleStart;
            foreground.texture = curSample.tex;
            foreground.rectTransform.sizeDelta = Globals.GetScreenRatio( curSample.tex, new Vector2( Screen.width, Screen.height ) );

            yield return waitSampleEnd;
            if ( ++curIndex < foregrounds.Count )
                 curSample = foregrounds[curIndex];
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
            if ( _A.start > _B.start ) return 1;
            else if ( _A.start < _B.start ) return -1;
            else return 0;
        } );

        foregrounds.Sort( delegate ( PlaySpriteSample _A, PlaySpriteSample _B )
        {
            if ( _A.start > _B.start ) return 1;
            else if ( _A.start < _B.start ) return -1;
            else return 0;
        } );

        NowPlaying.Inst.IsLoadBackground = false;
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
            canDestroyTex = true;
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
    }

    public IEnumerator LoadBackground( string _path )
    {
        bool isExist = System.IO.File.Exists( _path );
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
        canDestroyTex = true;
    }
}

