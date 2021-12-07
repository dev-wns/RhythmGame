using System.IO;

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
}

public class Chart
{
    public int NoteCount { get; set; }
    public int LongNoteCount { get; set; }
    public int MinBPM { get; set; }
    public int MaxBPM { get; set; }
    public int KeyCount { get; set; }
}

public abstract class SubDirectoriesReader : FileReader
{
    public string directoryPath { get; private set; }

    public SubDirectoriesReader( string _dir ) { directoryPath = _dir; }

    public override void Read() 
    {
        string[] subDirectories = Directory.GetDirectories( directoryPath );

        foreach ( string dirs in subDirectories )
        {
            ReadSubDirectories( dirs );
        }
    }

    protected abstract void ReadSubDirectories( string _subDir );

    //private async void ReadAsync()
    //{
    //    if ( directory == string.Empty ) return;
    //
    //    string[] subDirectories = Directory.GetDirectories( directory );
    //    var tasks = new List<Task>();
    //    foreach ( string subDirs in subDirectories )
    //    {
    //        tasks.Add( LoadDirectoryAsync( subDirs ) );
    //    }
    //
    //    await Task.WhenAll( tasks );
    //}
    //
    //private Task LoadDirectoryAsync( string _subDir )
    //{
    //    return Task.Run( () => ReadSubDirectories( _subDir ) );
    //}  
}