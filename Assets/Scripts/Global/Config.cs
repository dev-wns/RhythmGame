using System;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;

public enum ConfigType : int
{
    // User Section
    Email = 0,
    IsSavedEmail,
    ScrollSpeed,
    SoundOffset,
    JudgeOffset,

    // Sound Section
    SoundBuffer = 100,
    Master,
    BGM,
    SFX,

    // GameSetting Section
    BGAOpacity = 200,
    PanelOpacity,
    GearOffsetX,
    AutoPlay,
    NoFailed,
    Measure,
    HitEffect,
    LaneEffect,

    // SystemSetting Section
    Resolution = 300,
    FrameLimit,
    AntiAliasing,
    ScreenMode,

    // KeySetting Section
    _4K = 400,
    _6K,
    _7K,
}

public class Config : Singleton<Config>
{
    private static readonly string ConfigPath  = System.IO.Path.Combine( Global.Path.DataDirectory, "config.ini" );
    private StringBuilder text = new StringBuilder( 255 );

    [DllImport( "kernel32.dll" )] private static extern uint GetPrivateProfileString( string _section, string _key, string _default, StringBuilder _result, uint _size, string _path );
    [DllImport( "kernel32.dll" )] private static extern bool WritePrivateProfileString( string _section, string _key, string _value, string _path );

    // Value가 어떤 타입인지 모르기 때문에 Read, Write시 타입에 맞도록 잘 사용해야함
    public bool Read<TVal>( ConfigType _key, out TVal _value )
    {
        text.Clear();
        Type type = typeof( TVal );
        try
        {
            if ( !IsSupportedType( type ) )
                 throw new NotSupportedException();

            GetPrivateProfileString( GetSectionName( _key ), _key.ToString(), string.Empty, text, 255, ConfigPath );
            
            string value = text.ToString();
            if ( string.IsNullOrEmpty( value ) )
                 throw new ArgumentNullException();

            _value = type.IsEnum ? ( TVal )Enum.Parse( type, value ) : ( TVal )Convert.ChangeType( value, type );
        }
        catch
        {
            Debug.LogWarning( $"Config Read Failed( Key {_key} / Type {type.Name} )" );
            _value = default;
            return false;
        }

        return true;
    }

    public void Write<TVal>( ConfigType _key, TVal _value )
    {
        try
        {
            Type type = typeof( TVal );
            if ( !IsSupportedType( type ) )
                 throw new NotSupportedException();

            if ( !System.IO.Directory.Exists( Global.Path.DataDirectory ) )
                 throw new System.IO.DirectoryNotFoundException();

            WritePrivateProfileString( GetSectionName( _key ), _key.ToString(), _value.ToString(), ConfigPath );
        }
        catch
        {
            Debug.LogWarning( $"Config Write Failed( Key {_key} / Value {_value} )" );
        }
    }

    public void Write( ConfigType _key, KeyCode[] _values )
    {
        text.Clear();
        for ( int i = 0; i < _values.Length; i++ )
        {
            text.Append( _values[i].ToString() );
            if ( i != _values.Length - 1 )
                 text.Append( ',' );
        }

        //string values = string.Join( ",", _values.Select( str => str.ToString() ) );
        Write( _key, text.ToString() );
    }

    public bool Read( ConfigType _key, out KeyCode[] _keyCodes )
    {
        if( Read( _key, out string values ) )
        {
            string[] tokens = values.Split( ',' );
            _keyCodes       = new KeyCode[tokens.Length];
            for ( int i = 0; i < tokens.Length; i++ )
            {
                if ( !Enum.TryParse( tokens[i], out _keyCodes[i] ) )
                     return false;
            }
            return true;
        }
        else
        {
            _keyCodes = Array.Empty<KeyCode>();
            return false;
        }
    }

    private bool IsSupportedType( Type _type )
    {
        return _type == typeof( string  ) ||
               _type == typeof( int     ) ||
               _type == typeof( float   ) ||
               _type == typeof( double  ) ||
               _type == typeof( bool    ) ||
               _type == typeof( short   ) ||
               _type == typeof( long    ) ||
               _type.IsEnum;
    }

    private string GetSectionName( ConfigType _type )
    {
        return ( int )_type switch 
        {
            < 100 => "User",
            < 200 => "Sound",
            < 300 => "GameSetting",
            < 400 => "SystemSetting",
            < 500 => "KeySetting",
            _ => throw new ArgumentException(),
        };
    }
}
