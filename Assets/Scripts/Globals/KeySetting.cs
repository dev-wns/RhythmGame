using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum KeyAction : int
{
    _0, _1, _2, _3, _4, _5, // InGame Input Keys
    _ScrollUp, _ScrollDown,
    _Esc,
};

[System.Serializable]
public static class KEY
{
    public static Dictionary<KeyAction, KeyCode> Keys = new Dictionary<KeyAction, KeyCode>();
}
public class KeySetting : MonoBehaviour
{
    
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
            KEY.Keys.Add( ( KeyAction )i, defaultKeys[i] );
        }
    }
}
