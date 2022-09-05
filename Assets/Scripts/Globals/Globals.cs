using UnityEngine;
using System.Diagnostics;

public static class Globals
{
    public static Timer Timer { get; } = new Timer();

    // 부동 소수점끼리의 비교는 두 값을 뺸 결과를 절대값으로 나타낸 후
    // 오차범위값과 비교하는게 정확하지만 아래 함수들은 성능을 중요시 함.
    public static float Lerp( float _start, float _end, float _t ) => _start + ( _end - _start ) * _t;
    public static double Abs( double _value ) => _value >= 0d ? _value : -_value;
    public static float Abs( float _value )   => _value >= 0f ? _value : -_value;
    public static int Abs( int _value )       => _value >= 0  ? _value : -_value;
    public static double Round( double _value ) => _value - ( int )_value >= .5d ? ( int )_value + 1d : ( int )_value;
    public static float Round( float _value )   => _value - ( int )_value >= .5f ? ( int )_value + 1f : ( int )_value;
    public static float Clamp( float _value, float _min, float _max )
    {
        return _value < _min ? _min : 
               _value > _max ? _max : 
               _value;
    }
    public static int Log10( double _value )
    {
        // 현재 7자리까지 사용함.
        return ( _value >= 10000000u )   ? 7 : ( _value >= 1000000u )   ? 6 :
               ( _value >= 100000u )     ? 5 : ( _value >= 10000u )     ? 4 :
               ( _value >= 1000u )       ? 3 : ( _value >= 100u )       ? 2 : 
               ( _value >= 10u )         ? 1 : 0;
    }

    /// <summary>
    /// Returns the calculated value of the ratio to the screen.
    /// </summary> 
    /// <param name="_screen"> The value is adjusted based on this value. </param>
    public static Vector3 GetScreenRatio( Texture2D _tex, Vector2 _screen )
    {
        float width = _tex.width;
        float height = _tex.height;

        float offsetX = _screen.x / _tex.width;
        width *= offsetX;
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

public static class GlobalConst
{
    public static readonly float OptionFadeDuration = .15f;
}

public static class Debug
{
    [Conditional( "UNITY_EDITOR" )] public static void Log( object _message ) => UnityEngine.Debug.Log( _message );
    [Conditional( "UNITY_EDITOR" )] public static void LogWarning( object _message ) => UnityEngine.Debug.LogWarning( _message );
    [Conditional( "UNITY_EDITOR" )] public static void LogError( object _message ) => UnityEngine.Debug.LogError( _message );

    [Conditional( "UNITY_EDITOR" )] public static void Log( object _message, Object _context ) => UnityEngine.Debug.Log( _message, _context );
    [Conditional( "UNITY_EDITOR" )] public static void LogWarning( object _message, Object _context ) => UnityEngine.Debug.LogWarning( _message, _context );
    [Conditional( "UNITY_EDITOR" )] public static void LogError( object _message, Object _context ) => UnityEngine.Debug.LogError( _message, _context );
}