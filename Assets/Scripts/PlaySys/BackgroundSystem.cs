using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Video;

public class BackgroundSystem : MonoBehaviour
{
    private InGame scene;
    private RawImage image;

    [Header( "Video" )]
    private VideoPlayer vp;
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
    private int sampleCount;
    private List<PlaySpriteSample> samples = new List<PlaySpriteSample>();
    private Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
    private PlaySpriteSample curSample;
    private int curIndex;

    private void PlayVideo()
    {
        image.texture = renderTexture;
        vp.Play();
    }

    private void ReLoad()
    {
        image.texture = Texture2D.blackTexture;
        vp.frame = 0;
    }

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnSystemInitialize += Initialize;

        image = GetComponent<RawImage>();
        image.texture = Texture2D.blackTexture;

        vp = GetComponent<VideoPlayer>();
    }

    private void OnDestroy()
    {
        NowPlaying.Inst.OnStart -= PlayVideo;
        NowPlaying.Inst.OnPause -= OnPause;
        if ( canDestroyTex )
        {
            if ( image.texture && image.texture != Texture2D.blackTexture )
                DestroyImmediate( image.texture );

            foreach ( var tex in textures )
            {
                DestroyImmediate( tex.Value );
            }
        }
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
                vp.url = @$"{NowPlaying.Inst.CurrentSong.videoPath}";
                NowPlaying.Inst.OnStart += PlayVideo;
                NowPlaying.Inst.OnPause += OnPause;
                scene.OnReLoad += ReLoad;

                NowPlaying.Inst.IsLoadSpriteSample = false;
                Debug.Log( "Background Type : Video" );
            }
            else
            {
                sampleCount = _chart.spriteSamples.Count;
                bool hasSprites = _chart.spriteSamples.Count > 0 ? true : false;
                if ( hasSprites )
                {
                    // Sprites
                    scene.OnGameStart += () => StartCoroutine( Process() );
                    StartCoroutine( LoadSamples( _chart.spriteSamples ) );
                    
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
                    NowPlaying.Inst.IsLoadSpriteSample = false;
                    Debug.Log( "Background Type : Image" );
                }
                vp.enabled = false;
            }

            image.color = new Color( 1f, 1f, 1f, GameSetting.BGAOpacity * .01f );
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

    private IEnumerator Process()
    {
        if ( samples.Count > 0 )
        {
            curSample = samples[curIndex];
            image.texture = curSample.tex;
        }

        WaitUntil waitNextNote = new WaitUntil( () => curSample.end <= NowPlaying.Playback );
        while ( curIndex < samples.Count )
        {
            yield return waitNextNote;

            image.texture = curSample.tex;

            if ( ++curIndex < samples.Count )
                 curSample = samples[curIndex];
        }
    }

    public IEnumerator LoadSamples( ReadOnlyCollection<SpriteSample> _samples )
    {
        var dir = System.IO.Path.GetDirectoryName( NowPlaying.Inst.CurrentSong.filePath );
        for ( int i = 0; i < _samples.Count; i++ )
        {
            yield return StartCoroutine( LoadSample( dir, _samples[i] ) );
        }

        samples.Sort( delegate ( PlaySpriteSample _A, PlaySpriteSample _B )
        {
            if ( _A.start > _B.start ) return 1;
            else if ( _A.start < _B.start ) return -1;
            else return 0;
        } );

        NowPlaying.Inst.IsLoadSpriteSample = false;
    }

    public IEnumerator LoadSample( string _dir, SpriteSample _sample )
    {
        if ( textures.ContainsKey( _sample.name ) )
        {
            samples.Add( new PlaySpriteSample( _sample, textures[_sample.name] ) );
            yield break;
        }

        var path = @System.IO.Path.Combine( _dir, _sample.name );
        bool isExist = System.IO.File.Exists( path );
        if ( isExist )
        {
            Texture2D tex;
            var ext = System.IO.Path.GetExtension( path );
            if ( ext.Contains( ".bmp" ) )
            {
                BMPLoader loader = new BMPLoader();
                BMPImage img = loader.LoadBMP( path );
                tex = img.ToTexture2D();
                samples.Add( new PlaySpriteSample( _sample, tex ) );
                textures.Add( _sample.name, tex );
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
                        samples.Add( new PlaySpriteSample( _sample, tex ) );
                        textures.Add( _sample.name, tex );
                    }
                }
            }

            image.rectTransform.sizeDelta = Globals.GetScreenRatio( tex, new Vector2( Screen.width, Screen.height ) );
            canDestroyTex = true;
        }
    }

    public IEnumerator LoadBackground( string _path )
    {
        bool isExist = System.IO.File.Exists( _path );
        if ( isExist )
        {
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

            image.texture = tex;
            image.rectTransform.sizeDelta = Globals.GetScreenRatio( tex, new Vector2( Screen.width, Screen.height ) );
            canDestroyTex = true;
        }
    }
}
