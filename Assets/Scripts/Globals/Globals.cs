using UnityEngine;
using System.Diagnostics;

public static class Globals
{
    public static Timer Timer { get; } = new Timer();


    public static float Abs( float _value ) => _value >= 0f ? _value : -_value;
    public static int Abs( int _value )     => _value >= 0  ? _value : -_value;
}

//public static class Debug
//{
//    [Conditional( "UNITY_EDITOR" )] public static void Log( object _message )        => UnityEngine.Debug.Log( _message );
//    [Conditional( "UNITY_EDITOR" )] public static void LogWarning( object _message ) => UnityEngine.Debug.LogWarning( _message );
//    [Conditional( "UNITY_EDITOR" )] public static void LogError( object _message )   => UnityEngine.Debug.LogError( _message );

//    [Conditional( "UNITY_EDITOR" )] public static void Log( object _message, Object _context )        => UnityEngine.Debug.Log( _message, _context );
//    [Conditional( "UNITY_EDITOR" )] public static void LogWarning( object _message, Object _context ) => UnityEngine.Debug.LogWarning( _message, _context );
//    [Conditional( "UNITY_EDITOR" )] public static void LogError( object _message, Object _context )   => UnityEngine.Debug.LogError( _message, _context );
//}