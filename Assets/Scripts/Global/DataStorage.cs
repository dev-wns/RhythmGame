using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public struct RecordData
{
    public double score;
    public double accuracy;
    public GameRandom random;
    public float  pitch;
    public string date;
}

public class DataStorage : Singleton<DataStorage>
{
    [Header( "Network" )]
    public static bool IsMultiPlaying { get; set; } = false;
    public static USER_INFO? UserInfo { get; set; }
    public static STAGE_INFO? StageInfo { get; set; }

    [Header( "Parsing Data" )]
    public static ReadOnlyCollection<Song>         OriginSongs { get; private set; } // 원본 음악 리스트
    public static ReadOnlyCollection<Note>         Notes       { get; private set; }
    public static ReadOnlyCollection<Timing>       Timings     { get; private set; }
    public static ReadOnlyCollection<KeySound>     Samples     { get; private set; } // 배경음
    public static ReadOnlyCollection<SpriteSample> Backgrounds { get; private set; }
    public static ReadOnlyCollection<SpriteSample> Foregrounds { get; private set; }

    [Header( "Resource" )]
    private BMPLoader bitmapLoader = new ();
    private Dictionary<string/* name */, Texture2D>  loadedTextures = new ();
    private Dictionary<string/* name */, FMOD.Sound> loadedSounds   = new ();
    private List<Texture2D> textures = new (); // Default Texture
    private int             texIndex = 0;      // Default Texture

    [Header( "Result Data" )]
    public  static RecordData CurrentRecord => recordData;
    private static RecordData recordData = new RecordData();

    public static event Action<Song> OnParsing;

    protected override void Awake()
    {
        base.Awake();

        NowPlaying.OnAsyncInit += () =>
        {
            // FMOD Sound는 Thread에서 로딩 가능
            for ( int i = 0; i < Samples.Count; i++ )
            {
                LoadSound( Samples[i].name );
                Debug.LogError( $"Sound Load( {Samples[i].name} )" );
            }
        };

        // UnityWebRequest( Coroutine ) 사용
        NowPlaying.OnPostInit += () => StartCoroutine( LoadTextures() );

        // Default Resource
        LoadAssetsAsync( "DefaultTexture", ( Texture2D texture ) => textures.Add( texture ) );
    }

    private void OnApplicationQuit()
    {
        Release();
    }

    public void Release()
    {
        StopAllCoroutines();

        foreach ( var texture in loadedTextures )
            DestroyImmediate( texture.Value, true );

        foreach ( var sound in loadedSounds )
            AudioManager.Inst.Release( sound.Value );

        loadedTextures.Clear();
        loadedSounds.Clear();
        Debug.Log( $"DataStorage Release" );
    }

    #region Parsing
    public bool LoadSongs()
    {
        // StreamingAsset\\Songs 안의 모든 파일 순회하며 파싱
        List<Song> newSongs = new List<Song>();
        string[] files = Global.Path.GetFilesInSubDirectories( Global.Path.SoundDirectory, "*.osu" );
        for ( int i = 0; i < files.Length; i++ )
        {
            using ( FileParser parser = new FileParser() )
            {
                if ( parser.TryParse( files[i], out Song newSong ) )
                     newSongs.Add( newSong );

                OnParsing?.Invoke( newSong );
            }
        }

        newSongs.Sort( ( _left, _right ) => _left.title.CompareTo( _right.title ) );
        for ( int i = 0; i < newSongs.Count; i++ )
        {
            Song song   = newSongs[i];
            song.index  = i;
            newSongs[i] = song;
        }
        OriginSongs = new ReadOnlyCollection<Song>( newSongs );

        // 파일 수정하고 싶을 때 사용
        //for ( int i = 0; i < OriginSongs.Count; i++ )
        //{
        //    using ( FileParser parser = new FileParser() )
        //        parser.ReWrite( OriginSongs[i] );
        //}

        return true;
    }

    public void LoadChart()
    {
        using ( FileParser parser = new FileParser() )
        {
            if ( parser.TryParse( NowPlaying.CurrentSong.filePath, out Chart chart ) )
            {
                Notes       = chart.notes;
                Timings     = chart.timings;
                Samples     = chart.samples;
                Backgrounds = chart.backgrounds;
                Foregrounds = chart.foregrounds;
            }
            else
            {
                // Goto FreeStyle
            }
        }
    }
    #endregion

