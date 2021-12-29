using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GAME_KEY_ACTION : int
{
    _0, _1, _2, _3, _4, _5, // InGame Input Keys
    SCROLL_UP, SCROLL_DOWN,
};

public class GlobalKeySetting : SingletonUnity<GlobalKeySetting>
{
    public Dictionary<GAME_KEY_ACTION, KeyCode> Keys = new Dictionary<GAME_KEY_ACTION, KeyCode>();


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
            Keys.Add( ( GAME_KEY_ACTION )i, defaultKeys[i] );
        }
    }
}
