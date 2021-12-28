using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum KeyAction : int
{
    _0, _1, _2, _3, _4, _5, // InGame Input Keys
    _ScrollUp, _ScrollDown,
    _Esc,
};

public class GlobalKeySetting : SingletonUnity<GlobalKeySetting>
{
    public Dictionary<KeyAction, KeyCode> Keys = new Dictionary<KeyAction, KeyCode>();


    private KeyCode[] defaultKeys = new KeyCode[]
    {
        KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.K, KeyCode.L, KeyCode.Semicolon,
        KeyCode.Alpha1, KeyCode.Alpha2,
        KeyCode.Escape
    };

    private void Awake()
    {
        for ( int i = 0; i < defaultKeys.Length; i++ )
        {
            Keys.Add( ( KeyAction )i, defaultKeys[i] );
        }
    }
}
