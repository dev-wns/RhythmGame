using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public enum GameKeyCount : int { _1 = 1, _2, _3, _4, _5, _6, _7, _8 };
public enum KeyState { None, Down, Hold, Up, }
public class KeySetting : Singleton<KeySetting>
{
    // 사용자가 설정한 게임에서 사용되는 키
    public Dictionary<GameKeyCount, KeyCode[]> Keys = new Dictionary<GameKeyCount, KeyCode[]>();

    // 사용가능한 키
    private static readonly Dictionary<int/* Virtual Key */, KeyCode>        vKeyToUnity   = new ();
    private static readonly Dictionary<KeyCode, int/* Virtual Key */>        unityToVKey   = new ();
    private static readonly Dictionary<KeyCode, string/*keyCode to string*/> unityToString = new ();
    
    public Dictionary<KeyCode, string>.KeyCollection AvailableKeys => unityToString.Keys;
    public bool IsAvailable( KeyCode _key )      => unityToString.ContainsKey( _key );
    public KeyCode GetKeyCode( int _vKey )       => vKeyToUnity.TryGetValue( _vKey, out KeyCode keyCode ) ? keyCode : KeyCode.None;
    public int GetVirtualKey( KeyCode _keyCode ) => unityToVKey.TryGetValue( _keyCode, out int vKey ) ? vKey : -1;
    public string GetString( KeyCode _code )     => unityToString.ContainsKey( _code ) ? unityToString[_code] : "None";

    protected override void Awake()
    {
        base.Awake();

        Initialize( GameKeyCount._4, new KeyCode[] { KeyCode.W, KeyCode.E, KeyCode.Delete, KeyCode.End } );
        Initialize( GameKeyCount._6, new KeyCode[] { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.Delete, KeyCode.End, KeyCode.PageDown } );
        Initialize( GameKeyCount._7, new KeyCode[] { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.Space, KeyCode.Delete, KeyCode.End, KeyCode.PageDown, } );

        KeyMapping();
        StringMapping();
    }

    private void Initialize( GameKeyCount _key, KeyCode[] _code )
    {
        int count = ( int )_key;
        if ( count != _code.Length )
        {
            Debug.LogError( "key count and length do not match. " );
            return;
        }
        if ( Keys.ContainsKey( _key ) )
        {
            Debug.LogWarning( "The key already exists." );
            return;
        }

        Keys.Add( _key, _code );
    }

    #region Mapping
    private void AddMapping( int _vKey, KeyCode _keyCode )
    {
        vKeyToUnity[_vKey]    = _keyCode;
        unityToVKey[_keyCode] = _vKey;
    }

