using UnityEngine;
using System.Diagnostics;

public static class Globals
{
    public static Timer Timer { get; } = new Timer();

    public static double Abs( double _value ) => _value >= 0d ? _value : -_value;
    public static float Abs( float _value )   => _value >= 0f ? _value : -_value;
    public static int Abs( int _value )       => _value >= 0  ? _value : -_value;
    public static int Log10( float _value )
    {
        // 현재 6자리까지 사용함.
        return ( _value >= 10000000u )   ? 7 : ( _value >= 1000000u )   ? 6 :
               ( _value >= 100000u )     ? 5 : ( _value >= 10000u )     ? 4 :
               ( _value >= 1000u )       ? 3 : ( _value >= 100u )       ? 2 : 
               ( _value >= 10u )         ? 1 : 0;
    }
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