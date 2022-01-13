using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public abstract class FileReader : IDisposable
{
    private StreamReader streamReader;
    protected string path { get; private set; }
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

    public FileReader() { }

    protected FileReader( string _path )
    {
        path = _path;
        try 
        {
            streamReader = new StreamReader( _path );
        }
        catch ( FileNotFoundException error ) { UnityEngine.Debug.LogError( $"The file could not be read : { error.Message }" ); }
    }

    public void OpenFile( string _path )
    {
        path = _path;
        try
        {
            Dispose();

            streamReader = new StreamReader( @$"\\?\{_path}" );
        }
        catch ( FileNotFoundException error ) { UnityEngine.Debug.LogError( $"The file could not be read : { error.Message }" ); }
    }

    // 한줄 읽기
    protected string ReadLine()
    {
        return streamReader.EndOfStream ? string.Empty : line = streamReader.ReadLine();
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

    public void Dispose()
    {
        streamReader?.Dispose();
    }

    protected string[] GetFilesInSubDirectories( string _dirPath, string _extension )
    {
        List<string> paths = new List<string>();

        try 
        {
            string[] subDirectories = Directory.GetDirectories( _dirPath );

            paths.Capacity = subDirectories.Length;
            for ( int i = 0; i < subDirectories.Length; i++ )
            {
                DirectoryInfo dirInfo = new DirectoryInfo( subDirectories[i] );
                FileInfo[] files      = dirInfo.GetFiles( _extension );

                for ( int j = 0; j < files.Length; j++ )
                {
                    var path = files[j].FullName;
                    if ( File.Exists( path ) ) paths.Add( path );
                    else                       Debug.LogWarning( $"has not file {path}" );
                }
            }
        }
        catch ( Exception e )
        {
            // 대부분 폴더가 없는 경우.
            Debug.LogWarning( $"{e}, {_dirPath}" );
        }

        return paths.ToArray();
    }
}