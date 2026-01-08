using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class Browser : Singleton<Browser>
{
    private ReadOnlyCollection<Song> originSongs;
    public ReadOnlyCollection<Song> Songs { get; private set; } // 검색 등으로 변환된 리스트
    private List<Song> buffer = new ();

    public int  CurrentIndex { get; private set; }
    public Song CurrentSong  { get; private set; }

    public string SearchText  { get; private set; } = string.Empty;
    public bool   IsSearching { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        DataStorage.Inst.OnSongsParsed += Initialize;
    }

    private void OnDestroy()
    {
        DataStorage.Inst.OnSongsParsed -= Initialize;
    }

    public void Initialize( ReadOnlyCollection<Song> _originSongs )
    {
        originSongs = _originSongs;
        Songs       = _originSongs;
        Select( 0 );
    }

    public Song Select( int _index )
    {
        if ( _index >= Songs.Count )
             throw new Exception( "out of range" );

        CurrentIndex = _index;
        CurrentSong  = Songs[_index];
        return CurrentSong;
    }

    public int Search( string _keyword )
    {
        string keyword = _keyword?.Replace( " ", string.Empty );
        if ( originSongs.Count == 0 || string.IsNullOrEmpty( keyword ) )
        {
            Songs = originSongs;
            Select( CurrentSong.index );

            return -1;
        }

        buffer.Clear();
        bool isSV = keyword.ToUpper().CompareTo( "SV" ) == 0;
        for ( int i = 0; i < originSongs.Count; i++ )
        {
            if ( isSV )
            {
                if ( originSongs[i].minBpm != originSongs[i].maxBpm )
                     buffer.Add( originSongs[i] );
            }
            else
            {
                if ( originSongs[i].title.Replace(   " ", string.Empty ).Contains( keyword, StringComparison.OrdinalIgnoreCase ) ||
                     originSongs[i].version.Replace( " ", string.Empty ).Contains( keyword, StringComparison.OrdinalIgnoreCase ) ||
                     originSongs[i].artist.Replace(  " ", string.Empty ).Contains( keyword, StringComparison.OrdinalIgnoreCase ) ||
                     originSongs[i].source.Replace(  " ", string.Empty ).Contains( keyword, StringComparison.OrdinalIgnoreCase ) )
                     buffer.Add( originSongs[i] );
            }
        }

        int prevIndex = 0;
        Songs = buffer.Count == 0 ? originSongs : new ReadOnlyCollection<Song>( buffer );
        for ( int i = 0; i < Songs.Count; i++ )
        {
            if ( Songs[i].title.Contains(   CurrentSong.title,   StringComparison.OrdinalIgnoreCase ) &&
                 Songs[i].version.Contains( CurrentSong.version, StringComparison.OrdinalIgnoreCase ) )
            {
                prevIndex = i;
                break;
            }
        }

        Select( prevIndex );
        
        SearchText  = _keyword;
        IsSearching = SearchText != string.Empty;

        return buffer.Count;
    }

    public void Search( Song _song )
    {
        int prevIndex = 0;
        for ( int i = 0; i < originSongs.Count; i++ )
        {
            if ( originSongs[i].title.Contains(   _song.title,   StringComparison.OrdinalIgnoreCase ) &&
                 originSongs[i].version.Contains( _song.version, StringComparison.OrdinalIgnoreCase ) )
            {
                prevIndex = i;
                break;
            }
        }

        Select( prevIndex );
    }
}
