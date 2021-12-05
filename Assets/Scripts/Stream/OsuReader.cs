using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

public class Song
{
    public string AudioPath { get; set; }
    public string ImagePath { get; set; }
    public string VideoPath { get; set; }
    public bool HasVideo    { get; set; }
                            
    public string Title     { get; set; }
    public string Artist    { get; set; }
    public string Creator   { get; set; }
    public string Version   { get; set; }
    
    public int PreviewTime  { get; set; }

    public Song() { }
    public Song( Song _song )
    {
        AudioPath   = _song.AudioPath;
        ImagePath   = _song.ImagePath;
        VideoPath   = _song.VideoPath;
        HasVideo    = _song.HasVideo;
        Title       = _song.Title;
        Artist      = _song.Artist;
        Creator     = _song.Creator; 
        PreviewTime = _song.PreviewTime;
    }
}

public class Chart
{
    public int NoteCount { get; set; }
    public int LongNoteCount { get; set; }
    public int MinBPM { get; set; }
    public int MaxBPM { get; set; }
    public int KeyCount { get; set; }
}

public class OsuReader : FileReader
{
    public OsuReader( string _path ) : base( _path ) { }

    public override void Read() 
    {
        string[] subDirectories = Directory.GetDirectories( directory );

        foreach ( string dirs in subDirectories )
        {
            ReadSubDirectories( dirs );
        }

        //ReadAsync(); 
    }

    private async void ReadAsync()
    {
        if ( directory == string.Empty ) return;

        string[] subDirectories = Directory.GetDirectories( directory );
        var tasks = new List<Task>();
        foreach ( string subDirs in subDirectories )
        {
            tasks.Add( LoadDirectoryAsync( subDirs ) );
        }

        await Task.WhenAll( tasks );
    }

    private Task LoadDirectoryAsync( string _subDir )
    {
        return Task.Run( () => ReadSubDirectories( _subDir ) );
    }

    private void ReadSubDirectories( string _subDir )
    {
        Song sharedData = new Song();
        DirectoryInfo dirInfo = new DirectoryInfo( _subDir );
        
        // 채보 파일
        FileInfo[] charts = dirInfo.GetFiles( "*.osu" );
        if ( charts.Length == 0 ) return; // 채보 없으면 종료

        // 음악 파일
        FileInfo[] musics = dirInfo.GetFiles( "*.mp3" );
        if ( musics.Length == 0 ) return; // 음악 없으면 종료
        sharedData.AudioPath = musics[0].FullName;

        // 동영상 파일
        FileInfo[] videos = dirInfo.GetFiles( "*.avi" );
        if ( videos.Length == 0 ) videos = dirInfo.GetFiles( "*.mp4" );
        if ( videos.Length == 0 ) videos = dirInfo.GetFiles( "*.mpg" );
        if ( videos.Length == 0 ) sharedData.HasVideo = false;
        else
        {
            sharedData.VideoPath = videos[0].FullName;
            sharedData.HasVideo = true;
        }

        // 파싱
        try
        {
            for ( int i = 0; i < charts.Length; i++ )
            {
                Song newSong = new Song( sharedData );
                Initialize( charts[i].FullName );

                while ( ReadLine() != "[Metadata]" )
                {
                    if ( Contains( "PreviewTime" ) ) newSong.PreviewTime = int.Parse( SplitAndTrim( ':' ) );
                }

                while ( ReadLine() != "[Events]" )
                {
                    if ( Contains( "Title" ) )   newSong.Title   = SplitAndTrim( ':' );
                    if ( Contains( "Artist" ) )  newSong.Artist  = SplitAndTrim( ':' );
                    if ( Contains( "Creator" ) ) newSong.Creator = SplitAndTrim( ':' );
                    if ( Contains( "Version" ) ) newSong.Version = SplitAndTrim( ':' );
                }

                while ( ReadLine() != "[TimingPoints]" )
                {
                    if ( ReadLine().Contains( "Video" ) ) ReadLine();

                    string[] imageToken = line.Split( '"' );
                    newSong.ImagePath = Path.Combine( _subDir, SplitAndTrim( '"' ) );

                    FileInfo imageInfo = new FileInfo( newSong.ImagePath );
    
                    if ( !imageInfo.Exists ) newSong.ImagePath = Path.Combine( UnityEngine.Application.dataPath, "Textures", "Default", "DefaultImage.jpg" );
                    break;
                }

                GameManager.songs.Add( newSong );
            }
        }
        catch(Exception _error )
        {
            Dispose();
        }
    }    
}