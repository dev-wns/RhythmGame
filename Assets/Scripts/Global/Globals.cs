using UnityEngine;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

namespace Global
{
    /// <summary> 가벼운 Math </summary>
    public static class Math
    {
        public static float Lerp( float _start, float _end, float _t ) => _start + ( _end - _start ) * _t;
        public static double Lerp( double _start, double _end, double _t ) => _start + ( _end - _start ) * _t;
        public static double Abs( double _value )     => _value >= 0d ? _value : -_value;
        public static float Abs( float _value )       => _value >= 0f ? _value : -_value;
        public static int Abs( int _value )           => _value >= 0  ? _value : -_value;
        public static double Round( double _value )   => _value - ( int )_value >= .5d ? ( int )_value + 1d : ( int )_value;
        public static float Round( float _value )     => _value - ( int )_value >= .5f ? ( int )_value + 1f : ( int )_value;
        public static int Clamp( int _value, int _min, int _max )
        {
            return _value < _min ? _min :
                   _value > _max ? _max :
                   _value;
        }
        public static float Clamp( float _value, float _min, float _max )
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
        public static int Log10( double _value )
        {
            return ( _value >= 10000000u ) ? 7 : ( _value >= 1000000u ) ? 6 :
                   ( _value >= 100000u )   ? 5 : ( _value >= 10000u )   ? 4 :
                   ( _value >= 1000u )     ? 3 : ( _value >= 100u )     ? 2 :
                   ( _value >= 10u )       ? 1 : 0;
        }

        /// <summary> Returns the calculated value of the ratio to the screen. </summary> 
        /// <param name="_screen"> The value is adjusted based on this value. </param>
        public static Vector3 GetScreenRatio( Texture2D _tex, Vector2 _screen )
        {
            float width  = _tex.width;
            float height = _tex.height;

            float offsetX = _screen.x / width;
            width  *= offsetX;
            height *= offsetX;

            float offsetY = _screen.y / height;
            if ( offsetY > 1f )
            {
                width  *= offsetY;
                height *= offsetY;
            }

            return new Vector3( width, height, 1f );
        }
    }


    public static class Color
    {
        /// <summary> Clear All ( 0, 0, 0, 0 ) </summary>
        public static UnityEngine.Color Clear    = new UnityEngine.Color( 0f, 0f, 0f, 0f );
        /// <summary> Clear Alpha ( 1, 1, 1, 0 ) </summary>
        public static UnityEngine.Color ClearA   = new UnityEngine.Color( 1f, 1f, 1f, 0f );
        /// <summary> Clear Red, Blue, Green ( 0, 0, 0, 1 ) </summary>
        public static UnityEngine.Color ClearRGB = new UnityEngine.Color( 0f, 0f, 0f, 1f );
    }

    public static class FILE
    {
        public static string[] GetFilesInSubDirectories( string _dirPath, string _extension )
        {
            List<string> paths = new List<string>();
            try
            {
                string[] subDirectories = System.IO.Directory.GetDirectories( _dirPath );
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
                // 대부분 폴더가 없는 경우.
                Debug.LogError( $"{_error}, {_dirPath}" );
            }

            return paths.ToArray();
        }
    }

    public static class Const
    {
        public static readonly float OptionFadeDuration = .15f;
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

public static class Debug
{
    [Conditional( "UNITY_EDITOR" )] public static void Log( object _message )        => UnityEngine.Debug.Log( _message );
    [Conditional( "UNITY_EDITOR" )] public static void LogWarning( object _message ) => UnityEngine.Debug.LogWarning( _message );
    [Conditional( "UNITY_EDITOR" )] public static void LogError( object _message )   => UnityEngine.Debug.LogError( _message );

    [Conditional( "UNITY_EDITOR" )] public static void Log( object _message, UnityEngine.Object _context )        => UnityEngine.Debug.Log( _message, _context );
    [Conditional( "UNITY_EDITOR" )] public static void LogWarning( object _message, UnityEngine.Object _context ) => UnityEngine.Debug.LogWarning( _message, _context );
    [Conditional( "UNITY_EDITOR" )] public static void LogError( object _message, UnityEngine.Object _context )   => UnityEngine.Debug.LogError( _message, _context );
}