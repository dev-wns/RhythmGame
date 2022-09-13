using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeySetting : SingletonUnity<KeySetting>
{
    public Dictionary<KeyCode, string/*keyCode to string*/> AvailableKeys = new Dictionary<KeyCode, string>();
    public Dictionary<GameKeyAction, KeyCode> Keys = new Dictionary<GameKeyAction, KeyCode>();
    private readonly KeyCode[] defaultKeys = new KeyCode[]
    {
        KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.L, KeyCode.Semicolon, KeyCode.Quote,
    };

    public bool IsAvailableKey( KeyCode _key ) => AvailableKeys.ContainsKey( _key );

    protected override void Awake()
    {
        base.Awake();

        AvailableKeyBind();
        for ( int i = 0; i < defaultKeys.Length; i++ )
        {
            Keys.Add( ( GameKeyAction )i, defaultKeys[i] );
        }
    }

    public string KeyCodeToString( KeyCode _code ) =>
           AvailableKeys.ContainsKey( _code ) ? AvailableKeys[_code] : "None";

    private void AvailableKeyBind()
    {
        AvailableKeys.Add( KeyCode.Numlock, "Numlock" );

        AvailableKeys.Add( KeyCode.LeftShift, "LeftShift" );
        AvailableKeys.Add( KeyCode.LeftAlt, "LeftAlt" );
        AvailableKeys.Add( KeyCode.LeftControl, "LeftCtrl" );

        AvailableKeys.Add( KeyCode.RightShift, "RightShift" );
        AvailableKeys.Add( KeyCode.RightAlt, "RightAlt" );
        AvailableKeys.Add( KeyCode.RightControl, "RightCtrl" );

        AvailableKeys.Add( KeyCode.BackQuote, "`" );
        AvailableKeys.Add( KeyCode.Alpha0, "0" );
        AvailableKeys.Add( KeyCode.Alpha1, "1" );
        AvailableKeys.Add( KeyCode.Alpha2, "2" );
        AvailableKeys.Add( KeyCode.Alpha3, "3" );
        AvailableKeys.Add( KeyCode.Alpha4, "4" );
        AvailableKeys.Add( KeyCode.Alpha5, "5" );
        AvailableKeys.Add( KeyCode.Alpha6, "6" );
        AvailableKeys.Add( KeyCode.Alpha7, "7" );
        AvailableKeys.Add( KeyCode.Alpha8, "8" );
        AvailableKeys.Add( KeyCode.Alpha9, "9" );
        AvailableKeys.Add( KeyCode.Minus, "-" );
        AvailableKeys.Add( KeyCode.Equals, "=" );

        AvailableKeys.Add( KeyCode.Keypad0, "Pad 0" );
        AvailableKeys.Add( KeyCode.Keypad1, "Pad 1" );
        AvailableKeys.Add( KeyCode.Keypad2, "Pad 2" );
        AvailableKeys.Add( KeyCode.Keypad3, "Pad 3" );
        AvailableKeys.Add( KeyCode.Keypad4, "Pad 4" );
        AvailableKeys.Add( KeyCode.Keypad5, "Pad 5" );
        AvailableKeys.Add( KeyCode.Keypad6, "Pad 6" );
        AvailableKeys.Add( KeyCode.Keypad7, "Pad 7" );
        AvailableKeys.Add( KeyCode.Keypad8, "Pad 8" );
        AvailableKeys.Add( KeyCode.Keypad9, "Pad 9" );
        AvailableKeys.Add( KeyCode.KeypadPeriod, "Pad ." );
        AvailableKeys.Add( KeyCode.KeypadDivide, "Pad /" );
        AvailableKeys.Add( KeyCode.KeypadMultiply, "Pad *" );
        AvailableKeys.Add( KeyCode.KeypadMinus, "Pad -" );
        AvailableKeys.Add( KeyCode.KeypadPlus, "Pad +" );
        AvailableKeys.Add( KeyCode.KeypadEquals, "Pad =" );

        AvailableKeys.Add( KeyCode.Q, "Q" );
        AvailableKeys.Add( KeyCode.W, "W" );
        AvailableKeys.Add( KeyCode.E, "E" );
        AvailableKeys.Add( KeyCode.R, "R" );
        AvailableKeys.Add( KeyCode.T, "T" );
        AvailableKeys.Add( KeyCode.Y, "Y" );
        AvailableKeys.Add( KeyCode.U, "U" );
        AvailableKeys.Add( KeyCode.I, "I" );
        AvailableKeys.Add( KeyCode.O, "O" );
        AvailableKeys.Add( KeyCode.P, "P" );
        AvailableKeys.Add( KeyCode.LeftBracket, "[" );
        AvailableKeys.Add( KeyCode.RightBracket, "]" );
        AvailableKeys.Add( KeyCode.Backslash, "\\" );

        AvailableKeys.Add( KeyCode.A, "A" );
        AvailableKeys.Add( KeyCode.S, "S" );
        AvailableKeys.Add( KeyCode.D, "D" );
        AvailableKeys.Add( KeyCode.F, "F" );
        AvailableKeys.Add( KeyCode.G, "G" );
        AvailableKeys.Add( KeyCode.H, "H" );
        AvailableKeys.Add( KeyCode.J, "J" );
        AvailableKeys.Add( KeyCode.K, "K" );
        AvailableKeys.Add( KeyCode.L, "L" );
        AvailableKeys.Add( KeyCode.Semicolon, ";" );
        AvailableKeys.Add( KeyCode.Quote, "\'" );

        AvailableKeys.Add( KeyCode.Z, "Z" );
        AvailableKeys.Add( KeyCode.X, "X" );
        AvailableKeys.Add( KeyCode.C, "C" );
        AvailableKeys.Add( KeyCode.V, "V" );
        AvailableKeys.Add( KeyCode.B, "B" );
        AvailableKeys.Add( KeyCode.N, "N" );
        AvailableKeys.Add( KeyCode.M, "M" );
        AvailableKeys.Add( KeyCode.Comma, "," );
        AvailableKeys.Add( KeyCode.Period, "." );
        AvailableKeys.Add( KeyCode.Slash, "/" );

        AvailableKeys.Add( KeyCode.Home, "Home" );
        AvailableKeys.Add( KeyCode.Insert, "Insert" );
        AvailableKeys.Add( KeyCode.PageUp, "PageUp" );
        AvailableKeys.Add( KeyCode.Delete, "Delete" );
        AvailableKeys.Add( KeyCode.End, "End" );
        AvailableKeys.Add( KeyCode.PageDown, "PageDown" );
    }
}
