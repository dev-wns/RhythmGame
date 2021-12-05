using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public abstract class FileReader : IDisposable
{
    private StreamReader streamReader;
    public string line { get; private set; }
    public string directory { get; private set; }

    public FileReader( string _directory )
    {
        directory = _directory;
    }

    public void Initialize( string _path )
    {
        Dispose();

        streamReader = new StreamReader( _path );
    }

    // 한줄 읽기
    public string ReadLine()
    {
        return line = streamReader.ReadLine();
    }

    // 현재 라인에서 단어 찾기
    public bool Contains( string _str )
    {
        if ( line == null )
            return false;

        return line.Contains( _str );
    }

    // 토큰 자르고 공백없앤 후 반환
    public string SplitAndTrim( char _separator )
    {
        if ( line == null || line == string.Empty ) 
            return string.Empty;

        return line.Split( _separator )[1].Trim();
    }

    // 특정 단어 나올때까지 Read
    public string ReadContainsLine( string _str )
    {
        if ( _str == string.Empty ) 
            return string.Empty;

        while ( Contains( _str ) ) 
            ReadLine();

        return line;
    }

    public abstract void Read();

    public void Dispose() => streamReader?.Dispose();
}