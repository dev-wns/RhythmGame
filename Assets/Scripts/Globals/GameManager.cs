using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public static List<MetaData> Datas = new List<MetaData>();

    public delegate void DelLoaded( float _offset );
    public static DelLoaded OnLoaded;

    public static bool IsDone { get; private set; } = false;
    public static int Combo;

    private void Awake()
    {
        // Setting
        Screen.SetResolution( 1920, 1080, false );
        Screen.fullScreen = true;
        Application.targetFrameRate = 144;
        
        SoundManager.OnRelease += Release;

        // Parsing
        DirectoryInfo info = new DirectoryInfo( Application.streamingAssetsPath + "/Songs" );
        foreach ( var dir in info.GetDirectories() )
        {
            foreach ( var file in dir.GetFiles( "*.osu" ) )
            {
                MetaData data = Read( file.FullName );
                if ( ReferenceEquals( null, data ) )
                {
                    Debug.Log( string.Format( "parsing failed. no data was created. #Path : {0}", file.FullName ) );
                }

                data.sound = SoundManager.Inst.Load( data.audioPath, true );
                Datas.Add( data );
            }
            StartCoroutine( BackgroundsLoad() );
        }
        Debug.Log( "Data Parsing Finish" );
    }

    private IEnumerator BackgroundsLoad()
    {
        foreach ( var data in Datas )
        {
            // backgrounds
            UnityWebRequest www = UnityWebRequestTexture.GetTexture( data.imgPath );

            yield return www.SendWebRequest();
            if ( www.result != UnityWebRequest.Result.Success )
            {
                Debug.Log( www.error );
            }
            else
            {
                Texture2D tex = ( ( DownloadHandlerTexture )www.downloadHandler ).texture;
                Sprite sprite = Sprite.Create( tex, new Rect( 0f, 0f, tex.width, tex.height ), new Vector2( 0.5f, 0.5f ) );

                data.background = sprite;
            }

            OnLoaded( 1f / Datas.Count );
        }

        IsDone = true;
        Debug.Log( "Backgrounds Load Finish." );
    }

    #region File Read
    public MetaData Read( string _path )
    {
        string line;
        StreamReader reader = new StreamReader( _path );
        MetaData data = new MetaData();

        while ( ( line = reader.ReadLine() ) != null )
        {
            if ( line.Contains( "[General]" ) )
            {
                List<string> arr = new List<string>();
                for ( int index = 0; index < 3; ++index )
                {
                    if ( string.IsNullOrEmpty( line ) || line.Contains( "[Metadata]" ) )
                    {
                        break;
                    }
                    arr.Add( line = reader.ReadLine() );
                }

                data.audioName = Path.GetFileName( arr[0].Substring( 14 ).Trim() );
                data.audioPath = Path.GetDirectoryName( _path ) + "\\" + data.audioName;
                data.previewTime = int.Parse( arr[2].Substring( 12 ).Trim() );
            }

            if ( line.Contains( "[Metadata]" ) )
            {
                List<string> arr = new List<string>();
                for ( int index = 0; index < 6; ++index )
                {
                    if ( string.IsNullOrEmpty( line ) || line.Contains( "[Events]" ) )
                    {
                        break;
                    }
                    arr.Add( line = reader.ReadLine() );
                }

                data.title = arr[0].Substring( 6 ).Trim();
                data.artist = arr[2].Substring( 7 ).Trim();
                data.creator = arr[4].Substring( 8 ).Trim();
                data.version = arr[5].Substring( 8 ).Trim();
            }

            if ( line.Contains( "[Events]" ) )
            {
                List<string> arr = new List<string>();
                for ( int index = 0; index < 4; ++index )
                {
                    if ( string.IsNullOrEmpty( line ) || line.Contains( "[TimingPoints]" ) )
                    {
                        break;
                    }
                    arr.Add( line = reader.ReadLine() );
                }

                string[] img = arr[1].Split( ',' );
                data.imgName = img[2].Trim().Replace( "\"", string.Empty );
                data.imgPath = Path.GetDirectoryName( _path ) + "\\" + data.imgName;
            }

            if ( line.Contains( "[TimingPoints]" ) )
            {
                double prevBPM = 0d;
                while ( !( string.IsNullOrEmpty( line = reader.ReadLine() ) || line.Contains( "[Colours]" ) || line.Contains( "[HitObjects]" ) ) )
                {
                    string[] arr = line.Split( ',' );

                    bool isUninherited = StringToBoolean( arr[6] );
                    float changeTime = float.Parse( arr[0] );
                    double beatLength = Mathf.Abs( float.Parse( arr[1] ) );
                    double BPM = 1d / beatLength * 60000d;

                    if ( isUninherited ) prevBPM = BPM;
                    else                 BPM = ( prevBPM * 100d ) / beatLength;

                    data.timings.Add( new Timings( changeTime, ( float )BPM ) );
                }
            }

            if ( line.Contains( "[HitObjects]" ) )
            {
                while ( !string.IsNullOrEmpty( line = reader.ReadLine() ) )
                {
                    string[] arr = line.Split( ',' );
                    string[] LNTiming = arr[5].Split( ':' );
                    data.notes.Add( new Notes( int.Parse( arr[0] ), float.Parse( arr[2] ), int.Parse( arr[3] ), int.Parse( LNTiming[0] ) ) );
                }
            }
        }
        reader.Close();

        int idx = data.audioName.IndexOf( "-" );
        if ( idx >= 0 )
        {
            string src = data.audioName;
            data.audioName = data.audioName.Replace( "-", "" );
            File.Move( Path.GetDirectoryName( _path ) + "\\" + src, Path.GetDirectoryName( _path ) + "\\" + data.audioName );

            string[] lines = File.ReadAllLines( _path );
            var pos = Array.FindIndex( lines, row => row.Contains( "AudioFilename:" ) );
            if ( pos > 0 )
            {
                lines[pos] = string.Format( "AudioFilename:{0}", data.audioName );
                File.WriteAllLines( _path, lines );
            }
        }

        return data;
    }

    private bool StringToBoolean( string _value )
    {
        int value = int.Parse( _value );
        if ( value != 0 ) return true;
        else return false;
    }

    // directories since streaming asset path
    public static string[] GetFiles( string _path, string _extension = "*.osu" )
    {
        List<string> directories = new List<string>();
        DirectoryInfo info = new DirectoryInfo( Application.streamingAssetsPath + _path );
        foreach ( var dir in info.GetDirectories() )
        {
            foreach ( var file in dir.GetFiles( _extension ) )
            {
                directories.Add( file.FullName );
            }
        }

        return directories.ToArray();
    }
    #endregion

    private void Release()
    {
        foreach( var data in Datas )
        {
            data.sound.release();
        }
    }
}
