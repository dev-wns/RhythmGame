using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class Record
{
    public RecordData CurrentData { get; private set; }

    public RecordData CreateNewRecord()
    {
        var newRecord = new RecordData()
        {
            score    = Judgement.Inst.Score,
            accuracy = Judgement.Inst.Accuracy,
            random   = GameSetting.CurrentRandom,
            pitch    = GameSetting.SoundPitch,
            date     = DateTime.Now.ToString( "yyyy. MM. dd @ hh:mm:ss tt" )
        };

        if ( CurrentData.score > newRecord.score )
             return CurrentData;

        string path = Path.Combine( Global.IO.Path.RecordDirectory, $"{Path.GetFileNameWithoutExtension( Browser.Inst.CurrentSong.filePath )}.json" );
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
            CurrentData = newRecord;
        }
        catch ( Exception )
        {
            if ( File.Exists( path ) )
                 File.Delete( path );

            Debug.LogError( $"Record Write Error : {path}" );
        }

        return CurrentData;
    }

    public bool UpdateRecord()
    {
        string path = Path.Combine( Global.IO.Path.RecordDirectory, $"{Path.GetFileNameWithoutExtension( Browser.Inst.CurrentSong.filePath )}.json" );
        if ( !File.Exists( path ) )
        {
            //recordData = new RecordData();
            return false;
        }

        using ( StreamReader stream = new StreamReader( path ) )
        {
            try
            {
                CurrentData = JsonConvert.DeserializeObject<RecordData>( stream.ReadToEnd() );
            }
            catch ( Exception )
            {
                Debug.LogWarning( $"Record Deserialize Error : {Browser.Inst.CurrentSong.filePath}" );
                return false;
            }
        }

        return true;
    }
}
