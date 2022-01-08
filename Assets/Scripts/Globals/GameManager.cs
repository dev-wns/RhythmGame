using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonUnity<GameManager>
{
    private List<Song> Songs = new List<Song>();
    public int Count { get { return Songs.Count; } }
    public Song CurrentSong { get; private set; }
    public int CurrentSongIndex { get; private set; }
    public float MedianBpm { get; private set; }

    public static int Combo;
    public static int Kool, Cool, Good;
    
    private void Awake()
    {
        using ( FileConverter converter = new FileConverter() )
        {
            converter.ReLoad();
        }

        using ( FileParser parser = new FileParser() )
        {
            parser.TryParseArray( ref Songs );
        }

        if ( Songs.Count > 0 ) { SelectSong( 0 ); }
        Debug.Log( "Parse Success " );

        QualitySettings.vSyncCount = 0;
    }

    private void Update()
    {
        //SoundManager.Inst.Update();
    }

    private void OnApplicationQuit()
    {
        //SoundManager.Inst.Release();
    }

    public void SelectSong( int _index )
    {
        if ( _index < 0 || _index > Songs.Count - 1 )
        {
            Debug.Log( $"Sound Select Out Of Range. Index : {_index}" );
            return;
        }

        CurrentSongIndex = _index;
        CurrentSong      = Songs[_index];
        MedianBpm        = Songs[_index].medianBpm;
    }

    public Song GetSong( int _index )
    {
        if ( _index > Count )
        {
            Debug.Log( $"Sound Select Out Of Range. Index : {_index}" );
            return new Song();
        }

        return Songs[_index];
    }
}