    private void KeyMapping()
    {
        // 숫자 (0~9)
        for ( int i = 0; i <= 9; i++ )
        {
            int vKey = 0x30 + i; // '0' ~ '9'
            KeyCode uKey = KeyCode.Alpha0 + i;
            AddMapping( vKey, uKey );
        }


        // 알파벳 (A~Z)
        for ( int i = 0; i < 26; i++ )
        {
            int vKey = 0x41 + i; // 'A' ~ 'Z'
            KeyCode uKey = KeyCode.A + i;
            AddMapping( vKey, uKey );
        }

        // 특수키
        AddMapping( 0xC0, KeyCode.BackQuote ); 
        AddMapping( 0x08, KeyCode.Backspace );
        AddMapping( 0xDC, KeyCode.Backslash );
        AddMapping( 0x14, KeyCode.CapsLock);
        AddMapping( 0x0D, KeyCode.Return );
        AddMapping( 0x1B, KeyCode.Escape );
        AddMapping( 0xBB, KeyCode.Plus );
        AddMapping( 0xBD, KeyCode.Minus ); 
        AddMapping( 0x20, KeyCode.Space );
        AddMapping( 0x09, KeyCode.Tab );

        AddMapping( 0xDD, KeyCode.RightBracket ); // ]
        AddMapping( 0xDB, KeyCode.LeftBracket );  // [
        AddMapping( 0xBA, KeyCode.Semicolon );    // ;
        AddMapping( 0xDE, KeyCode.Quote );        // '
        AddMapping( 0xBC, KeyCode.Comma );        // ,
        AddMapping( 0xBE, KeyCode.Period );       // .
        AddMapping( 0xBF, KeyCode.Slash );        // /

        AddMapping( 0xA0, KeyCode.LeftShift );  
        AddMapping( 0xA2, KeyCode.LeftControl );  
        AddMapping( 0xA4, KeyCode.LeftAlt );      
        AddMapping( 0xA1, KeyCode.RightShift ); 
        AddMapping( 0x19, KeyCode.RightControl ); // 한자
        AddMapping( 0x15, KeyCode.RightAlt );     // 한영
        
        AddMapping( 0x2E, KeyCode.Delete );
        AddMapping( 0x2D, KeyCode.Insert );
        AddMapping( 0x24, KeyCode.Home );
        AddMapping( 0x23, KeyCode.End );
        AddMapping( 0x21, KeyCode.PageUp );
        AddMapping( 0x22, KeyCode.PageDown );
        
        AddMapping( 0x25, KeyCode.LeftArrow );
        AddMapping( 0x26, KeyCode.UpArrow );
        AddMapping( 0x27, KeyCode.RightArrow );
        AddMapping( 0x28, KeyCode.DownArrow );

        //// 펑션키
        //for ( int i = 0; i < 12; i++ )
        //{
        //    int vKey = 0x70 + i;       // F1~F12
        //    KeyCode uKey = KeyCode.F1 + i;
        //    AddMapping( vKey, uKey );
        //}

        // 넘버패드
        AddMapping( 0x60, KeyCode.Keypad0 );
        AddMapping( 0x61, KeyCode.Keypad1 );
        AddMapping( 0x62, KeyCode.Keypad2 );
        AddMapping( 0x63, KeyCode.Keypad3 );
        AddMapping( 0x64, KeyCode.Keypad4 );
        AddMapping( 0x65, KeyCode.Keypad5 );
        AddMapping( 0x66, KeyCode.Keypad6 );
        AddMapping( 0x67, KeyCode.Keypad7 );
        AddMapping( 0x68, KeyCode.Keypad8 );
        AddMapping( 0x69, KeyCode.Keypad9 );
        AddMapping( 0x6A, KeyCode.KeypadMultiply );
        AddMapping( 0x6B, KeyCode.KeypadPlus );
        AddMapping( 0x6D, KeyCode.KeypadMinus );
        AddMapping( 0x6E, KeyCode.KeypadPeriod );
        AddMapping( 0x6F, KeyCode.KeypadDivide );
    }

