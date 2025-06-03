using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

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
        random  = _random;
        pitch   = _pitch;
        maximum = perfect = great = good = bad = miss = 0;
        fast    = slow = accuracy = combo = score = 0;
    }
}

public class GameManager : Singleton<GameManager>
{
    // MultiPlay
    public static bool IsMultiPlaying { get; set; } = false;
    public static USER_INFO? UserInfo { get; set; }
    public static STAGE_INFO? StageInfo { get; set; }

    // 
    public  List<HitData> HitDatas { get; private set; } = new List<HitData>();

    public  static RecordData CurrentRecord => recordData;
    private static RecordData recordData = new RecordData();
    public  static ResultData CurrentResult => resultData;
    private static ResultData resultData = new ResultData();

    public static int ResultCount => CurrentResult.Count;

    [Header( "Addressable" )]
    private List<AsyncOperationHandle> handles = new List<AsyncOperationHandle>();

    protected override void Awake()
    {
        base.Awake();
        KeySetting keySetting = KeySetting.Inst;
        InputManager inputManager = InputManager.Inst;
    }

    public void LoadAssetsAsync<T>( string _label, Action<T> _OnCompleted ) where T : UnityEngine.Object
    {
        AsyncOperationHandle<IList<IResourceLocation>> locationHandle = Addressables.LoadResourceLocationsAsync( _label, typeof( T ) );
        handles.Add( locationHandle );

        locationHandle.Completed += ( AsyncOperationHandle<IList<IResourceLocation>> _handle ) =>
        {
            if ( _handle.Status != AsyncOperationStatus.Succeeded )
            {
                Debug.LogWarning( "Load Location Async Failed" );
                return;
            }

            foreach ( IResourceLocation location in _handle.Result )
            {
                AsyncOperationHandle<T> assetHandle = Addressables.LoadAssetAsync<T>( location );
                handles.Add( assetHandle );

                assetHandle.Completed += ( AsyncOperationHandle<T> _handle ) =>
                {
                    if ( _handle.Status != AsyncOperationStatus.Succeeded )
                    {
                        Debug.LogError( "Load Asset Async Failed" );
                        return;
                    }

                    _OnCompleted?.Invoke( _handle.Result );
                };
            }
        };
    }

    public bool UpdateRecord()
    {
        string path = Path.Combine( GameSetting.RecordPath, $"{Path.GetFileNameWithoutExtension( NowPlaying.CurrentSong.filePath )}.json" );
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

        string path = Path.Combine( GameSetting.RecordPath, $"{Path.GetFileNameWithoutExtension( NowPlaying.CurrentSong.filePath )}.json" );
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

    public void AddHitData( NoteType _type, double _diff )
    {
        HitDatas.Add( new HitData( _type, _diff, NowPlaying.Playback ) );
    }

    public void Clear()
    {
        resultData = new ResultData( ( int )GameSetting.CurrentRandom, GameSetting.CurrentPitch );
        HitDatas.Clear();
    }
}
