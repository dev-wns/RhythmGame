using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.Networking;
using Newtonsoft.Json;

public struct HitData
{
    public NoteType type;
    public double   diff;
    public double   time;

    public HitData( NoteType _type, double _diff, double _time )
    {
        type = _type;
        diff = _diff;
        time = _time;
    }
}

public struct RecordData
{
    public int    score;
    public int    accuracy;
    public int    random;
    public float  pitch;
    public string date;
}

public struct ResultData
{
    // counts
    public int maximum;
    public int perfect;
    public int great;
    public int good;
    public int bad;
    public int miss;

    public int fast;
    public int slow;
    public int accuracy;
    public int combo;
    public int score;

    public int   random;
    public float pitch;

    public int Count => maximum + perfect + great + good + bad + miss;

    public ResultData( int _random, float _pitch )
    {
        random = _random;
        pitch = _pitch;
        maximum = perfect = great = good = bad = miss = 0;
        fast = slow = accuracy = combo = score = 0;
    }
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
    public static ReadOnlyCollection<KeySound>     Samples     { get; private set; }
    public static ReadOnlyCollection<SpriteSample> Sprites     { get; private set; }

    [Header( "Texture" )]
    private BMPLoader bitmapLoader = new ();
    private Dictionary<string/* name */, Texture2D> loadedTextures = new ();

    [Header( "Sound" )]
    private Dictionary<string/* name */, FMOD.Sound> loadedSounds = new ();

    [Header( "Result Data" )]
    public  List<HitData> HitDatas { get; private set; } = new ();
    public  static RecordData CurrentRecord => recordData;
    private static RecordData recordData = new RecordData();
    public  static ResultData CurrentResult => resultData;
    private static ResultData resultData = new ResultData();

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
                {
                    newSong.index = newSongs.Count;
                    newSongs.Add( newSong );
                }
            }
        }

        newSongs.Sort( ( _left, _right ) => _left.title.CompareTo( _right.title ) );
        for ( int i = 0; i < newSongs.Count; i++ )
        {
            var song    = newSongs[i];
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

    public bool LoadChart()
    {
        using ( FileParser parser = new FileParser() )
        {
            if ( parser.TryParse( NowPlaying.CurrentSong.filePath, out Chart chart ) )
            {
                Notes   = chart.notes;
                Timings = chart.timings;
                Sprites = chart.sprites;
                Samples = chart.samples;

                return true;
            }
        }

        return false;
    }
    #endregion

    public void Release()
    {
        StopAllCoroutines();

        foreach ( var texture in loadedTextures )
            DestroyImmediate( texture.Value, true );

        foreach ( var sound in loadedSounds )
            AudioManager.Inst.Release( sound.Value );


        loadedTextures.Clear();
        loadedSounds.Clear();
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
    public bool TryGetSound( string _name, out FMOD.Sound _sound ) => loadedSounds.TryGetValue( _name, out _sound );

    public void LoadSound( in KeySound _sound )
    {
        if ( !loadedSounds.ContainsKey( _sound.name ) )
        {
            string path = Path.Combine( NowPlaying.CurrentSong.directory, _sound.name );
            if ( AudioManager.Inst.Load( path, out FMOD.Sound sound ) )
                 loadedSounds.Add( _sound.name, sound );
        }
    }
    #endregion

    #region Texture
    public bool TryGetTexture( string _name, out Texture2D _tex ) => loadedTextures.TryGetValue( _name, out _tex );

    public IEnumerator LoadTexture( SpriteSample _sprite )
    {
        var path = Path.Combine( NowPlaying.CurrentSong.directory, _sprite.name );
        
        // 이미 로딩 되었거나 존재하는 파일인지 확인
        if ( loadedTextures.ContainsKey( _sprite.name ) || !File.Exists( path ) )
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
                         throw new System.Exception( www.error );

                    loadedTextures.Add( _sprite.name, handler.texture );
                }
            }
        }
    }
    #endregion

    #region Result Data
    public RecordData CreateNewRecord()
    {
        var newRecord = new RecordData()
        {
            score    = resultData.score,
            accuracy = resultData.accuracy,
            random   = ( int )GameSetting.CurrentRandom,
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

    public void UpdateResult( HitResult _type, int _value = 1 )
    {
        switch ( _type )
        {
            case HitResult.Maximum:  resultData.maximum += _value; break;
            case HitResult.Perfect:  resultData.perfect += _value; break;
            case HitResult.Great:    resultData.great   += _value; break;
            case HitResult.Good:     resultData.good    += _value; break;
            case HitResult.Bad:      resultData.bad     += _value; break;
            case HitResult.Miss:     resultData.miss    += _value; break;

            case HitResult.Fast:     resultData.fast    += _value; break;
            case HitResult.Slow:     resultData.slow    += _value; break;
            case HitResult.Accuracy: resultData.accuracy = _value; break;
            case HitResult.Combo:    resultData.combo    = _value; break;
            case HitResult.Score:    resultData.score    = _value; break;
        }
    }

    public void AddHitData( NoteType _type, double _diff )
    {
        HitDatas.Add( new HitData( _type, _diff, NowPlaying.Playback ) );
    }

    public void Clear()
    {
        resultData = new ResultData( ( int )GameSetting.CurrentRandom, GameSetting.CurrentPitch );
        HitDatas.Clear();
    }
    #endregion
}
