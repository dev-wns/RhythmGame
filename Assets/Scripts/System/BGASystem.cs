using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Networking;
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

    public GameDebug gameDebug;

    private struct SpriteBGA
    {
        public string name;
        public double start, end;
        public Texture2D tex;
        public SpriteBGA( SpriteSample _sample, Texture2D _tex )
        {
            name = _sample.name;
            start = _sample.start;
            end = _sample.end;
            tex = _tex;
        }
    }
    private List<SpriteBGA> backgrounds = new List<SpriteBGA>();
    private List<SpriteBGA> foregrounds = new List<SpriteBGA>();
    private Dictionary<string/*texture name*/, Texture2D> textures = new Dictionary<string, Texture2D>();

    private Color color;

    private int curBackIndex = 0;
    private int curForeIndex = 0;

    private BackgroundType type;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnSystemInitialize += Initialize;
        scene.OnReLoad           += ReLoad;
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

        foreach ( var tex in textures )
        {
            DestroyImmediate( tex.Value );
        }
        textures.Clear();
    }

    private void ClearRenderTexture()
    {
        RenderTexture rt = RenderTexture.active;
        RenderTexture.active = vp.targetTexture;
        GL.Clear( true, true, Color.black );
        RenderTexture.active = rt;
    }

    private void Initialize( Chart _chart )
    {
        if ( GameSetting.BGAOpacity == 0 )
        {
            transform.root.gameObject.SetActive( false );
            NowPlaying.Inst.IsLoadBGA = true;
            return;
        }

        type = NowPlaying.CurrentSong.hasVideo ? BackgroundType.Video  :
               _chart.sprites.Count > 0        ? BackgroundType.Sprite : 
                                                 BackgroundType.Image;
        switch ( type )
        {
            case BackgroundType.Video:
                StartCoroutine( LoadVideo() );
                scene.OnGameStart += PlayVideo;
                scene.OnPause     += OnPause;
                foreground.gameObject.SetActive( false );
            break;

            case BackgroundType.Sprite:
                scene.OnGameStart += SpriteProcess;
                foreground.gameObject.SetActive( true );
                StartCoroutine( LoadSamples( _chart.sprites ) );
            break;

            case BackgroundType.Image:
            if ( !System.IO.File.Exists( NowPlaying.CurrentSong.imagePath ) )
            {
                transform.root.gameObject.SetActive( false );
                NowPlaying.Inst.IsLoadBGA = true;
            }
            else
            {
                StartCoroutine( LoadBackground( NowPlaying.CurrentSong.imagePath ) );
            }
            break;
        }
        gameDebug?.SetBackgroundType( type );
    }

    private void PlayVideo()
    {
        background.texture = renderTexture;
        StartCoroutine( WaitVideo() );
    }

    private IEnumerator WaitVideo()
    {
        yield return new WaitUntil( () => NowPlaying.Playback >= ( GameSetting.DefaultSoundOffset + NowPlaying.CurrentSong.videoOffset + GameSetting.SoundOffset ) * .001d );
        vp.Play();
    }

    private void UpdatePitch( float _pitch )
    {
        if ( type != BackgroundType.Video )
             return;

        vp.playbackSpeed = _pitch;
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
                // background.texture = Texture2D.blackTexture;
                if ( !vp.isPlaying ) vp.Play();
                vp.Pause();
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
        if ( type != BackgroundType.Video )
             return;

        if ( _isPause ) vp?.Pause();
        else            StartCoroutine( WaitVideoTime() );
    }

    private IEnumerator WaitVideoTime()
    {
        // 노트가 위로 올라갔다 내려오는 효과 때문에
        // 시간이 역행하는지 확인 후 시작시간 타이밍을 기다린다.
        yield return new WaitUntil( () => NowPlaying.Playback < vp.time );
        yield return new WaitUntil( () => NowPlaying.Playback > vp.time );
        vp.Play();
    }

    private IEnumerator LoadVideo()
    {
        vp.enabled = true;
        vp.playbackSpeed = GameSetting.CurrentPitch;
        vp.url = @$"{NowPlaying.CurrentSong.videoPath}";
        vp.targetTexture = renderTexture;
        background.texture = renderTexture;
        background.color = color;
        
        vp.Prepare();
        yield return new WaitUntil( () => vp.isPrepared );

        if ( vp.isPlaying ) vp.Play();
        vp.Pause();
        vp.frame = 0;
        NowPlaying.Inst.IsLoadBGA = true;
    }

    private void SpriteProcess()
    {
        StartCoroutine( BackProcess() );
        StartCoroutine( ForeProcess() );
    }

    private IEnumerator BackProcess()
    {
        SpriteBGA curSample = new SpriteBGA();

        if ( backgrounds.Count > 0 )
             curSample = backgrounds[curBackIndex];

        WaitUntil waitSampleStart = new WaitUntil( () => curSample.start <= NowPlaying.Playback );
        WaitUntil waitSampleEnd   = new WaitUntil( () => curSample.end   <= NowPlaying.Playback );

        yield return waitSampleStart;
        background.color = color;
        
        while ( curBackIndex < backgrounds.Count )
        {
            yield return waitSampleStart;
            background.texture = curSample.tex;
            background.rectTransform.sizeDelta = Global.Math.GetScreenRatio( curSample.tex, new Vector2( Screen.width, Screen.height ) );

            yield return waitSampleEnd;
            if ( ++curBackIndex < backgrounds.Count )
                 curSample = backgrounds[curBackIndex];
        }
    }

    private IEnumerator ForeProcess()
    {
        SpriteBGA curSample = new SpriteBGA();

        if ( foregrounds.Count > 0 )
             curSample = foregrounds[curForeIndex];
        else if ( foregrounds.Count == 0 )
        {
            foreground.gameObject.SetActive( false );
            yield break;
        }

        WaitUntil waitSampleStart = new WaitUntil( () => curSample.start <= NowPlaying.Playback );
        WaitUntil waitSampleEnd   = new WaitUntil( () => curSample.end   <= NowPlaying.Playback );

        yield return waitSampleStart;
        foreground.color = color;

        while ( curForeIndex < foregrounds.Count )
        {
            yield return waitSampleStart;
            foreground.texture = curSample.tex;
            foreground.rectTransform.sizeDelta = Global.Math.GetScreenRatio( curSample.tex, new Vector2( Screen.width, Screen.height ) );

            yield return waitSampleEnd;
            if ( ++curForeIndex < foregrounds.Count )
                 curSample = foregrounds[curForeIndex];
        }
    }

    public IEnumerator LoadSamples( ReadOnlyCollection<SpriteSample> _samples )
    {
        var dir = System.IO.Path.GetDirectoryName( NowPlaying.CurrentSong.filePath );
        for ( int i = 0; i < _samples.Count; i++ )
        {
            if ( textures.ContainsKey( _samples[i].name ) )
            {
                Texture2D tex;
                tex = textures[_samples[i].name];
                switch ( _samples[i].type )
                {
                    case SpriteType.Background:
                    backgrounds.Add( new SpriteBGA( _samples[i], tex ) );
                    break;

                    case SpriteType.Foreground:
                    foregrounds.Add( new SpriteBGA( _samples[i], tex ) );
                    break;
                }

                gameDebug?.SetSpriteCount( backgrounds.Count, foregrounds.Count );
            }
            else 
                yield return StartCoroutine( LoadSample( dir, _samples[i] ) );
        }

        //backgrounds.Sort( delegate ( SpriteBGA _A, SpriteBGA _B )
        //{
        //    if ( _A.start > _B.start )      return 1;
        //    else if ( _A.start < _B.start ) return -1;
        //    else                            return 0;
        //} );

        //foregrounds.Sort( delegate ( SpriteBGA _A, SpriteBGA _B )
        //{
        //    if ( _A.start > _B.start )      return 1;
        //    else if ( _A.start < _B.start ) return -1;
        //    else                            return 0;
        //} );

        yield return YieldCache.WaitForEndOfFrame;
        NowPlaying.Inst.IsLoadBGA = true;
    }

    public IEnumerator LoadSample( string _dir, SpriteSample _sample )
    {
        Texture2D tex;
        var path = @System.IO.Path.Combine( _dir, _sample.name );
        if ( !System.IO.File.Exists( path ) ) 
             yield break;
        
        var ext = System.IO.Path.GetExtension( path );
        if ( ext.Contains( ".bmp" ) )
        {
            BMPLoader loader = new BMPLoader();
            BMPImage img = loader.LoadBMP( path );
            tex = img.ToTexture2D( TextureFormat.RGB24 );
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

                    // 이미지 배경 알파값이 빠져있거나 검정색인 2가지 경우가 있다.
                    // 쉐이더 블렌드 옵션을 사용하기위해 배경을 검은색으로 통일한다.
                    //tex = new Texture2D( handler.texture.width, handler.texture.height, TextureFormat.RGB24, false );
                    //if ( !tex.LoadImage( handler.data ) )
                    //{
                    //    throw new System.Exception( $"LoadImage Error : {www.error}" );
                    //}
                    tex = handler.texture;

                    // 다운로드 하기전에 Texture2D 설정할수있는 방법 찾기
                    // UnityWebRequest로 다운받은 데이터는 비관리 데이터라 지워줘야한다.
                    //DestroyImmediate( handler.texture );
                }
            }
        }

        textures.Add( _sample.name, tex );

        switch ( _sample.type )
        {
            case SpriteType.Background:
            backgrounds.Add( new SpriteBGA( _sample, tex ) );
            break;

            case SpriteType.Foreground:
            foregrounds.Add( new SpriteBGA( _sample, tex ) );
            break;
        }

        gameDebug?.SetBackgroundType( type, textures.Count );
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
        background.rectTransform.sizeDelta = Global.Math.GetScreenRatio( tex, new Vector2( Screen.width, Screen.height ) );
        NowPlaying.Inst.IsLoadBGA = true;
    }
}