    private void StringMapping()
    {
        unityToString.Add( KeyCode.CapsLock,     "CapsLock" );
        unityToString.Add( KeyCode.Space,        "Space" );
        unityToString.Add( KeyCode.LeftShift,    "LShift" );
        unityToString.Add( KeyCode.LeftAlt,      "LAlt" );
        unityToString.Add( KeyCode.LeftControl,  "LCtrl" );
        unityToString.Add( KeyCode.RightShift,   "RShift" );
        unityToString.Add( KeyCode.RightAlt,     "RAlt" );
        unityToString.Add( KeyCode.RightControl, "RCtrl" );

        unityToString.Add( KeyCode.BackQuote, "`" );
        unityToString.Add( KeyCode.Alpha0, "0" );
        unityToString.Add( KeyCode.Alpha1, "1" );
        unityToString.Add( KeyCode.Alpha2, "2" );
        unityToString.Add( KeyCode.Alpha3, "3" );
        unityToString.Add( KeyCode.Alpha4, "4" );
        unityToString.Add( KeyCode.Alpha5, "5" );
        unityToString.Add( KeyCode.Alpha6, "6" );
        unityToString.Add( KeyCode.Alpha7, "7" );
        unityToString.Add( KeyCode.Alpha8, "8" );
        unityToString.Add( KeyCode.Alpha9, "9" );
        unityToString.Add( KeyCode.Minus,  "-" );
        unityToString.Add( KeyCode.Equals, "=" );

        unityToString.Add( KeyCode.Keypad0,        "Pad 0" );
        unityToString.Add( KeyCode.Keypad1,        "Pad 1" );
        unityToString.Add( KeyCode.Keypad2,        "Pad 2" );
        unityToString.Add( KeyCode.Keypad3,        "Pad 3" );
        unityToString.Add( KeyCode.Keypad4,        "Pad 4" );
        unityToString.Add( KeyCode.Keypad5,        "Pad 5" );
        unityToString.Add( KeyCode.Keypad6,        "Pad 6" );
        unityToString.Add( KeyCode.Keypad7,        "Pad 7" );
        unityToString.Add( KeyCode.Keypad8,        "Pad 8" );
        unityToString.Add( KeyCode.Keypad9,        "Pad 9" );
        unityToString.Add( KeyCode.KeypadPeriod,   "Pad ." );
        unityToString.Add( KeyCode.KeypadDivide,   "Pad /" );
        unityToString.Add( KeyCode.KeypadMultiply, "Pad *" );
        unityToString.Add( KeyCode.KeypadMinus,    "Pad -" );
        unityToString.Add( KeyCode.KeypadPlus,     "Pad +" );
        unityToString.Add( KeyCode.KeypadEquals,   "Pad =" );

        unityToString.Add( KeyCode.Q,            "Q" );
        unityToString.Add( KeyCode.W,            "W" );
        unityToString.Add( KeyCode.E,            "E" );
        unityToString.Add( KeyCode.R,            "R" );
        unityToString.Add( KeyCode.T,            "T" );
        unityToString.Add( KeyCode.Y,            "Y" );
        unityToString.Add( KeyCode.U,            "U" );
        unityToString.Add( KeyCode.I,            "I" );
        unityToString.Add( KeyCode.O,            "O" );
        unityToString.Add( KeyCode.P,            "P" );
        unityToString.Add( KeyCode.LeftBracket,  "[" );
        unityToString.Add( KeyCode.RightBracket, "]" );
        unityToString.Add( KeyCode.Backslash,    "\\" );

        unityToString.Add( KeyCode.A,         "A" );
        unityToString.Add( KeyCode.S,         "S" );
        unityToString.Add( KeyCode.D,         "D" );
        unityToString.Add( KeyCode.F,         "F" );
        unityToString.Add( KeyCode.G,         "G" );
        unityToString.Add( KeyCode.H,         "H" );
        unityToString.Add( KeyCode.J,         "J" );
        unityToString.Add( KeyCode.K,         "K" );
        unityToString.Add( KeyCode.L,         "L" );
        unityToString.Add( KeyCode.Semicolon, ";" );
        unityToString.Add( KeyCode.Quote,     "\'" );

        unityToString.Add( KeyCode.Z,      "Z" );
        unityToString.Add( KeyCode.X,      "X" );
        unityToString.Add( KeyCode.C,      "C" );
        unityToString.Add( KeyCode.V,      "V" );
        unityToString.Add( KeyCode.B,      "B" );
        unityToString.Add( KeyCode.N,      "N" );
        unityToString.Add( KeyCode.M,      "M" );
        unityToString.Add( KeyCode.Comma,  "," );
        unityToString.Add( KeyCode.Period, "." );
        unityToString.Add( KeyCode.Slash,  "/" );

        unityToString.Add( KeyCode.Home,     "Home" );
        unityToString.Add( KeyCode.Insert,   "Insert" );
        unityToString.Add( KeyCode.PageUp,   "PgUp" );
        unityToString.Add( KeyCode.Delete,   "Delete" );
        unityToString.Add( KeyCode.End,      "End" );
        unityToString.Add( KeyCode.PageDown, "PgDn" );
    }
    #endregion
}
