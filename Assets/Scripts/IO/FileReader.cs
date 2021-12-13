using System;
using System.IO;

public abstract class FileReader : IDisposable
{
    private StreamReader streamReader;
    protected string path { get; private set; }
    protected string directory { get; private set; }
    protected string line { get; private set; }

    protected bool ReadLineEndOfStream()
    {
        if ( streamReader.EndOfStream ) return false;
        else
        {
            line = streamReader.ReadLine();
            return true;
        }
    }

    protected FileReader( string _path )
    {
        path = _path;
        try 
        {
            streamReader = new StreamReader( _path );
            directory    = Path.GetDirectoryName( _path );
        }
        catch ( Exception error ) { UnityEngine.Debug.Log( $"The file could not be read : { error.Message }" ); }
    }

    // 한줄 읽기
    protected string ReadLine()
    {
        return line = streamReader.ReadLine();
    }

    // 현재 라인에서 단어 찾기
    protected bool Contains( string _str )
    {
        if ( line == null )
            return false;

        return line.Contains( _str );
    }

    // 토큰 자르고 공백없앤 후 반환
    protected string SplitAndTrim( char _separator )
    {
        if ( line == null || line == string.Empty ) 
            return string.Empty;

        return line.Split( _separator )[1].Trim();
    }

    public void Dispose() => streamReader?.Dispose();
}