    #region Addressable
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
    #endregion

    #region Sound
    public bool GetSound( string _name, out FMOD.Sound _sound ) => loadedSounds.TryGetValue( _name, out _sound );
    public void LoadSound( string _name )
    {
        if ( !loadedSounds.ContainsKey( _name ) )
        {
            string path = Path.Combine( NowPlaying.CurrentSong.directory, _name );
            if ( AudioManager.Inst.Load( path, out FMOD.Sound sound ) )
                 loadedSounds.Add( _name, sound );
        }
    }
    #endregion

    #region Texture
    public void LoadTexture( string _name, Action _OnCompleted = null ) => StartCoroutine( LoadExternalTexture( new SpriteSample( _name ), _OnCompleted ) );
    public bool GetTexture( string _name, out Texture2D _tex ) => loadedTextures.TryGetValue( _name, out _tex );
    public Texture2D GetDefaultTexture() => textures[( ++texIndex < textures.Count ? texIndex : texIndex = 0 )];
    private IEnumerator LoadTextures()
    {
        // 스프라이트 배경 로딩 ( UnityWebRequest, Main Thread에서 사용 )
        for ( int i = 0; i < Backgrounds.Count; i++ )
              yield return StartCoroutine( LoadExternalTexture( Backgrounds[i] ) );

        for ( int i = 0; i < Foregrounds.Count; i++ )
              yield return StartCoroutine( LoadExternalTexture( Foregrounds[i] ) );
    }

    private IEnumerator LoadExternalTexture( SpriteSample _sprite, Action _OnCompleted = null )
    {
        var path = Path.Combine( NowPlaying.CurrentSong.directory, _sprite.name );
        // 파일이 없거나, 이미 로딩된 파일일 경우
        if ( !File.Exists( path ) || loadedTextures.ContainsKey( _sprite.name ) )
             yield break;
        
        if ( Path.GetExtension( path ) == ".bmp" )
        {
            // 비트맵 파일은 런타임에서 읽히지 않음( 외부 도움 )
            BMPImage img = bitmapLoader.LoadBMP( path );
            loadedTextures.Add( _sprite.name, img.ToTexture2D( TextureFormat.RGB24 ) );
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

                    loadedTextures.Add( _sprite.name, handler.texture );
                }
            }
        }

        _OnCompleted?.Invoke();
    }
    #endregion

    #region Result Data
    public RecordData CreateNewRecord()
    {
        var newRecord = new RecordData()
        {
            score    = Judgement.CurrentResult.Score,
            accuracy = Judgement.CurrentResult.Accuracy,
            random   = GameSetting.CurrentRandom,
            pitch    = GameSetting.CurrentPitch,
            date     = DateTime.Now.ToString( "yyyy. MM. dd @ hh:mm:ss tt" )
        };

        if ( recordData.score > newRecord.score )
             return recordData;

        string path = Path.Combine( Global.Path.RecordDirectory, $"{Path.GetFileNameWithoutExtension( NowPlaying.CurrentSong.filePath )}.json" );
        try
        {
            FileMode mode = File.Exists( path ) ? FileMode.Truncate : FileMode.Create;
            using ( FileStream stream = new FileStream( path, mode ) )
            {
                using ( StreamWriter writer = new StreamWriter( stream, System.Text.Encoding.UTF8 ) )
                {
                    writer.Write( JsonConvert.SerializeObject( newRecord, Formatting.Indented ) );
                }
            }
            recordData = newRecord;
        }
        catch ( Exception )
        {
            if ( File.Exists( path ) )
                 File.Delete( path );

            Debug.LogError( $"Record Write Error : {path}" );
        }

        return newRecord;
    }

    public bool UpdateRecord()
    {
        string path = Path.Combine( Global.Path.RecordDirectory, $"{Path.GetFileNameWithoutExtension( NowPlaying.CurrentSong.filePath )}.json" );
        if ( !File.Exists( path ) )
        {
            recordData = new RecordData();
            return false;
        }

        using ( StreamReader stream = new StreamReader( path ) )
        {
            try
            {
                recordData = JsonConvert.DeserializeObject<RecordData>( stream.ReadToEnd() );
            }
            catch ( Exception )
            {
                Debug.LogWarning( $"Record Deserialize Error : {NowPlaying.CurrentSong.filePath}" );
                return false;
            }
        }

        return true;
    }
    #endregion
}
