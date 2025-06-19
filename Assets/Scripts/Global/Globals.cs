using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Global
{
    public struct Path
    {
        public static readonly string SoundDirectory  = System.IO.Path.Combine( Application.streamingAssetsPath, "Songs" );
        public static readonly string FailedDirectory = System.IO.Path.Combine( Application.streamingAssetsPath, "Failed" );
        public static readonly string RecordDirectory = System.IO.Path.Combine( Application.streamingAssetsPath, "Records" );

        public static string[] GetFilesInSubDirectories( string _dir, string _extension )
        {
            List<string> paths = new List<string>();
            try
            {
                string[] subDirectories = System.IO.Directory.GetDirectories( _dir );
                paths.Capacity = subDirectories.Length;
                for ( int i = 0; i < subDirectories.Length; i++ )
                {
                    System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo( subDirectories[i] );
                    System.IO.FileInfo[] files      = dirInfo.GetFiles( _extension );

                    for ( int j = 0; j < files.Length; j++ )
                        paths.Add( files[j].FullName );
                }
            }
            catch ( System.Exception _error )
            {
                // 대부분 폴더가 없는 경우
                Debug.LogError( $"{_error}, {_dir}" );
            }

            return paths.ToArray();
        }
    }

    /// <summary> 가벼운 Math </summary>
    public struct Math
    {
        public static float  Lerp( float _start, float _end, float _t )    => _start + ( _end - _start ) * _t;
        public static double Lerp( double _start, double _end, double _t ) => _start + ( _end - _start ) * _t;
        public static double Abs( double _value )                          => _value >= 0d ? _value : -_value;
        public static float  Abs( float _value )                           => _value >= 0f ? _value : -_value;
        public static int    Abs( int _value )                             => _value >= 0 ? _value : -_value;
        public static double Round( double _value )                        => _value - ( int )_value >= .5d ? ( int )_value + 1d : ( int )_value;
        public static float  Round( float _value )                         => _value - ( int )_value >= .5f ? ( int )_value + 1f : ( int )_value;
        public static int    Min( int _arg1, int _arg2 )                   => _arg1 < _arg2 ? _arg1 : _arg2;
        public static float  Min( float _arg1, float _arg2 )               => _arg1 < _arg2 ? _arg1 : _arg2;
        public static double Min( double _arg1, double _arg2 )             => _arg1 < _arg2 ? _arg1 : _arg2;
        public static int    Max( int _arg1, int _arg2 )                   => _arg1 > _arg2 ? _arg1 : _arg2;
        public static float  Max( float _arg1, float _arg2 )               => _arg1 > _arg2 ? _arg1 : _arg2;
        public static double Max( double _arg1, double _arg2 )             => _arg1 > _arg2 ? _arg1 : _arg2;
        public static int    Clamp( int _value, int _min, int _max )
        {
            return _value < _min ? _min :
                   _value > _max ? _max :
                   _value;
        }
        public static float  Clamp( float _value, float _min, float _max )
        {
            return _value < _min ? _min :
                   _value > _max ? _max :
                   _value;
        }
        public static double Clamp( double _value, double _min, double _max )
        {
            return _value < _min ? _min :
                   _value > _max ? _max :
                   _value;
        }
        public static int    Log10( double _value )
        {
            return ( _value >= 10000000u ) ? 7 : ( _value >= 1000000u ) ? 6 :
                   ( _value >= 100000u ) ? 5 : ( _value >= 10000u ) ? 4 :
                   ( _value >= 1000u ) ? 3 : ( _value >= 100u ) ? 2 :
                   ( _value >= 10u ) ? 1 : 0;
        }

    }

    public struct Screen
    {
        public static int Width  = 1920;
        public static int Height = 1080;

        /// <returns> 스크린 해상도에 맞춰진 텍스처의 해상도를 반환합니다. </returns>
        public static Vector2 GetRatio<T>( in T _tex ) where T : UnityEngine.Texture
        {
            float texWidth  = _tex.width;
            float texHeight = _tex.height;

            float offsetX = Width / texWidth;
            texWidth  *= offsetX;
            texHeight *= offsetX;

            float offsetY = Height / texHeight;
            if ( offsetY > 1f )
            {
                texWidth  *= offsetY;
                texHeight *= offsetY;
            }

            return new Vector2( texWidth, texHeight );
        }
    }

    public struct Color
    {
        /// <summary> Clear All ( 0, 0, 0, 0 ) </summary>
        public static UnityEngine.Color Clear    = new UnityEngine.Color( 0f, 0f, 0f, 0f );
        /// <summary> Clear Alpha ( 1, 1, 1, 0 ) </summary>
        public static UnityEngine.Color ClearA   = new UnityEngine.Color( 1f, 1f, 1f, 0f );
        /// <summary> Clear Red, Blue, Green ( 0, 0, 0, 1 ) </summary>
        public static UnityEngine.Color ClearRGB = new UnityEngine.Color( 0f, 0f, 0f, 1f );
    }

    public struct Const
    {
        public static readonly float CanvasFadeDuration = .15f;
    }
}

public static class Extentions
{
    public static void Increment<T>( this IDictionary<T, int> _dictionary, T _key )
    {
        if ( _dictionary.TryGetValue( _key, out int _count ) )
            _dictionary[_key] = _count + 1;
    }
}

public enum Error : ushort
{
    OK = 0,
    DB_ERR_DISCONNECTED,   // 서버에 연결되지않음
    DB_ERR_INVALID_QUERY,  // 쿼리 구문이 유효하지않음
    DB_ERR_DUPLICATE_DATA, // UNIQUE로 설정된 데이터가 이미 존재함

    ERR_NOT_EXIST_DATA,    // 데이터가 존재하지않음
    ERR_INVALID_DATA,      // 데이터가 유효하지않음
    ERR_UNABLE_PROCESS,    // 요청을 수행할 수 없음
}

public static class Debug
{
    [Conditional( "UNITY_EDITOR" )] public static void Log( object _message ) => UnityEngine.Debug.Log( _message );
    [Conditional( "UNITY_EDITOR" )] public static void LogWarning( object _message ) => UnityEngine.Debug.LogWarning( _message );
    [Conditional( "UNITY_EDITOR" )] public static void LogError( object _message ) => UnityEngine.Debug.LogError( _message );

    [Conditional( "UNITY_EDITOR" )] public static void Log( object _message, UnityEngine.Object _context ) => UnityEngine.Debug.Log( _message, _context );
    [Conditional( "UNITY_EDITOR" )] public static void LogWarning( object _message, UnityEngine.Object _context ) => UnityEngine.Debug.LogWarning( _message, _context );
    [Conditional( "UNITY_EDITOR" )] public static void LogError( object _message, UnityEngine.Object _context ) => UnityEngine.Debug.LogError( _message, _context );
}