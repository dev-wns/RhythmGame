using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class DataStorage : Singleton<DataStorage>
{
    //[Header( "Network" )]
    //public static bool IsMultiPlaying { get; set; } = false;
    //public static USER_INFO? UserInfo { get; set; }
    //public static STAGE_INFO? StageInfo { get; set; }

    [Header( "Resource" )]
    private BMPLoader bitmapLoader = new ();
    private Dictionary<string/* name */, Texture2D>  loadedTextures = new ();
    private Dictionary<string/* name */, FMOD.Sound> loadedSounds   = new ();
    private List<Texture2D> textures = new (); // Default Texture
    private int             texIndex = 0;      // Default Texture

    //[Header( "Record Data" )]
    //public  static RecordData CurrentRecord => recordData;
    //private static RecordData recordData = new RecordData();

    [Header( "Origin Datas" )]
    public ReadOnlyCollection<Song>         Songs   { get; private set; } // 원본 음악 리스트
    public ReadOnlyCollection<Note>         Notes   { get; private set; }
    public ReadOnlyCollection<Timing>       Timings { get; private set; } // BPM Timing
    public ReadOnlyCollection<SpriteSample> Sprites { get; private set; } // 이미지로 이루어진 BGA의 재생 데이터
    public ReadOnlyCollection<KeySound>     Samples { get; private set; }

    // Song Parsing
    public bool IsSongParsing { get; private set; }
    public event Action<Song>                     OnSongParsed;
    public event Action<ReadOnlyCollection<Song>> OnSongsParsed;

    // Chart Parsing
    //public event Action<ReadOnlyCollection<Note>>         OnLoadedNote;
    //public event Action<ReadOnlyCollection<Timing>>       OnLoadedTiming;
    //public event Action<ReadOnlyCollection<SpriteSample>> OnLoadedSprite;
    //public event Action<ReadOnlyCollection<KeySound>>     OnLoadedSample;

    //[Header( "Converted Chart" )]
    //public List<KeySound> ConvertedSamples { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        LoadAssetsAsync( "DefaultTexture", ( Texture2D texture ) => textures.Add( texture ) );

        //NowPlaying.OnLoadAsync += () =>
        //{
        //    for ( int i = 0; i < Samples.Count; i++ )
        //          LoadSound( Samples[i].name );
        //};

        //NowPlaying.OnLoad += async () => 
        //{
        //    for ( int i = 0; i < Sprites.Count; i++ )
        //          await LoadTexture( Sprites[i] );
        //};

        //NowPlaying.OnLoadEnd += () =>
        //{
        //    // 특정모드 선택으로 키음이 추가될 수 있다. ( 시간 오름차순 정렬 )
        //    ConvertedSamples.Sort( delegate ( KeySound _left, KeySound _right )
        //    {
        //        if      ( _left.time > _right.time ) return 1;
        //        else if ( _left.time < _right.time ) return -1;
        //        else                                 return 0;
        //    } );    
        //};
    }

    private void OnApplicationQuit()
    {
        Release();
    }

    public void Release()
    {
        foreach ( var texture in loadedTextures )
            DestroyImmediate( texture.Value, true );

        foreach ( var sound in loadedSounds )
            AudioManager.Inst.Release( sound.Value );

        loadedTextures.Clear();
        loadedSounds.Clear();

        Notes   = null;
        Timings = null;
        Sprites = null;
        Samples = null;
        //ConvertedSamples = null;
        Debug.Log( $"DataStorage Release" );
    }

    /// <summary> 음악의 기본 정보 파싱 ( 플레이하기 전 프리스타일 등에서 표시될 최소 정보 ) </summary>
    public bool LoadSongs()
    {
        IsSongParsing = false;
        // StreamingAsset\\Songs 안의 모든 파일 순회하며 파싱
        List<Song> newSongs = new List<Song>();
        string[] files = Global.IO.Path.GetFilesInSubDirectories( Global.IO.Path.SoundDirectory, "*.osu" );
        for ( int i = 0; i < files.Length; i++ )
        {
            using ( FileParser parser = new FileParser() )
            {
                if ( parser.TryParse( files[i], out Song newSong ) )
                     newSongs.Add( newSong );

                OnSongParsed?.Invoke( newSong );
            }
        }

        newSongs.Sort( ( _left, _right ) => _left.title.CompareTo( _right.title ) );
        for ( int i = 0; i < newSongs.Count; i++ )
        {
            Song song   = newSongs[i];
            song.index  = i;
            newSongs[i] = song;
        }
        Songs = new ReadOnlyCollection<Song>( newSongs );
        OnSongsParsed?.Invoke( Songs );

        IsSongParsing = true;

        // 파일 수정하고 싶을 때 사용
        //for ( int i = 0; i < OriginSongs.Count; i++ )
        //{
        //    using ( FileParser parser = new FileParser() )
        //        parser.ReWrite( OriginSongs[i] );
        //}

        return Songs.Count != 0;
    }

    public bool LoadChart( in Song _song )
    {
        using ( FileParser parser = new FileParser() )
        {
            if ( parser.TryParse( _song.filePath, out List<Note>         notes,
                                                  out List<Timing>       timings,
                                                  out List<SpriteSample> sprites,
                                                  out List<KeySound>     samples ) )
            {
                Notes   = new ReadOnlyCollection<Note>( notes );
                Timings = new ReadOnlyCollection<Timing>( timings );
                Sprites = new ReadOnlyCollection<SpriteSample>( sprites );
                Samples = new ReadOnlyCollection<KeySound>( samples );
                
                return true;
            }
        }

        return false;
    }

    public void LoadAssetsAsync<T>( string _label, Action<T> _OnCompleted ) where T : UnityEngine.Object
    {
        AsyncOperationHandle<IList<IResourceLocation>> locationHandle = Addressables.LoadResourceLocationsAsync( _label, typeof( T ) );
        locationHandle.Completed += ( AsyncOperationHandle<IList<IResourceLocation>> _handle ) =>
        {
            if ( _handle.Status != AsyncOperationStatus.Succeeded )
            {
                Debug.LogWarning( "Load Location Async Failed" );
                Addressables.Release( locationHandle );
                return;
            }

            foreach ( IResourceLocation location in _handle.Result )
            {
                AsyncOperationHandle<T> assetHandle = Addressables.LoadAssetAsync<T>( location );
                assetHandle.Completed += ( AsyncOperationHandle<T> _handle ) =>
                {
                    if ( _handle.Status != AsyncOperationStatus.Succeeded )
                    {
                        Debug.LogError( "Load Asset Async Failed" );
                        Addressables.Release( assetHandle );
                        return;
                    }

                    _OnCompleted?.Invoke( _handle.Result );
                    Addressables.Release( assetHandle );
                };
            }

            Addressables.Release( locationHandle );
        };
    }

    public bool GetSound( string _name, out FMOD.Sound _sound ) => loadedSounds.TryGetValue( _name, out _sound );
    public void LoadSound( string _name )
    {
        if ( !loadedSounds.ContainsKey( _name ) )
        {
            string path = Path.Combine( Browser.Inst.CurrentSong.directory, _name );
            if ( AudioManager.Inst.Load( path, out FMOD.Sound sound ) )
                 loadedSounds.Add( _name, sound );
        }
    }

    public bool GetTexture( string _name, out Texture2D _tex ) => loadedTextures.TryGetValue( _name, out _tex );
    public Texture2D GetDefaultTexture() => textures[( ++texIndex < textures.Count ? texIndex : texIndex = 0 )];
    public async UniTask LoadTextureAsync( SpriteSample _sprite, Action _OnCompleted = null )
    {
        var path = Path.Combine( Browser.Inst.CurrentSong.directory, _sprite.name );
        if ( !File.Exists( path ) || loadedTextures.ContainsKey( _sprite.name ) )
             return;
        
        if ( Path.GetExtension( path ) == ".bmp" )
        {
            // 비트맵 파일은 UnityWebRequest로 로딩되지 않음( 외부 도움 )
            BMPImage  img = bitmapLoader.LoadBMP( path );
            Texture2D tex = img.ToTexture2D( TextureFormat.RGBA32 );

            // 전경일때 대부분의 비트맵 배경은 검은색으로 되어있음
            if ( _sprite.type == SpriteType.Foreground )
            {
                Color32[] pixels = tex.GetPixels32();
                for ( int i = 0; i < pixels.Length; i++ )
                {
                    if ( pixels[i].r == 0 && pixels[i].g == 0 && pixels[i].b == 0 )
                         pixels[i].a = 0;
                }
                tex.SetPixels32( pixels );
                tex.Apply();
            }

            loadedTextures.Add( _sprite.name, tex );
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
                    await www.SendWebRequest();

                    if ( www.result == UnityWebRequest.Result.ConnectionError ||
                         www.result == UnityWebRequest.Result.ProtocolError )
                         throw new Exception( www.error );

                    loadedTextures.Add( _sprite.name, handler.texture );
                }
            }
        }

        _OnCompleted?.Invoke();
    }

    public async UniTask<Texture2D> LoadTextureAsync( string _dir, string _name, CancellationToken _token )
    {
        string path = Path.Combine( _dir, _name );
        return await LoadTextureAsync( path, SpriteType.Background, _token );
    }

    public async UniTask<Texture2D> LoadTextureAsync( string _dir, SpriteSample _sprite, CancellationToken _token )
    {
        string path = Path.Combine( _dir, _sprite.name );
        return await LoadTextureAsync( path, _sprite.type, _token );
    }

    public async UniTask<Texture2D> LoadTextureAsync( string _path, SpriteType _type, CancellationToken _token )
    {
        if ( !File.Exists( _path ) ) return null;

        Texture2D tex = null;
        if ( Path.GetExtension( _path ) == ".bmp" )
        {
            // 비트맵 파일은 UnityWebRequest에서 읽히지 않음( 외부 도움 )
            BMPImage img = bitmapLoader.LoadBMP( _path );
            tex = img.ToTexture2D( TextureFormat.RGB24 );

            // 전경일때 대부분의 비트맵 배경은 검은색으로 되어있음
            if ( _type == SpriteType.Foreground )
            {
                Color32[] pixels = tex.GetPixels32();
                for ( int i = 0; i < pixels.Length; i++ )
                {
                    if ( pixels[i].r == 0 && pixels[i].g == 0 && pixels[i].b == 0 )
                        pixels[i].a = 0;
                }
                tex.SetPixels32( pixels );
                tex.Apply();
            }
        }
        else
        {
            // 그 외 JPG, JPEG, PNG 등 이미지 파일 로딩
            using ( UnityWebRequest www = UnityWebRequestTexture.GetTexture( _path, true ) )
            {
                www.method = UnityWebRequest.kHttpVerbGET;
                using ( DownloadHandlerTexture handler = new DownloadHandlerTexture() )
                {
                    www.downloadHandler = handler;
                    await www.SendWebRequest().WithCancellation( _token );

                    if ( www.result == UnityWebRequest.Result.ConnectionError ||
                         www.result == UnityWebRequest.Result.ProtocolError )
                         throw new Exception( www.error );

                    tex = handler.texture;
                }
            }
        }

        return tex;
    }
}
