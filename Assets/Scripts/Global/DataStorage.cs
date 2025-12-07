using Cysharp.Threading.Tasks;
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

    [Header( "Resource" )]
    private BMPLoader bitmapLoader = new ();
    private Dictionary<string/* name */, Texture2D>  loadedTextures = new ();
    private Dictionary<string/* name */, FMOD.Sound> loadedSounds   = new ();
    private List<Texture2D> textures = new (); // Default Texture
    private int             texIndex = 0;      // Default Texture

    [Header( "Result Data" )]
    public  static RecordData CurrentRecord => recordData;
    private static RecordData recordData = new RecordData();

    [Header( "Origin Chart" )]
    public static ReadOnlyCollection<Note>         Notes   { get; private set; }
    public static ReadOnlyCollection<Timing>       Timings { get; private set; } // BPM Timing
    public static ReadOnlyCollection<SpriteSample> Sprites { get; private set; } // 이미지로 이루어진 BGA의 재생 데이터
    public static ReadOnlyCollection<KeySound>     Samples { get; private set; }
    
    [Header( "Converted Chart" )]
    public static List<KeySound> ConvertedSamples { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        LoadAssetsAsync( "DefaultTexture", ( Texture2D texture ) => textures.Add( texture ) );

        NowPlaying.OnLoadAsync += () =>
        {
            for ( int i = 0; i < Samples.Count; i++ )
                  LoadSound( Samples[i].name );
        };

        NowPlaying.OnLoad += async () => 
        {
            for ( int i = 0; i < Sprites.Count; i++ )
                  await LoadTexture( Sprites[i] );
        };

        NowPlaying.OnLoadEnd += () =>
        {
            // 특정모드 선택으로 잘린 키음이 추가될 수 있다. ( 시간 오름차순 정렬 )
            ConvertedSamples.Sort( delegate ( KeySound _left, KeySound _right )
            {
                if      ( _left.time > _right.time ) return 1;
                else if ( _left.time < _right.time ) return -1;
                else                                 return 0;
            } );    
        };
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
        ConvertedSamples = null;
        Debug.Log( $"DataStorage Release" );
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
                ConvertedSamples = samples;
                if ( !_song.isOnlyKeySound )
                      AddSample( new KeySound( GameSetting.SoundOffset, _song.audioName, 1f ) );
                
                return true;
            }
        }

        return false;
    }

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
    public void AddSample( KeySound _keySound ) => ConvertedSamples.Add( _keySound );
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
    public bool GetTexture( string _name, out Texture2D _tex ) => loadedTextures.TryGetValue( _name, out _tex );
    public Texture2D GetDefaultTexture() => textures[( ++texIndex < textures.Count ? texIndex : texIndex = 0 )];
    public async UniTask LoadTexture( SpriteSample _sprite, Action _OnCompleted = null )
    {
        var path = Path.Combine( NowPlaying.CurrentSong.directory, _sprite.name );

        // 파일이 없거나, 이미 로딩된 파일일 경우
        if ( !File.Exists( path ) || loadedTextures.ContainsKey( _sprite.name ) )
             return;
        
        if ( Path.GetExtension( path ) == ".bmp" )
        {
            // 비트맵 파일은 런타임에서 읽히지 않음( 외부 도움 )
            BMPImage  img = bitmapLoader.LoadBMP( path );
            Texture2D tex = img.ToTexture2D( TextureFormat.RGBA32 );

            // 전경인 경우 비트맵 배경이 검은색으로 되어있음
            Color32[] pixels = tex.GetPixels32();
            for ( int i = 0; i < pixels.Length; i++ )
            {
                if ( pixels[i].r == 0 && pixels[i].g == 0 && pixels[i].b == 0 )
                     pixels[i].a = 0;
            }
            tex.SetPixels32( pixels );
            tex.Apply();

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
        await UniTask.Yield( PlayerLoopTiming.Update );
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
