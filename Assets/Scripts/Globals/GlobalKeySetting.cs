using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameKeyAction : int
{
    _0, _1, _2, _3, _4, _5, Count // InGame Input Keys
};

public class GlobalKeySetting : SingletonUnity<GlobalKeySetting>
{
    public Dictionary<GameKeyAction, KeyCode> Keys = new Dictionary<GameKeyAction, KeyCode>();


    private KeyCode[] defaultKeys = new KeyCode[]
    {
        KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.K, KeyCode.L, KeyCode.Semicolon,
        KeyCode.Alpha1, KeyCode.Alpha2,
        KeyCode.Escape
    };

    private void Awake()
    {
        for ( int i = 0; i < defaultKeys.Length; i++ )
        {
            Keys.Add( ( GameKeyAction )i, defaultKeys[i] );
        }
    }
}
